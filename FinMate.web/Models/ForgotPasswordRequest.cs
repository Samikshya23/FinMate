using System.ComponentModel.DataAnnotations;

namespace FinMate.web.Models
{
    public class ForgotPasswordRequest
    {
        [Required, EmailAddress(ErrorMessage = "Enter a valid email")]
        public string Email { get; set; } = "";
    }
}
