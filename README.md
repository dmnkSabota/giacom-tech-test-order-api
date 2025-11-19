# Order API - Giacom Tech Test

Tech test submission for Giacom - REST API for order management.

**Dominik Sabota**

## Tasks Completed

- Task 1: Get orders by status endpoint - ** GET `/orders/status/{statusName}`
- Task 2: Update order status endpoint - ** PUT `/orders/{orderId}/status` 
- Task 3: Create new orders ** POST `/orders` 
- Task 4: Monthly profit calculation ** GET `/orders/profit/monthly`


All 21 unit tests passing.

## Run
```bash
docker-compose up
```

API available at `http://localhost:8000`

### Example: Create Order
```bash
POST /orders
{
  "resellerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "customerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "items": [
    { "productId": "3fa85f64-5717-4562-b3fc-2c963f66afa6", "quantity": 5 }
  ]
}
```

Submitted for Giacom technical assessment