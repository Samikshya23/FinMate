using System.Text.Json.Serialization;

namespace FinMate.web.Models
{
    public class DashboardSummaryVm
    {
        [JsonPropertyName("totalIncome")]
        public decimal TotalIncome { get; set; }

        [JsonPropertyName("totalExpense")]
        public decimal TotalExpense { get; set; }

        [JsonPropertyName("balance")]
        public decimal RemainingBudget { get; set; }

        // API does not return these yet
        public string? NextReminderTitle { get; set; }
        public string? Month { get; set; }
    }
}
