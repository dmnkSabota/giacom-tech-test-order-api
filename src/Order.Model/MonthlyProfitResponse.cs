
namespace Order.Model
{
    /// <summary>
    /// Response model for monthly profit calculation
    /// Used in Task 4: Calculate Profit by Month API endpoint
    /// </summary>
    public class MonthlyProfitResponse
    {
        /// <summary>
        /// The year of the profit calculation
        /// </summary>
        public int Year { get; set; }
        
        /// <summary>
        /// The month number (1-12)
        /// </summary>
        public int Month { get; set; }
        
        /// <summary>
        /// The name of the month for easy reading
        /// </summary>
        public string MonthName { get; set; }
        
        /// <summary>
        /// Total profit for this month (TotalPrice - TotalCost)
        /// </summary>
        public decimal TotalProfit { get; set; }
        
        /// <summary>
        /// Number of completed orders in this month
        /// </summary>
        public int OrderCount { get; set; }
    }
}