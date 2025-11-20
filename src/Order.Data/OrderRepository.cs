using Microsoft.EntityFrameworkCore;
using Order.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Order.Data
{
    /// <summary>
    /// Repository for Order data access - implements all database operations
    /// </summary>
    public class OrderRepository : IOrderRepository
    {
        private readonly OrderContext _orderContext;

        public OrderRepository(OrderContext orderContext)
        {
            _orderContext = orderContext;
        }

        /// <summary>
        /// Gets all orders from the database as summaries
        /// </summary>
        public async Task<IEnumerable<OrderSummary>> GetOrdersAsync()
        {
            var orders = await _orderContext.Order
                .Include(x => x.Items)
                    .ThenInclude(i => i.Product)
                .Include(x => x.Status)
                .Select(x => new OrderSummary
                {
                    Id = new Guid(x.Id),
                    ResellerId = new Guid(x.ResellerId),
                    CustomerId = new Guid(x.CustomerId),
                    StatusId = new Guid(x.StatusId),
                    StatusName = x.Status.Name,
                    ItemCount = x.Items.Count,
                    TotalCost = x.Items.Sum(i => i.Quantity * i.Product.UnitCost).Value,
                    TotalPrice = x.Items.Sum(i => i.Quantity * i.Product.UnitPrice).Value,
                    CreatedDate = x.CreatedDate
                })
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();

            return orders;
        }

        /// <summary>
        /// Gets a single order by ID with full details including all items
        /// </summary>
        public async Task<OrderDetail> GetOrderByIdAsync(Guid orderId)
        {
            var orderIdBytes = orderId.ToByteArray();

            var order = await _orderContext.Order
                .Where(x => _orderContext.Database.IsInMemory() 
                    ? x.Id.SequenceEqual(orderIdBytes) 
                    : x.Id == orderIdBytes)
                .Select(x => new OrderDetail
                {
                    Id = new Guid(x.Id),
                    ResellerId = new Guid(x.ResellerId),
                    CustomerId = new Guid(x.CustomerId),
                    StatusId = new Guid(x.StatusId),
                    StatusName = x.Status.Name,
                    CreatedDate = x.CreatedDate,
                    TotalCost = x.Items.Sum(i => i.Quantity * i.Product.UnitCost).Value,
                    TotalPrice = x.Items.Sum(i => i.Quantity * i.Product.UnitPrice).Value,
                    Items = x.Items.Select(i => new OrderItem
                    {
                        Id = new Guid(i.Id),
                        OrderId = new Guid(i.OrderId),
                        ServiceId = new Guid(i.ServiceId),
                        ServiceName = i.Service.Name,
                        ProductId = new Guid(i.ProductId),
                        ProductName = i.Product.Name,
                        UnitCost = i.Product.UnitCost,
                        UnitPrice = i.Product.UnitPrice,
                        TotalCost = i.Product.UnitCost * i.Quantity.Value,
                        TotalPrice = i.Product.UnitPrice * i.Quantity.Value,
                        Quantity = i.Quantity.Value
                    })
                }).SingleOrDefaultAsync();
            
            return order;
        }

        /// <summary>
        /// TASK 1: Gets all orders filtered by status 
        /// </summary>
        public async Task<IEnumerable<OrderSummary>> GetOrdersByStatusAsync(string statusName)
        {
            var orders = await _orderContext.Order
                .Include(x => x.Items)
                    .ThenInclude(i => i.Product)
                .Include(x => x.Status)
                .Where(x => x.Status.Name.ToLower() == statusName.ToLower())
                .Select(x => new OrderSummary
                {
                    Id = new Guid(x.Id),
                    ResellerId = new Guid(x.ResellerId),
                    CustomerId = new Guid(x.CustomerId),
                    StatusId = new Guid(x.StatusId),
                    StatusName = x.Status.Name,
                    ItemCount = x.Items.Count,
                    TotalCost = x.Items.Sum(i => i.Quantity * i.Product.UnitCost).Value,
                    TotalPrice = x.Items.Sum(i => i.Quantity * i.Product.UnitPrice).Value,
                    CreatedDate = x.CreatedDate
                })
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();

            return orders;
        }

        /// <summary>
        /// TASK 2: Updates an order's status
        /// </summary>
        public async Task<bool> UpdateOrderStatusAsync(Guid orderId, string newStatusName)
        {
            var orderIdBytes = orderId.ToByteArray();
            
            var order = await _orderContext.Order
                .Where(x => _orderContext.Database.IsInMemory() 
                    ? x.Id.SequenceEqual(orderIdBytes)
                    : x.Id == orderIdBytes)
                .SingleOrDefaultAsync();
            
            if (order == null)
                return false;
            
            var status = await _orderContext.OrderStatus
                .Where(x => x.Name.ToLower() == newStatusName.ToLower())
                .SingleOrDefaultAsync();
            
            if (status == null)
                return false;
            
            order.StatusId = status.Id;
            await _orderContext.SaveChangesAsync();
            
            return true;
        }

        /// <summary>
        /// TASK 3: Creates a new order with items
        /// </summary>
        public async Task<OrderDetail> CreateOrderAsync(CreateOrderRequest request)
        {
            var duplicateProducts = request.Items
                .GroupBy(i => i.ProductId)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();
            
            if (duplicateProducts.Any())
                throw new InvalidOperationException(
                    $"Duplicate products found: {string.Join(", ", duplicateProducts)}");
            
            var createdStatus = await _orderContext.OrderStatus
                .Where(x => x.Name.ToLower() == "created")
                .SingleOrDefaultAsync();
            
            if (createdStatus == null)
                throw new InvalidOperationException("Created status not found in database");
            
            var productIds = request.Items.Select(i => i.ProductId.ToByteArray()).ToList();
            var products = await _orderContext.OrderProduct
                .Include(p => p.Service) // Include service for ServiceName
                .Where(p => _orderContext.Database.IsInMemory()
                    ? productIds.Any(pid => pid.SequenceEqual(p.Id))
                    : productIds.Contains(p.Id))
                .ToListAsync();
            
            var productDict = products.ToDictionary(p => new Guid(p.Id), p => p);
            
            var missingProducts = request.Items
                .Where(i => !productDict.ContainsKey(i.ProductId))
                .Select(i => i.ProductId)
                .ToList();
            
            if (missingProducts.Any())
                throw new InvalidOperationException(
                    $"Products not found: {string.Join(", ", missingProducts)}");
            
            var newOrderId = Guid.NewGuid();
            var newOrder = new Entities.Order
            {
                Id = newOrderId.ToByteArray(),
                ResellerId = request.ResellerId.ToByteArray(),
                CustomerId = request.CustomerId.ToByteArray(),
                StatusId = createdStatus.Id,
                CreatedDate = DateTime.UtcNow
            };
            
            _orderContext.Order.Add(newOrder);
            
            var orderItemsList = new List<Entities.OrderItem>();
            foreach (var itemRequest in request.Items)
            {
                var product = productDict[itemRequest.ProductId];
                
                var orderItem = new Entities.OrderItem
                {
                    Id = Guid.NewGuid().ToByteArray(),
                    OrderId = newOrder.Id,
                    ProductId = product.Id,
                    ServiceId = product.ServiceId,
                    Quantity = itemRequest.Quantity
                };
                
                _orderContext.OrderItem.Add(orderItem);
                orderItemsList.Add(orderItem);
            }
            
            await _orderContext.SaveChangesAsync();
            
            return new OrderDetail
            {
                Id = newOrderId,
                ResellerId = request.ResellerId,
                CustomerId = request.CustomerId,
                StatusId = new Guid(createdStatus.Id),
                StatusName = "Created",
                CreatedDate = newOrder.CreatedDate,
                TotalCost = request.Items.Sum(i => 
                    productDict[i.ProductId].UnitCost * i.Quantity),
                TotalPrice = request.Items.Sum(i => 
                    productDict[i.ProductId].UnitPrice * i.Quantity),
                Items = request.Items.Select(i =>
                {
                    var p = productDict[i.ProductId];
                    return new OrderItem
                    {
                        Id = orderItemsList.First(oi => 
                            new Guid(oi.ProductId) == i.ProductId).Id != null 
                            ? new Guid(orderItemsList.First(oi => 
                                new Guid(oi.ProductId) == i.ProductId).Id) 
                            : Guid.NewGuid(),
                        OrderId = newOrderId,
                        ServiceId = new Guid(p.ServiceId),
                        ServiceName = p.Service?.Name,
                        ProductId = i.ProductId,
                        ProductName = p.Name,
                        Quantity = i.Quantity,
                        UnitCost = p.UnitCost,
                        UnitPrice = p.UnitPrice,
                        TotalCost = p.UnitCost * i.Quantity,
                        TotalPrice = p.UnitPrice * i.Quantity
                    };
                }).ToList()
            };
        }

        /// <summary>
        /// TASK 4: Gets all completed orders for profit calculation
        /// </summary>
        public async Task<IEnumerable<OrderSummary>> GetCompletedOrdersAsync()
        {
            return await GetOrdersByStatusAsync("Completed");
        }
    }
}