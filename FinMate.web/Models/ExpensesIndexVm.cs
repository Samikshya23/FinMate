namespace FinMate.web.Models
{
    public class ExpensesIndexVm
    {
        public ExpenseCreateRequest Form { get; set; } = new ExpenseCreateRequest();
        public List<ExpenseItemVm> Items { get; set; } = new List<ExpenseItemVm>();
    }
}
