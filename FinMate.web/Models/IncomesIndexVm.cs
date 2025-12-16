namespace FinMate.web.Models
{
    public class IncomesIndexVm
    {
        public IncomeCreateRequest Form { get; set; } = new IncomeCreateRequest();
        public List<IncomeItemVm> Items { get; set; } = new List<IncomeItemVm>();
    }
}
