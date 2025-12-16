namespace FinMate.api.Models
{
    public class Expense
    {
        public int UserId { get; set; }

        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public string Category { get; set; } = string.Empty;

        public DateTime Date { get; set; }

        // NEW FIELDS
        public string Source { get; set; } = string.Empty;

        public decimal LimitAmount { get; set; }
    }
}
