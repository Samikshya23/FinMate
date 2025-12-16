using System.ComponentModel.DataAnnotations;

namespace FinMate.web.Models
{
    public class ResetPasswordRequest
    {
        [Required, EmailAddress(ErrorMessage = "Enter a valid email")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Token is required")]
        public string Token { get; set; } = "";

        [Required, MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
        public string NewPassword { get; set; } = "";

        [Required, Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = "";
    }
}
