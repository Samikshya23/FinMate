using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FinMate.api.Data;
using FinMate.api.Models;
using FinMate.api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace FinMate.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly FinMateDbContext _context;
        private readonly SmtpEmailService _email;
        private readonly IConfiguration _config;

        // ✅ FIX: include IConfiguration and assign it
        public AuthController(FinMateDbContext context, SmtpEmailService email, IConfiguration config)
        {
            _context = context;
            _email = email;
            _config = config;
        }

        // =============================
        //        REGISTER USER
        // =============================
        // POST: api/Auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            // 1) Basic validation
            if (dto.Password != dto.ConfirmPassword)
                return BadRequest("Password and Confirm Password do not match.");

            if (string.IsNullOrWhiteSpace(dto.UserName) ||
                string.IsNullOrWhiteSpace(dto.Email))
                return BadRequest("Username and Email are required.");

            // 2) Check if email or username already exists
            bool emailExists = await _context.Users.AnyAsync(u => u.Email == dto.Email);
            if (emailExists)
                return BadRequest("Email is already registered.");

            bool usernameExists = await _context.Users.AnyAsync(u => u.UserName == dto.UserName);
            if (usernameExists)
                return BadRequest("Username is already taken.");

            // 3) Hash password
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            // 4) Create user entity
            var user = new User
            {
                UserName = dto.UserName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber ?? string.Empty,
                PasswordHash = passwordHash
            };

            // 5) Save to DB
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok("User registered successfully.");
        }

        // =============================
        //           LOGIN
        // =============================
        // POST: api/Auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Login))
                return BadRequest("Please provide username or email.");

            // 1) Find user by username OR email
            var user = await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.Email == dto.Login || u.UserName == dto.Login);

            if (user == null)
                return Unauthorized("User not found.");

            // 2) Check password
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
            if (!isPasswordValid)
                return Unauthorized("Invalid password.");

            // 3) Generate JWT
            string token = GenerateToken(user);

            return Ok(new
            {
                token,
                user = new
                {
                    user.Id,
                    user.UserName,
                    user.Email,
                    user.PhoneNumber
                }
            });
        }

        // =============================
        //     FORGOT PASSWORD (OTP)
        // =============================
        // POST: api/Auth/forgot-password
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email))
                return BadRequest("Email is required.");

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null)
                return NotFound("User with this email not found.");

            // 1) Generate 6-digit OTP
            var random = new Random();
            string otpCode = random.Next(100000, 999999).ToString();

            // 2) Store OTP + expiry
            user.PasswordResetOtp = otpCode;
            user.PasswordResetOtpExpiry = DateTime.UtcNow.AddMinutes(10);

            await _context.SaveChangesAsync();

            // 3) Send OTP to email (✅ use service, not inline SMTP)
            await _email.SendEmailAsync(
                user.Email,
                "FinMate - Password Reset OTP",
                $"Your OTP code for resetting password is: {otpCode}"
            );

            return Ok("OTP has been sent to your email.");
        }

        // =============================
        //      RESET PASSWORD (OTP)
        // =============================
        // POST: api/Auth/reset-password
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email) ||
                string.IsNullOrWhiteSpace(dto.OtpCode))
                return BadRequest("Email and OTP are required.");

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null)
                return NotFound("User with this email not found.");

            if (user.PasswordResetOtp == null || user.PasswordResetOtpExpiry == null)
                return BadRequest("No OTP has been generated. Use forgot-password first.");

            if (DateTime.UtcNow > user.PasswordResetOtpExpiry.Value)
                return BadRequest("OTP has expired. Please request a new one.");

            if (!string.Equals(user.PasswordResetOtp, dto.OtpCode))
                return BadRequest("Invalid OTP code.");

            if (dto.NewPassword != dto.ConfirmPassword)
                return BadRequest("New Password and Confirm Password do not match.");

            // 1) Hash new password
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

            // 2) Clear OTP
            user.PasswordResetOtp = null;
            user.PasswordResetOtpExpiry = null;

            await _context.SaveChangesAsync();

            return Ok("Password has been reset successfully.");
        }

        // =============================
        //        JWT GENERATION
        // =============================
        private string GenerateToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var jwtKey = _config["JWT:Key"]
                         ?? throw new InvalidOperationException("JWT:Key is missing in appsettings.json.");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    // =============================
    //            DTOs
    // (You can move these to separate files)
    // =============================

    public class RegisterDto
    {
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }

        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class LoginDto
    {
        // User can type either username or email
        public string Login { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class ForgotPasswordDto
    {
        public string Email { get; set; } = string.Empty;
    }

    public class ResetPasswordDto
    {
        public string Email { get; set; } = string.Empty;
        public string OtpCode { get; set; } = string.Empty;

        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
