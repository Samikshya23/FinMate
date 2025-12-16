namespace FinMate.api.Models
{
    public class FinancialSummary
    {
        // Total of all incomes (always positive)
        public decimal TotalIncome { get; set; }

        // Total of all expenses (we will treat them as positive in controller)
        public decimal TotalExpense { get; set; }

        // Balance is calculated from income - expense
        public decimal Balance => TotalIncome - TotalExpense;
    }
}
