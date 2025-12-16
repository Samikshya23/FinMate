namespace FinMate.web.Models
{
    public class IncomeItemVm
    {
        public int Id { get; set; }
        public string Source { get; set; } = "";
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string Category { get; set; } = "";
        public string? Note { get; set; }
    }
}
