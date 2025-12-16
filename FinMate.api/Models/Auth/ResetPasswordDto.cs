public class ResetPasswordDto
{
    public string Email { get; set; } = string.Empty;
    public string OtpCode { get; set; }

    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}
