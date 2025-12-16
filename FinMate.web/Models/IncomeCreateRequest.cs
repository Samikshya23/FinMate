using System.ComponentModel.DataAnnotations;

namespace FinMate.web.Models
{
    public class IncomeCreateRequest
    {
        [Required(ErrorMessage = "Source is required")]
        public string Source { get; set; } = "";

        [Required(ErrorMessage = "Amount is required")]
        [Range(1, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Date is required")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Category is required")]
        public string Category { get; set; } = "";

        public string? Note { get; set; }
    }
}
