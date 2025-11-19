using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using NUnit.Framework;
using Order.Data;
using Order.Data.Entities;
using Order.Model;
using System;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace Order.Service.Tests
{
    /// <summary>
    /// Tests for OrderService covering all CRUD operations and business logic
    /// </summary>
    public class OrderServiceTests
    {
        
        private const decimal PRODUCT_UNIT_COST = 0.8m;
        private const decimal PRODUCT_UNIT_PRICE = 0.9m;
        private const decimal PRODUCT_PROFIT_PER_UNIT = PRODUCT_UNIT_PRICE - PRODUCT_UNIT_COST;
        
        private const string STATUS_CREATED = "Created";
        private const string STATUS_PENDING = "Pending";
        private const string STATUS_IN_PROGRESS = "InProgress";
        private const string STATUS_COMPLETED = "Completed";
        private const string STATUS_FAILED = "Failed";
        
        private const string PRODUCT_NAME = "100GB Mailbox";
        private const string SERVICE_NAME = "Email";
        
        private IOrderService _orderService;
        private IOrderRepository _orderRepository;
        private OrderContext _orderContext;
        private DbConnection _connection;

        private byte[] _statusCreatedId;
        private byte[] _statusPendingId;
        private byte[] _statusInProgressId;
        private byte[] _statusCompletedId;
        private byte[] _statusFailedId;
        
        private byte[] _serviceEmailId;
        private byte[] _productEmailId;

        [SetUp]
        public async Task Setup()
        {
            InitializeTestIds();
            await InitializeDatabaseAsync();
            InitializeServices();
            await SeedReferenceDataAsync();
        }

        [TearDown]
        public void TearDown()
        {
            _connection?.Dispose();
            _orderContext?.Dispose();
        }

        private void InitializeTestIds()
        {
            _statusCreatedId = Guid.NewGuid().ToByteArray();
            _statusPendingId = Guid.NewGuid().ToByteArray();
            _statusInProgressId = Guid.NewGuid().ToByteArray();
            _statusCompletedId = Guid.NewGuid().ToByteArray();
            _statusFailedId = Guid.NewGuid().ToByteArray();
            _serviceEmailId = Guid.NewGuid().ToByteArray();
            _productEmailId = Guid.NewGuid().ToByteArray();
        }

        private async Task InitializeDatabaseAsync()
        {
            var options = new DbContextOptionsBuilder<OrderContext>()
                .UseSqlite(CreateInMemoryDatabase())
                .EnableDetailedErrors()
                .EnableSensitiveDataLogging()
                .Options;

            _connection = RelationalOptionsExtension.Extract(options).Connection;
            _orderContext = new OrderContext(options);
            
            await _orderContext.Database.EnsureDeletedAsync();   
            await _orderContext.Database.EnsureCreatedAsync();
        }

        private void InitializeServices()
        {
            _orderRepository = new OrderRepository(_orderContext);
            _orderService = new OrderService(_orderRepository);
        }

        private static DbConnection CreateInMemoryDatabase()
        {
            var connection = new SqliteConnection("Filename=:memory:");
            connection.Open();
            return connection;
        }

        [Test]
        public async Task GetOrders_WhenThreeOrdersExist_ReturnsAllThreeOrders()
        {
            await CreateTestOrderAsync(quantity: 1);
            await CreateTestOrderAsync(quantity: 2);
            await CreateTestOrderAsync(quantity: 3);

            var orders = await _orderService.GetOrdersAsync();

            Assert.AreEqual(3, orders.Count(), "Should return all 3 orders");
        }

        [Test]
        public async Task GetOrders_WhenOrdersHaveDifferentQuantities_CalculatesTotalsCorrectly()
        {
            var order1Id = await CreateTestOrderAsync(quantity: 1);
            var order2Id = await CreateTestOrderAsync(quantity: 2);
            var order3Id = await CreateTestOrderAsync(quantity: 3);

            var orders = (await _orderService.GetOrdersAsync()).ToList();

            var order1 = orders.Single(x => x.Id == order1Id);
            var order2 = orders.Single(x => x.Id == order2Id);
            var order3 = orders.Single(x => x.Id == order3Id);

            AssertOrderTotals(order1, quantity: 1);
            AssertOrderTotals(order2, quantity: 2);
            AssertOrderTotals(order3, quantity: 3);
        }

        [Test]
        public async Task GetOrderById_WhenOrderExists_ReturnsCorrectOrder()
        {
            var orderId = await CreateTestOrderAsync(quantity: 1);

            var order = await _orderService.GetOrderByIdAsync(orderId);

            Assert.IsNotNull(order, "Order should be found");
            Assert.AreEqual(orderId, order.Id, "Should return order with matching ID");
        }

        [Test]
        public async Task GetOrderById_WhenOrderHasItems_ReturnsCorrectItemCount()
        {
            var orderId = await CreateTestOrderAsync(quantity: 5);

            var order = await _orderService.GetOrderByIdAsync(orderId);

            Assert.AreEqual(1, order.Items.Count(), "Order should have 1 item");
        }

        [Test]
        public async Task GetOrderById_WhenOrderHasItems_CalculatesTotalsCorrectly()
        {
            var orderId = await CreateTestOrderAsync(quantity: 2);

            var order = await _orderService.GetOrderByIdAsync(orderId);

            AssertOrderDetailTotals(order, quantity: 2);
        }

        [Test]
        public async Task GetOrdersByStatus_WhenFilteringByCompleted_ReturnsOnlyCompletedOrders()
        {
            await CreateTestOrderAsync(quantity: 1, statusId: _statusCompletedId);
            await CreateTestOrderAsync(quantity: 2, statusId: _statusCompletedId);
            await CreateTestOrderAsync(quantity: 1, statusId: _statusPendingId);
            await CreateTestOrderAsync(quantity: 1, statusId: _statusFailedId);

            var orders = (await _orderService.GetOrdersByStatusAsync(STATUS_COMPLETED)).ToList();  

            Assert.AreEqual(2, orders.Count, "Should return exactly 2 completed orders"); 
            Assert.IsTrue(orders.All(o => o.StatusName == STATUS_COMPLETED), 
                "All returned orders should have Completed status");
        }

        [Test]
        public async Task GetOrdersByStatus_WhenFilteringByFailed_ReturnsOnlyFailedOrders()
        {
            await CreateTestOrderAsync(quantity: 1, statusId: _statusFailedId);
            await CreateTestOrderAsync(quantity: 1, statusId: _statusCompletedId);
            await CreateTestOrderAsync(quantity: 1, statusId: _statusPendingId);

            var orders = (await _orderService.GetOrdersByStatusAsync(STATUS_FAILED)).ToList();

            Assert.AreEqual(1, orders.Count, "Should return exactly 1 failed order"); 
            Assert.AreEqual(STATUS_FAILED, orders[0].StatusName,
                "Returned order should have Failed status");
        }

        [Test]
        public async Task GetOrdersByStatus_WhenStatusDoesNotExist_ReturnsEmptyList()
        {
            await CreateTestOrderAsync(quantity: 1, statusId: _statusCompletedId);

            var orders = await _orderService.GetOrdersByStatusAsync("NonExistentStatus");

            Assert.AreEqual(0, orders.Count(), "Should return empty list for non-existent status");
        }

        [Test]
        public async Task GetOrdersByStatus_WhenUsingLowercase_IsCaseInsensitive()
        {
            await CreateTestOrderAsync(quantity: 1, statusId: _statusCompletedId);

            var orders = (await _orderService.GetOrdersByStatusAsync("completed")).ToList();

            Assert.AreEqual(1, orders.Count, "Should find order with case-insensitive search");
            Assert.AreEqual(STATUS_COMPLETED, orders[0].StatusName, 
                "Should return properly cased status name");
        }

        [Test]
        public async Task UpdateOrderStatus_WhenOrderExists_UpdatesStatusSuccessfully()
        {
            var orderId = await CreateTestOrderAsync(quantity: 1, statusId: _statusPendingId);

            var result = await _orderService.UpdateOrderStatusAsync(orderId, STATUS_IN_PROGRESS);

            Assert.IsTrue(result, "Update should succeed");
            
            var updatedOrder = await _orderService.GetOrderByIdAsync(orderId);
            Assert.AreEqual(STATUS_IN_PROGRESS, updatedOrder.StatusName, 
                "Order status should be updated to InProgress");
        }

        [Test]
        public async Task UpdateOrderStatus_WhenOrderDoesNotExist_ReturnsFalse()
        {
            var nonExistentOrderId = Guid.NewGuid();

            var result = await _orderService.UpdateOrderStatusAsync(nonExistentOrderId, STATUS_COMPLETED);

            Assert.IsFalse(result, "Update should fail for non-existent order");
        }

        [Test]
        public async Task UpdateOrderStatus_WhenStatusIsInvalid_ReturnsFalse()
        {
            var orderId = await CreateTestOrderAsync(quantity: 1, statusId: _statusPendingId);

            var result = await _orderService.UpdateOrderStatusAsync(orderId, "InvalidStatus");

            Assert.IsFalse(result, "Update should fail for invalid status");
        }

        [Test]
        public async Task CreateOrder_WhenRequestIsValid_CreatesOrderSuccessfully()
        {
            var request = BuildCreateOrderRequest(quantity: 5);

            var createdOrder = await _orderService.CreateOrderAsync(request);

            Assert.IsNotNull(createdOrder, "Created order should not be null");
            Assert.AreEqual(request.ResellerId, createdOrder.ResellerId, "ResellerId should match");
            Assert.AreEqual(request.CustomerId, createdOrder.CustomerId, "CustomerId should match");
            Assert.AreEqual(STATUS_CREATED, createdOrder.StatusName, "New orders should have Created status");
        }

        [Test]
        public async Task CreateOrder_WhenRequestHasMultipleItems_CreatesAllItems()
        {
            var request = BuildCreateOrderRequest(
                new[] { 5, 3 }
            );

            var createdOrder = await _orderService.CreateOrderAsync(request);

            Assert.AreEqual(2, createdOrder.Items.Count(), "Should create 2 order items");
        }

        [Test]
        public async Task CreateOrder_WhenRequestHasQuantity_SetsQuantityCorrectly()
        {
            var request = BuildCreateOrderRequest(quantity: 10);

            var createdOrder = await _orderService.CreateOrderAsync(request);

            Assert.AreEqual(10, createdOrder.Items.First().Quantity, "Quantity should be set correctly");
        }

        [Test]
        public async Task CreateOrder_WhenCreated_CalculatesTotalsCorrectly()
        {
            var request = BuildCreateOrderRequest(quantity: 10);

            var createdOrder = await _orderService.CreateOrderAsync(request);

            var expectedCost = 10 * PRODUCT_UNIT_COST;
            var expectedPrice = 10 * PRODUCT_UNIT_PRICE;
            
            Assert.AreEqual(expectedCost, createdOrder.TotalCost, "Total cost should be calculated correctly");
            Assert.AreEqual(expectedPrice, createdOrder.TotalPrice, "Total price should be calculated correctly");
        }

        [Test]
        public void CreateOrder_WhenProductDoesNotExist_ThrowsException()
        {
            var request = BuildCreateOrderRequest(
                productId: Guid.NewGuid(),
                quantity: 5
            );

            var exception = Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _orderService.CreateOrderAsync(request),
                "Should throw exception for non-existent product"
            );
    
            Assert.That(exception, Is.Not.Null, "Exception should be thrown");
            Assert.That(exception!.Message, Does.Contain("not found"), 
                "Exception message should indicate product not found");
        }
        
        [Test]
        public async Task CalculateMonthlyProfit_WhenOrdersExistInSameMonth_CalculatesProfitCorrectly()
        {
            await CreateTestOrderAsync(
                quantity: 10, 
                statusId: _statusCompletedId, 
                createdDate: new DateTime(2024, 11, 1, 0, 0, 0, DateTimeKind.Utc)
            );
            await CreateTestOrderAsync(
                quantity: 5, 
                statusId: _statusCompletedId, 
                createdDate: new DateTime(2024, 11, 15, 0, 0, 0, DateTimeKind.Utc)
            );

            var monthlyProfit = (await _orderService.CalculateMonthlyProfitAsync()).ToList();

            Assert.AreEqual(1, monthlyProfit.Count, "Should have 1 month of data");
            
            var november = monthlyProfit[0];
            Assert.AreEqual(2024, november.Year, "Year should be 2024");
            Assert.AreEqual(11, november.Month, "Month should be November (11)");
            Assert.AreEqual("November", november.MonthName, "Month name should be November");
            Assert.AreEqual(2, november.OrderCount, "Should count 2 orders");
            
            var expectedProfit = (10 + 5) * PRODUCT_PROFIT_PER_UNIT;
            Assert.AreEqual(expectedProfit, november.TotalProfit, "Total profit should be correct");
        }

        [Test]
        public async Task CalculateMonthlyProfit_WhenOrdersInDifferentMonths_GroupsByMonth()
        {
            await CreateTestOrderAsync(quantity: 10, statusId: _statusCompletedId, 
                createdDate: new DateTime(2024, 10, 1, 0, 0, 0, DateTimeKind.Utc));
            await CreateTestOrderAsync(quantity: 5, statusId: _statusCompletedId, 
                createdDate: new DateTime(2024, 11, 1, 0, 0, 0, DateTimeKind.Utc));
            await CreateTestOrderAsync(quantity: 3, statusId: _statusCompletedId, 
                createdDate: new DateTime(2024, 12, 1, 0, 0, 0, DateTimeKind.Utc));

            var monthlyProfit = (await _orderService.CalculateMonthlyProfitAsync()).ToList();

            Assert.AreEqual(3, monthlyProfit.Count, "Should have 3 months of data");
            Assert.AreEqual(10, monthlyProfit[0].Month, "First month should be October");
            Assert.AreEqual(11, monthlyProfit[1].Month, "Second month should be November");
            Assert.AreEqual(12, monthlyProfit[2].Month, "Third month should be December");
        }

        [Test]
        public async Task CalculateMonthlyProfit_WhenNonCompletedOrdersExist_IgnoresThem()
        {
            await CreateTestOrderAsync(quantity: 10, statusId: _statusCompletedId, 
                createdDate: new DateTime(2024, 11, 1, 0, 0, 0, DateTimeKind.Utc));
            await CreateTestOrderAsync(quantity: 5, statusId: _statusPendingId, 
                createdDate: new DateTime(2024, 11, 15, 0, 0, 0, DateTimeKind.Utc));
            await CreateTestOrderAsync(quantity: 3, statusId: _statusFailedId, 
                createdDate: new DateTime(2024, 11, 20, 0, 0, 0, DateTimeKind.Utc));

            var monthlyProfit = (await _orderService.CalculateMonthlyProfitAsync()).ToList();

            Assert.AreEqual(1, monthlyProfit.Count, "Should have 1 month of data");
            
            var november = monthlyProfit[0];
            Assert.AreEqual(1, november.OrderCount, "Should only count completed order");
            
            var expectedProfit = 10 * PRODUCT_PROFIT_PER_UNIT;
            Assert.AreEqual(expectedProfit, november.TotalProfit, 
                "Should only calculate profit from completed order");
        }

        [Test]
        public async Task CalculateMonthlyProfit_WhenNoCompletedOrders_ReturnsEmptyList()
        {
            await CreateTestOrderAsync(quantity: 10, statusId: _statusPendingId);
            await CreateTestOrderAsync(quantity: 5, statusId: _statusFailedId);

            var monthlyProfit = await _orderService.CalculateMonthlyProfitAsync();

            Assert.AreEqual(0, monthlyProfit.Count(), "Should return empty list when no completed orders");
        }

        [Test]
        public async Task CalculateMonthlyProfit_WhenOrdersOutOfOrder_SortsChronologically()
        {
            await CreateTestOrderAsync(quantity: 1, statusId: _statusCompletedId, 
                createdDate: new DateTime(2024, 12, 1, 0, 0, 0, DateTimeKind.Utc));
            await CreateTestOrderAsync(quantity: 1, statusId: _statusCompletedId, 
                createdDate: new DateTime(2024, 10, 1, 0, 0, 0, DateTimeKind.Utc));
            await CreateTestOrderAsync(quantity: 1, statusId: _statusCompletedId, 
                createdDate: new DateTime(2024, 11, 1, 0, 0, 0, DateTimeKind.Utc));

            var monthlyProfit = (await _orderService.CalculateMonthlyProfitAsync()).ToList();

            var months = monthlyProfit.Select(x => x.Month).ToList();
            Assert.AreEqual(new[] { 10, 11, 12 }, months, 
                "Months should be sorted chronologically (Oct, Nov, Dec)");
        }

        /// <summary>
        /// Creates a test order with the specified parameters
        /// </summary>
        private async Task<Guid> CreateTestOrderAsync(
            int quantity, 
            byte[] statusId = null, 
            DateTime? createdDate = null)
        {
            var orderId = Guid.NewGuid();
            var orderIdBytes = orderId.ToByteArray();
            
            _orderContext.Order.Add(new Data.Entities.Order
            {
                Id = orderIdBytes,
                ResellerId = Guid.NewGuid().ToByteArray(),
                CustomerId = Guid.NewGuid().ToByteArray(),
                CreatedDate = createdDate ?? DateTime.UtcNow,
                StatusId = statusId ?? _statusCreatedId,
            });

            _orderContext.OrderItem.Add(new Data.Entities.OrderItem
            {
                Id = Guid.NewGuid().ToByteArray(),
                OrderId = orderIdBytes,
                ServiceId = _serviceEmailId,
                ProductId = _productEmailId,
                Quantity = quantity
            });

            await _orderContext.SaveChangesAsync();
            
            return orderId;
        }

        /// <summary>
        /// Builds a create order request for testing
        /// </summary>
        private CreateOrderRequest BuildCreateOrderRequest(int quantity)
        {
            return BuildCreateOrderRequest(new[] { quantity });
        }

        /// <summary>
        /// Builds a create order request with multiple items
        /// </summary>
        private CreateOrderRequest BuildCreateOrderRequest(int[] quantities)
        {
            return new CreateOrderRequest
            {
                ResellerId = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                Items = quantities.Select(q => new CreateOrderItemRequest
                {
                    ProductId = new Guid(_productEmailId),
                    Quantity = q
                }).ToList()
            };
        }

        /// <summary>
        /// Builds a create order request with custom product ID (for testing invalid products)
        /// </summary>
        private CreateOrderRequest BuildCreateOrderRequest(Guid productId, int quantity)
        {
            return new CreateOrderRequest
            {
                ResellerId = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                Items = new()  
                {
                    new CreateOrderItemRequest
                    {
                        ProductId = productId,
                        Quantity = quantity
                    }
                }
            };
        }

        /// <summary>
        /// Asserts that order summary totals are calculated correctly
        /// </summary>
        private static void AssertOrderTotals(OrderSummary order, int quantity)
        {
            var expectedCost = quantity * PRODUCT_UNIT_COST;
            var expectedPrice = quantity * PRODUCT_UNIT_PRICE;
            
            Assert.AreEqual(expectedCost, order.TotalCost, 
                $"Order total cost should be {expectedCost} for quantity {quantity}");
            Assert.AreEqual(expectedPrice, order.TotalPrice, 
                $"Order total price should be {expectedPrice} for quantity {quantity}");
        }

        /// <summary>
        /// Asserts that order detail totals are calculated correctly
        /// </summary>
        private static void AssertOrderDetailTotals(OrderDetail order, int quantity)
        {
            var expectedCost = quantity * PRODUCT_UNIT_COST;
            var expectedPrice = quantity * PRODUCT_UNIT_PRICE;
            
            Assert.AreEqual(expectedCost, order.TotalCost, 
                $"Order total cost should be {expectedCost} for quantity {quantity}");
            Assert.AreEqual(expectedPrice, order.TotalPrice, 
                $"Order total price should be {expectedPrice} for quantity {quantity}");
        }

        /// <summary>
        /// Seeds the database with reference data (statuses, services, products)
        /// </summary>
        private async Task SeedReferenceDataAsync()
        {
            _orderContext.OrderStatus.AddRange(
                new OrderStatus { Id = _statusCreatedId, Name = STATUS_CREATED },
                new OrderStatus { Id = _statusPendingId, Name = STATUS_PENDING },
                new OrderStatus { Id = _statusInProgressId, Name = STATUS_IN_PROGRESS },
                new OrderStatus { Id = _statusCompletedId, Name = STATUS_COMPLETED },
                new OrderStatus { Id = _statusFailedId, Name = STATUS_FAILED }
            );

            _orderContext.OrderService.Add(new Data.Entities.OrderService
            {
                Id = _serviceEmailId,
                Name = SERVICE_NAME
            });

            _orderContext.OrderProduct.Add(new OrderProduct
            {
                Id = _productEmailId,
                Name = PRODUCT_NAME,
                UnitCost = PRODUCT_UNIT_COST,
                UnitPrice = PRODUCT_UNIT_PRICE,
                ServiceId = _serviceEmailId
            });

            await _orderContext.SaveChangesAsync();
        }
    }
}