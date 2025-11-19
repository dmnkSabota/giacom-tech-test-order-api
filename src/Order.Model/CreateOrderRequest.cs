using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Order.Model
{
    /// <summary>
    /// Request model for creating a new order
    /// Used in Task 3: Create Order API endpoint
    /// </summary>
    public class CreateOrderRequest
    {
        /// <summary>
        /// The ID of the reseller placing the order
        /// </summary>
        [Required(ErrorMessage = "ResellerId is required")]
        public Guid ResellerId { get; set; }
        
        /// <summary>
        /// The ID of the customer the order is for
        /// </summary>
        [Required(ErrorMessage = "CustomerId is required")]
        public Guid CustomerId { get; set; }
        
        /// <summary>
        /// List of items to include in the order
        /// </summary>
        [Required(ErrorMessage = "Items list is required")]
        [MinLength(1, ErrorMessage = "At least one order item is required")]
        public List<CreateOrderItemRequest> Items { get; set; }
    }
    
    /// <summary>
    /// Represents a single item in a new order
    /// </summary>
    public class CreateOrderItemRequest
    {
        /// <summary>
        /// The product ID for this item
        /// </summary>
        [Required(ErrorMessage = "ProductId is required")]
        public Guid ProductId { get; set; }
        
        /// <summary>
        /// How many units of this product
        /// </summary>
        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int Quantity { get; set; }
    }
}