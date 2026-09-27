using Microsoft.EntityFrameworkCore;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase("portfolio"));
var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
Seed(app);

app.MapGet("/api/orders/{id:int}", async (int id, AppDbContext db) => {
    var order = await db.Orders.FindAsync(id);
    return order is null ? Results.NotFound(new { error = "Order not found" }) : Results.Ok(order);
});

app.MapGet("/api/orders", async (AppDbContext db) => Results.Ok(await db.Orders.AsNoTracking().OrderByDescending(x => x.CreatedAt).ToListAsync()));

app.MapPost("/api/orders/{id:int}/refund", async (int id, AppDbContext db) => {
    var order = await db.Orders.FindAsync(id);
    if (order is null) return Results.NotFound(new { error = "Order not found" });
    if (order.Status == "Refunded") return Results.Ok(order); // idempotent retry
    if (order.Status != "Paid") return Results.BadRequest(new { error = "Only paid orders can be refunded" });
    order.Status = "Refunded";
    order.UpdatedAt = DateTime.UtcNow;
    await db.SaveChangesAsync();
    return Results.Ok(order);
});

app.MapGet("/api/search", async (string? customer, AppDbContext db) => {
    if (string.IsNullOrWhiteSpace(customer)) return Results.BadRequest(new { error = "customer is required" });
    var q = customer.Trim();
    // Production SQL Server version: use a normalized indexed column or provider-specific
    // case-insensitive comparison rather than calling ToLower() on the indexed column.
    var matches = await db.Orders.AsNoTracking().Where(x => x.CustomerNormalized.Contains(q.ToUpper())).ToListAsync();
    return Results.Ok(matches);
});

app.MapGet("/api/health", () => Results.Ok(new { status = "healthy", utc = DateTime.UtcNow }));
app.Run();

static void Seed(WebApplication app)
{
    using var s = app.Services.CreateScope();
    var db = s.ServiceProvider.GetRequiredService<AppDbContext>();

    if (!db.Orders.Any())
    {
        db.Orders.AddRange(
            new Order { Customer = "Asha", Total = 1499, Status = "Paid" },
            new Order { Customer = "Ravi", Total = 799, Status = "Pending" },
            new Order { Customer = "Neha", Total = 2499, Status = "Paid" }
        );
    }

    foreach (var order in db.Orders.Local)
    {
        order.CustomerNormalized = order.Customer.Trim().ToUpperInvariant();
    }

    db.SaveChanges();
}

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options) { public DbSet<Order> Orders => Set<Order>(); protected override void OnModelCreating(ModelBuilder b){b.Entity<Order>().Property(x=>x.Total).HasPrecision(18,2); b.Entity<Order>().Property(x=>x.CustomerNormalized).HasMaxLength(200).IsRequired();} }
public class Order { 
    public int Id{get;set;} 
    public string Customer{get;set;}=""; 
    public string CustomerNormalized{get;set;}=""; 
    public decimal Total{get;set;} 
    public string Status{get;set;}="Pending"; 
    public DateTime CreatedAt{get;set;}=DateTime.UtcNow; 
    public DateTime UpdatedAt{get;set;}=DateTime.UtcNow; }
