# ASP.NET Core Production Bug Fix Lab

A production-style ASP.NET Core Web API built with C# and .NET 8.

This project demonstrates practical backend development concepts including REST APIs, Entity Framework Core, business-rule validation, idempotent operations, customer search, Swagger/OpenAPI documentation, health checks, debugging, automated integration testing, and Git-based version control.

---

## 🚀 Tech Stack

- C#
- .NET 8
- ASP.NET Core Minimal APIs
- Entity Framework Core
- EF Core InMemory Database
- Swagger / OpenAPI
- xUnit
- ASP.NET Core WebApplicationFactory
- Git
- GitHub

---

## 📌 Features

### Order Management

- Get all orders
- Get an order by ID
- Seed demo order data
- Track order status
- Handle missing orders with appropriate HTTP status codes

### Refund Workflow

- Refund only `Paid` orders
- Reject refunds for `Pending` orders
- Handle already-refunded orders safely
- Prevent repeated refund processing
- Demonstrate idempotent API behavior

### Customer Search

- Search orders by customer name
- Case-insensitive matching
- Customer name normalization
- Normalized search field for reliable matching

### Health Check

- API health endpoint
- Returns API status
- Returns UTC timestamp

### API Documentation

- Swagger/OpenAPI integration
- Interactive API testing
- Documented HTTP endpoints

---

## 🔌 API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/orders` | Get all orders |
| GET | `/api/orders/{id}` | Get an order by ID |
| POST | `/api/orders/{id}/refund` | Refund a paid order |
| GET | `/api/search?customer=Ravi` | Search orders by customer |
| GET | `/api/health` | Check API health |

---

## 🧪 API Examples

### 1. Get All Orders

```http
GET /api/orders

Example response:

[
  {
    "id": 1,
    "customer": "Asha",
    "customerNormalized": "ASHA",
    "total": 1499,
    "status": "Paid"
  },
  {
    "id": 2,
    "customer": "Ravi",
    "customerNormalized": "RAVI",
    "total": 799,
    "status": "Pending"
  },
  {
    "id": 3,
    "customer": "Neha",
    "customerNormalized": "NEHA",
    "total": 2499,
    "status": "Paid"
  }
]


2. Get Order By ID
GET /api/orders/1

Returns a single order when the ID exists.

If the order does not exist:

404 Not Found

Example error:

{
  "error": "Order not found"
}


3. Customer Search
GET /api/search?customer=Ravi

The search supports case-insensitive customer matching.

The following searches can match the same normalized customer:

Ravi
ravi
RAVI

Example response:

[
  {
    "id": 2,
    "customer": "Ravi",
    "customerNormalized": "RAVI",
    "total": 799,
    "status": "Pending"
  }
]


4. Refund a Paid Order
POST /api/orders/1/refund

Successful refund:

Paid → Refunded

Response:

200 OK

Example response:

{
  "id": 1,
  "customer": "Asha",
  "customerNormalized": "ASHA",
  "total": 1499,
  "status": "Refunded"
}
5. Invalid Refund

A pending order cannot be refunded.

Example:

Pending → Refund Request

Response:

400 Bad Request

Example error:

{
  "error": "Only paid orders can be refunded"
}
6. Repeated Refund Request

If an order is already refunded, sending the same refund request again returns the existing refunded state instead of processing the refund again.

Example:

First request:

Paid → Refunded

Second request:

Refunded → Refunded

Response:

200 OK

This demonstrates idempotent behavior at the API and business-logic level.

7. Health Check
GET /api/health

Example response:

{
  "status": "healthy",
  "utc": "2026-09-27T17:43:46Z"
}

🧪 Automated Testing
The project includes automated API integration tests using xUnit and ASP.NET Core WebApplicationFactory.

Test Coverage

The test suite covers:

Health endpoint returns 200 OK
Existing order retrieval
Non-existing order returns 404 Not Found
Successful refund of a paid order
Duplicate refund handling
Customer search
Case-insensitive customer search
Invalid search request returns 400 Bad Request
Pending order refund rejection
Run Tests

From the project root:

dotnet test tests/Portfolio.Api.Tests/Portfolio.Api.Tests.csproj

Current test result:

10 passed
0 failed
Testing Approach
xUnit
  │
  ▼
WebApplicationFactory
  │
  ▼
ASP.NET Core API
  │
  ▼
HTTP Requests
  │
  ▼
Assertions

This provides automated verification of API behavior and business rules instead of relying only on manual Swagger testing.

🐛 Bug Fix Demonstration

During development, the customer search endpoint initially returned an empty result even when a matching customer existed.

Problem

Customer names were stored correctly, but the normalized search field was empty for existing records.

Customer = Ravi

CustomerNormalized = ""

Because the search endpoint used the normalized field, this request returned an empty result:

GET /api/search?customer=Ravi

Response:

[]


Root Cause
The normalization logic was inside a conditional seed block that only executed when the database had no existing orders.

if (!db.Orders.Any())
{
    // seed orders
}

When orders already existed, the normalization code was skipped.

Fix

The normalization logic was moved outside the initial seed condition and applied to the tracked local orders:

foreach (var order in db.Orders.Local)
{
    order.CustomerNormalized =
        order.Customer.Trim().ToUpperInvariant();
}

db.SaveChanges();

After the fix:

Customer = Ravi

CustomerNormalized = RAVI

The search endpoint then correctly returned matching orders.

This demonstrates a practical debugging workflow rather than only implementing a happy-path API.


🔐 Business Rules

The refund endpoint implements the following rules:

Order Not Found
Order does not exist
        ↓
404 Not Found
Already Refunded
Refunded
   ↓
Refund request
   ↓
200 OK


The existing state is returned without processing another refund operation.

Pending Order
Pending
   ↓
Refund request
   ↓
400 Bad Request
Paid Order
Paid
   ↓
Refund request
   ↓
Refunded
   ↓
200 OK


🧠 Production Concepts Demonstrated
This project focuses on practical backend engineering concepts:
REST API design
ASP.NET Core Minimal APIs
HTTP status codes
Business-rule validation
Idempotent operations
Data normalization
Entity Framework Core
Database access
Swagger/OpenAPI
Health checks
Error handling
API testing
Integration testing
Debugging
Git version control
GitHub workflow


🛠️ Run Locally
1. Clone the repository
git clone https://github.com/amitchouhan12/portfolio-1.git
2. Navigate to the API project
cd portfolio-1/src/Portfolio.Api
3. Restore dependencies
dotnet restore
4. Run the application
dotnet run
5. Open Swagger
http://localhost:5000/swagger

Swagger provides an interactive interface for testing all available API endpoints.

🔄 Development Workflow
The project follows a practical backend development workflow:

Implement
   ↓
Run API
   ↓
Test with Swagger
   ↓
Identify incorrect behavior
   ↓
Debug
   ↓
Fix
   ↓
Retest
   ↓
Write automated tests
   ↓
Commit
   ↓
Push to GitHub


📋 Testing Scenarios

The API was tested for:
Successful order retrieval
Order not found
Successful refund
Refund of a pending order
Repeated refund request
Customer search
Case-insensitive customer search
Health check
Swagger endpoint execution
HTTP status code handling
Data normalization
Automated integration testing


📂 Project Structure
portfolio-1/
│
├── src/
│   └── Portfolio.Api/
│       ├── Program.cs
│       └── Portfolio.Api.csproj
│
├── tests/
│   └── Portfolio.Api.Tests/
│       ├── ApiIntegrationTests.cs
│       └── Portfolio.Api.Tests.csproj
│
├── docs/
├── .gitignore
└── README.md


📈 Current API Flow
Client
  │
  ▼
ASP.NET Core API
  │
  ├── Orders
  │     ├── Get All
  │     └── Get By ID
  │
  ├── Refund
  │     ├── Validate Status
  │     ├── Refund Paid Order
  │     └── Handle Repeated Request
  │
  ├── Search
  │     └── Normalized Customer Matching
  │
  └── Health Check
        │
        ▼
   EF Core InMemory DB

🔮 Future Improvements
Potential production-oriented improvements include:
SQL Server database
Entity Framework Core migrations
Repository/service architecture
DTOs
FluentValidation or built-in validation
Authentication and authorization
Structured logging
Global exception handling
Docker support
CI/CD pipeline
Real payment gateway integration
Database-backed idempotency keys
Pagination and filtering
API versioning
Rate limiting
Production monitoring

🎯 Project Purpose
This project was created as a practical .NET backend portfolio project.

The goal is to demonstrate the ability to:
Build REST APIs
Work with Entity Framework Core
Implement business rules
Debug API issues
Handle edge cases
Design idempotent operations
Normalize and search data
Document APIs
Write automated integration tests
Use Git and GitHub for version control

👨‍💻 Author
Amit Chouhan
.NET Developer | C# | ASP.NET Core | SQL Server | REST APIs

📄 License
This project is created for portfolio and learning purposes.
