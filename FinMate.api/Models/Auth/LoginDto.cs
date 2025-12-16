namespace FinMate.api.Models.Auth
{
    public class LoginDto
    {
        // User can give username OR email (or both)
        public string? UserName { get; set; }
        public string? Email { get; set; }

        public string Password { get; set; } = string.Empty;
    }
}
