using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Portfolio.Api.Tests;

public class ApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Health_Returns_Healthy()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetOrder_ExistingOrder_ReturnsCorrectOrder()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/orders/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();

        Assert.Contains("\"customer\":\"Asha\"", json);
        Assert.Contains("\"total\":1499", json);
        Assert.Contains("\"status\":\"Paid\"", json);
    }

    [Fact]
    public async Task GetOrder_NonExistingOrder_ReturnsNotFound()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/orders/999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task RefundOrder_PaidOrder_ReturnsRefunded()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsync("/api/orders/3/refund", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();

        Assert.Contains("\"status\":\"Refunded\"", json);
    }

    [Fact]
    public async Task RefundOrder_AlreadyRefunded_ReturnsOk()
    {
        var client = _factory.CreateClient();

        // First refund
        var firstResponse = await client.PostAsync("/api/orders/3/refund", null);
        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);

        // Second refund
        var secondResponse = await client.PostAsync("/api/orders/3/refund", null);
        Assert.Equal(HttpStatusCode.OK, secondResponse.StatusCode);

        var json = await secondResponse.Content.ReadAsStringAsync();

        Assert.Contains("\"status\":\"Refunded\"", json);
    }

[Fact]
public async Task SearchOrder_ByCustomer_ReturnsMatchingOrder()
{
    var client = _factory.CreateClient();

    var response = await client.GetAsync("/api/search?customer=Ravi");

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    var json = await response.Content.ReadAsStringAsync();

    Assert.Contains("\"customer\":\"Ravi\"", json);
}

[Theory]
[InlineData("ravi")]
[InlineData("RAVI")]
public async Task SearchOrder_ByCustomer_IsCaseInsensitive(string customer)
{
    var client = _factory.CreateClient();

    var response = await client.GetAsync($"/api/search?customer={customer}");

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    var json = await response.Content.ReadAsStringAsync();

    Assert.Contains("\"customer\":\"Ravi\"", json);
}

[Fact]
public async Task SearchOrder_WithoutCustomer_ReturnsBadRequest()
{
    var client = _factory.CreateClient();

    var response = await client.GetAsync("/api/search");

    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
}

[Fact]
public async Task RefundOrder_PendingOrder_ReturnsBadRequest()
{
    var client = _factory.CreateClient();

    var response = await client.PostAsync("/api/orders/2/refund", null);

    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
}

}