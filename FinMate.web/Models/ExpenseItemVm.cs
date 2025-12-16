namespace FinMate.web.Models
{
    public class ExpenseItemVm
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public decimal Amount { get; set; }
        public string Category { get; set; } = "";
        public DateTime Date { get; set; }
        public string Source { get; set; } = "";
        public string? Note { get; set; }
    }
}
