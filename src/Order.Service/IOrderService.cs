using Order.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Order.Service
{
    /// <summary>
    /// Interface for Order Service - defines business logic operations
    /// </summary>
    public interface IOrderService
    {
        Task<IEnumerable<OrderSummary>> GetOrdersAsync();
        
        Task<OrderDetail> GetOrderByIdAsync(Guid orderId);

        /// <summary>
        /// TASK 1: Gets orders filtered by status
        /// </summary>
        Task<IEnumerable<OrderSummary>> GetOrdersByStatusAsync(string statusName);

        /// <summary>
        /// TASK 2: Updates an order's status
        /// </summary>
        Task<bool> UpdateOrderStatusAsync(Guid orderId, string newStatusName);

        /// <summary>
        /// TASK 3: Creates a new order with validation
        /// </summary>
        Task<OrderDetail> CreateOrderAsync(CreateOrderRequest request);

        /// <summary>
        /// TASK 4: Calculates profit by month for completed orders
        /// </summary>
        Task<IEnumerable<MonthlyProfitResponse>> CalculateMonthlyProfitAsync();
    }
}