namespace FinMate.web.Models
{
    public class SummaryResponse
    {
        public decimal totalIncome { get; set; }
        public decimal totalExpense { get; set; }
        public decimal remainingBudget { get; set; }
        public string? nextReminder { get; set; }
    }
}
