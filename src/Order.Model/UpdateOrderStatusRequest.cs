using System.ComponentModel.DataAnnotations;

namespace Order.Model
{
    /// <summary>
    /// Request model for updating an order's status
    /// Used in Task 2: Update Order Status API endpoint
    /// </summary>
    public class UpdateOrderStatusRequest
    {
        /// <summary>
        /// The new status name for the order
        /// Examples: "Created", "In Progress", "Completed", "Failed"
        /// </summary>
        [Required(ErrorMessage = "StatusName is required")]
        [MinLength(1, ErrorMessage = "StatusName cannot be empty")]
        public string StatusName { get; set; }
    }
}