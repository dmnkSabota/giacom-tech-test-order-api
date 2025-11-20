using Order.Data;
using Order.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Order.Service
{
    /// <summary>
    /// Service layer for Order operations - handles business logic
    /// </summary>
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;

        public OrderService(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<IEnumerable<OrderSummary>> GetOrdersAsync()
        {
            return await _orderRepository.GetOrdersAsync();
        }

        public async Task<OrderDetail> GetOrderByIdAsync(Guid orderId)
        {
            return await _orderRepository.GetOrderByIdAsync(orderId);
        }

        /// <summary>
        /// TASK 1: Gets all orders filtered by status name
        /// </summary>
        public async Task<IEnumerable<OrderSummary>> GetOrdersByStatusAsync(string statusName)
        {
            return await _orderRepository.GetOrdersByStatusAsync(statusName);
        }

        /// <summary>
        /// TASK 2: Updates an order's status
        /// </summary>
        public async Task<bool> UpdateOrderStatusAsync(Guid orderId, string newStatusName)
        {
            return await _orderRepository.UpdateOrderStatusAsync(orderId, newStatusName);
        }

        /// <summary>
        /// TASK 3: Creates a new order
        /// </summary>
        public async Task<OrderDetail> CreateOrderAsync(CreateOrderRequest request)
        {
            return await _orderRepository.CreateOrderAsync(request);
        }

        /// <summary>
        /// TASK 4: Calculates total profit by month for all completed orders 
        /// </summary>
        public async Task<IEnumerable<MonthlyProfitResponse>> CalculateMonthlyProfitAsync()
        {
            var completedOrders = await _orderRepository.GetCompletedOrdersAsync();
            
            var profitByMonth = completedOrders
                .GroupBy(order => new 
                { 
                    order.CreatedDate.Year, 
                    order.CreatedDate.Month 
                })
                .Select(group => new MonthlyProfitResponse
                {
                    Year = group.Key.Year,
                    Month = group.Key.Month,
                    MonthName = new DateTime(group.Key.Year, group.Key.Month, 1, 0, 0, 0, DateTimeKind.Utc)
                        .ToString("MMMM", CultureInfo.InvariantCulture),
                    TotalProfit = group.Sum(order => order.TotalPrice - order.TotalCost),
                    OrderCount = group.Count()
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToList();
            
            return profitByMonth;
        }
    }
}