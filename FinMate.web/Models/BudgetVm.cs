namespace FinMate.web.Models
{
    public class BudgetVm
    {
        public string Category { get; set; } = "";
        public string Month { get; set; } = "";
        public decimal LimitAmount { get; set; }
        public decimal Spent { get; set; }
        public decimal Remaining { get; set; }
    }

    public class BudgetUpsertVm
    {
        public string Category { get; set; } = "";
        public string Month { get; set; } = "";
        public decimal LimitAmount { get; set; }
    }
}
