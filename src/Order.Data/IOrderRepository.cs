using Order.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Order.Data
{
    /// <summary>
    /// Interface for Order Repository - defines database operations
    /// </summary>
    public interface IOrderRepository
    {
        /// <summary>
        /// Gets all orders as summaries
        /// </summary>
        Task<IEnumerable<OrderSummary>> GetOrdersAsync();

        /// <summary>
        /// Gets a single order by ID with full details
        /// </summary>
        Task<OrderDetail> GetOrderByIdAsync(Guid orderId);
        
        /// <summary>
        /// TASK 1: Gets all orders filtered by status name
        /// </summary>
        Task<IEnumerable<OrderSummary>> GetOrdersByStatusAsync(string statusName);

        /// <summary>
        /// TASK 2: Updates an order's status
        /// </summary>
        Task<bool> UpdateOrderStatusAsync(Guid orderId, string newStatusName);

        /// <summary>
        /// TASK 3: Creates a new order
        /// </summary>
        Task<OrderDetail> CreateOrderAsync(CreateOrderRequest request);

        /// <summary>
        /// TASK 4: Gets all completed orders for profit calculation
        /// </summary>
        Task<IEnumerable<OrderSummary>> GetCompletedOrdersAsync();
    }
}
