# Order API Microservice - Giacom Technical Assessment

A RESTful API microservice for managing orders, built with .NET 8, Entity Framework Core, and PostgreSQL.

## 👤 Candidate Information
- **Name:** Dominik [Your Last Name]
- **Date:** November 2024
- **Position:** Junior Software Engineer

## 🎯 Completed Tasks

### ✅ Task 1: Get Orders by Status
**Endpoint:** `GET /orders/status/{statusName}`

Retrieves all orders filtered by their status (e.g., "Completed", "Pending", "Failed").

### ✅ Task 2: Update Order Status
**Endpoint:** `PUT /orders/{orderId}/status`

Updates the status of an existing order.

### ✅ Task 3: Create New Order
**Endpoint:** `POST /orders`

Creates a new order with multiple items, automatically setting initial status to "Created".

### ✅ Task 4: Calculate Monthly Profit
**Endpoint:** `GET /orders/profit/monthly`

Calculates total profit grouped by month for all completed orders.

## 🏗️ Architecture & Design Decisions

### Clean Architecture
- **Repository Pattern:** Separation of data access logic
- **Service Layer:** Business logic encapsulation
- **Dependency Injection:** Loose coupling, testable code
- **Async/Await:** Non-blocking I/O operations

### Key Technical Choices

**Database Handling:**
- Used conditional logic to support both PostgreSQL (production) and InMemory (testing)
- Handled byte array comparisons for Guid fields differently per provider

**Validation:**
- Implemented Data Annotations for request validation
- Controller-level validation with ModelState
- Repository-level business rule validation

**Testing Strategy:**
- 21 comprehensive unit tests covering all CRUD operations
- Arrange-Act-Assert pattern for clarity
- In-memory database for isolated test execution

**Code Quality:**
- Modern C# features (C# 12 collection expressions, target-typed new)
- XML documentation on all public APIs
- Descriptive test names following convention
- SonarQube compliant code

## 🚀 Getting Started

### Prerequisites
- .NET 8 SDK
- Docker & Docker Compose (for PostgreSQL)
- PostgreSQL (if running without Docker)

### Running with Docker (Recommended)
```bash
# Start PostgreSQL and API
docker-compose up

# API will be available at http://localhost:8000
```

### Running Locally
```bash
# Restore dependencies
dotnet restore

# Build solution
dotnet build

# Run tests
dotnet test

# Run API
cd src/OrderService.WebAPI
dotnet run
```

## 📝 API Documentation

### Base URL
```
http://localhost:8000
```

### Endpoints

#### Get All Orders
```http
GET /orders
```

#### Get Order by ID
```http
GET /orders/{orderId}
```

#### Get Orders by Status
```http
GET /orders/status/Completed
```

#### Create Order
```http
POST /orders
Content-Type: application/json

{
  "resellerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "customerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "items": [
    {
      "productId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "quantity": 5
    }
  ]
}
```

#### Update Order Status
```http
PUT /orders/{orderId}/status
Content-Type: application/json

{
  "statusName": "Completed"
}
```

#### Get Monthly Profit
```http
GET /orders/profit/monthly
```

## 🧪 Testing
```bash
# Run all tests
dotnet test

# Expected output: Total tests: 21, Passed: 21
```

### Test Coverage
- ✅ Existing functionality (6 tests)
- ✅ Task 1: Get orders by status (4 tests)
- ✅ Task 2: Update order status (3 tests)
- ✅ Task 3: Create order (5 tests)
- ✅ Task 4: Calculate monthly profit (5 tests)

## 📂 Project Structure
```
tech-test/
├── src/
│   ├── Order.Data/              # Repository & EF Core context
│   ├── Order.Model/             # DTOs and request/response models
│   ├── Order.Service/           # Business logic layer
│   └── OrderService.WebAPI/     # API controllers & startup
├── test/
│   └── Order.Service.Tests/     # Unit tests
├── docker-compose.yml           # Docker configuration
└── README.md
```

## 💡 Implementation Highlights

### Best Practices Applied
- ✅ SOLID principles
- ✅ RESTful API design
- ✅ Async/await throughout
- ✅ Comprehensive error handling
- ✅ Input validation
- ✅ Clean code principles
- ✅ Meaningful variable names
- ✅ XML documentation

### Performance Considerations
- Entity Framework query optimization
- Materialized collections to avoid multiple enumerations
- Indexed database fields
- Async operations for I/O-bound work

## 📧 Contact
For any questions regarding this implementation, please contact me at [your email].

---

**Submitted:** November 2024  
**Tech Test by:** Giacom