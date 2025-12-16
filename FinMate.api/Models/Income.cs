using System;

namespace FinMate.api.Models
{
    public class Income
    {
        public int Id { get; set; }

        public string Source { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public DateTime Date { get; set; }

        public string Category { get; set; } = string.Empty;

        public string Note { get; set; } = string.Empty;

        public int UserId { get; set; }
    }
}
