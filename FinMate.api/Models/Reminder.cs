using System.ComponentModel.DataAnnotations;

namespace FinMate.api.Models
{
    public class Reminder
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        [Required, MaxLength(120)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(300)]
        public string Message { get; set; } = string.Empty;

        public DateTime DueAt { get; set; }

        public bool SendEmail { get; set; } = true;

        public bool IsSent { get; set; } = false;

        [MaxLength(120)]
        public string EmailTo { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? SentAt { get; set; }
    }
}
