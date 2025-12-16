using System.ComponentModel.DataAnnotations;

namespace FinMate.web.Models
{
    public class LoginRequest
    {
        [Required, EmailAddress(ErrorMessage = "Enter a valid email")]
        public string Email { get; set; } = "";

        [Required, MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
        public string Password { get; set; } = "";
    }
}
