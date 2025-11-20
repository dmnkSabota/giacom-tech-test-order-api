# Order Management API

REST API for order management built with .NET 8, Entity Framework Core, and MySQL.

**Dominik Sabota** | Giacom Tech Test

---

## Quick Start

```bash
docker-compose up
```

API runs on `http://localhost:8000` with seeded sample data.

---

## Features

✅ **Task 1:** Get orders by status - `GET /orders/status/{statusName}`  
✅ **Task 2:** Update order status - `PUT /orders/{orderId}/status`  
✅ **Task 3:** Create orders - `POST /orders`  
✅ **Task 4:** Monthly profit calculation - `GET /orders/profit/monthly`

**Testing:** 22 unit tests, all passing.

---

## API Examples

### Create Order
```bash
POST /orders
{
  "resellerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "customerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "items": [
    {"productId": "3fa85f64-5717-4562-b3fc-2c963f66afa6", "quantity": 5}
  ]
}
```

### Filter by Status
```bash
GET /orders/status/completed
```

### Update Status
```bash
PUT /orders/{orderId}/status
{
  "statusName": "In Progress"
}
```

### Monthly Profit
```bash
GET /orders/profit/monthly
```

Returns profit grouped by month for completed orders only.


---

## Tech Stack

- .NET 8
- ASP.NET Core Web API
- Entity Framework Core 6
- MySQL 5.7
- NUnit (testing)
- Docker

---

## Running Tests

```bash
cd src
dotnet test
```

All 22 tests use in-memory SQLite for fast execution.

---

## Local Development

**With Docker (recommended):**
```bash
docker-compose up
```

**Without Docker:**
```bash
cd src
dotnet restore
dotnet run --project Order.WebAPI
```

Update connection string in `appsettings.Development.json` for local MySQL.

---

## Database

MySQL with binary GUIDs (16 bytes vs 36). 100 sample orders seed automatically.

**Tables:** `order`, `order_item`, `order_product`, `order_service`, `order_status`

---

## Key Decisions

**Layered architecture** - Separation of concerns, easy to test and modify  
**Repository pattern** - Abstracts data access, enables testing with mocks  
**Async/await** - All I/O operations are non-blocking for better scalability  
**Duplicate validation** - Prevents same product appearing twice in one order (UX + data quality)

---

## Production Considerations

If deploying to production, add:
- Pagination (for large result sets)
- Logging (structured logging with Serilog)
- Authentication/Authorization (JWT tokens)
- Health checks (`/health` endpoint)
- API versioning
- Rate limiting

Currently optimized for demonstration, not production scale.
