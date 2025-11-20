using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Order.Model;
using Order.Service;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Order.WebAPI.Controllers
{
    /// <summary>
    /// API Controller for Order operations
    /// </summary>
    [ApiController]
    [Route("orders")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        /// <summary>
        /// GET /orders - Get all orders
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Get()
        {
            var orders = await _orderService.GetOrdersAsync();
            return Ok(orders);
        }

        /// <summary>
        /// GET /orders/{orderId} - Get a specific order by ID
        /// </summary>
        [HttpGet("{orderId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetOrderById(Guid orderId)
        {
            if (orderId == Guid.Empty)
                return BadRequest(new { error = "Invalid order ID" });

            var order = await _orderService.GetOrderByIdAsync(orderId);
            return order != null ? Ok(order) : NotFound();
        }

        /// <summary>
        /// TASK 1: GET /orders/status/{statusName}
        /// Get all orders filtered by status
        /// </summary>
        [HttpGet("status/{statusName}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetOrdersByStatus(string statusName)
        {
            if (string.IsNullOrWhiteSpace(statusName))
                return BadRequest(new { error = "Status name is required" });
            
            var orders = await _orderService.GetOrdersByStatusAsync(statusName);
            return Ok(orders);
        }

        /// <summary>
        /// TASK 2: PUT /orders/{orderId}/status
        /// Update an order's status
        /// </summary>
        [HttpPut("{orderId}/status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateOrderStatus(
            Guid orderId, 
            [FromBody] UpdateOrderStatusRequest request)
        {
            if (orderId == Guid.Empty)
                return BadRequest(new { error = "Invalid order ID" });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            
            var success = await _orderService.UpdateOrderStatusAsync(orderId, request.StatusName);
            
            if (success)
            {
                return Ok(new 
                { 
                    message = "Order status updated successfully",
                    orderId,
                    newStatus = request.StatusName
                });
            }
            
            return NotFound(new 
            { 
                error = $"Order not found or invalid status: '{request.StatusName}'"
            });
        }

        /// <summary>
        /// TASK 3: POST /orders
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            
            if (request.ResellerId == Guid.Empty)
                return BadRequest(new { error = "ResellerId cannot be empty" });
            
            if (request.CustomerId == Guid.Empty)
                return BadRequest(new { error = "CustomerId cannot be empty" });
            
            var emptyProductIds = request.Items
                .Where(i => i.ProductId == Guid.Empty)
                .Select((index) => index)
                .ToList();
            
            if (emptyProductIds.Any())
                return BadRequest(new 
                { 
                    error = "ProductId cannot be empty",
                    invalidItemIndices = emptyProductIds
                });
            
            var duplicateProducts = request.Items
                .GroupBy(i => i.ProductId)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();
            
            if (duplicateProducts.Any())
                return BadRequest(new 
                { 
                    error = "Duplicate products are not allowed in the same order",
                    duplicateProductIds = duplicateProducts
                });
            
            try
            {
                var createdOrder = await _orderService.CreateOrderAsync(request);
                return CreatedAtAction(
                    nameof(GetOrderById),
                    new { orderId = createdOrder.Id },
                    createdOrder
                );
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                // Log exception here in production
                return StatusCode(500, new 
                { 
                    error = "An error occurred while creating the order",
                    details = ex.Message 
                });
            }
        }

        /// <summary>
        /// TASK 4: GET /orders/profit/monthly
        /// Calculate total profit by month for all completed orders
        /// </summary>
        [HttpGet("profit/monthly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetMonthlyProfit()
        {
            try
            {
                var monthlyProfit = await _orderService.CalculateMonthlyProfitAsync();
                return Ok(monthlyProfit);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new 
                { 
                    error = "An error occurred while calculating monthly profit",
                    details = ex.Message 
                });
            }
        }
    }
}