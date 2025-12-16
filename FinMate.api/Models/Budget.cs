namespace FinMate.api.Models
{
    public class Budget
    {
        public int Id { get; set; }                     // Primary key

        public string Category { get; set; } = string.Empty;
        // Food, Travel, Bills...

        public decimal LimitAmount { get; set; }        // Monthly limit

        public string Month { get; set; } = string.Empty;
        // Format: "2025-12"

        public int UserId { get; set; }                 // Logged-in user
    }
}
