using Microsoft.AspNetCore.Mvc;
using FinMate.web.Models;
using System.Text;
using System.Text.Json;

namespace FinMate.web.Controllers
{
    public class AuthController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;

        public AuthController(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
        }

        // ---------------- GET ----------------
        [HttpGet]
        public IActionResult Login() => View(new LoginRequest());

        [HttpGet]
        public IActionResult Register() => View(new RegisterRequest());

        [HttpGet]
        public IActionResult ForgotPassword() => View(new ForgotPasswordRequest());

        [HttpGet]
        public IActionResult ResetPassword(string? email, string? token)
        {
            return View(new ResetPasswordRequest
            {
                Email = email ?? "",
                Token = token ?? ""
            });
        }


        // ---------------- REGISTER (POST) ----------------
        [HttpPost]
        public async Task<IActionResult> Register(RegisterRequest model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var baseUrl = _config["ApiBaseUrl"]?.TrimEnd('/');
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                ViewBag.Error = "ApiBaseUrl missing in appsettings.json";
                return View(model);
            }

            var url = $"{baseUrl}/api/Auth/register";
            var client = _httpClientFactory.CreateClient();

            var payload = new
            {
                userName = model.UserName,
                email = model.Email,
                phoneNumber = model.PhoneNumber,
                password = model.Password,
                confirmPassword = model.ConfirmPassword
            };

            var json = JsonSerializer.Serialize(payload);
            var resp = await client.PostAsync(url, new StringContent(json, Encoding.UTF8, "application/json"));
            var body = await resp.Content.ReadAsStringAsync();

            if (resp.IsSuccessStatusCode)
            {
                TempData["Success"] = "User registered successfully.";
                return RedirectToAction("VerifyResetCode", new { email = model.Email });

            }

            var lower = (body ?? "").ToLower();
            if (lower.Contains("already") || lower.Contains("exist") || lower.Contains("duplicate"))
                ViewBag.Error = "Email is already used. Please try another email.";
            else
                ViewBag.Error = string.IsNullOrWhiteSpace(body) ? "Registration failed." : body;

            return View(model);
        }

        // ---------------- LOGIN (POST) ----------------
        [HttpPost]
        public async Task<IActionResult> Login(LoginRequest model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var baseUrl = _config["ApiBaseUrl"]?.TrimEnd('/');
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                ViewBag.Error = "ApiBaseUrl missing in appsettings.json";
                return View(model);
            }

            var url = $"{baseUrl}/api/Auth/login"; // ✅ if swagger is different, change only this line
            var client = _httpClientFactory.CreateClient();

            var payload = new { login = model.Email, password = model.Password };

            var json = JsonSerializer.Serialize(payload);

            var resp = await client.PostAsync(url, new StringContent(json, Encoding.UTF8, "application/json"));
            var body = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
            {
                ViewBag.Error = "Invalid email or password.";
                return View(model);
            }

            // ✅ token parse (supports: { token: "..." } or { accessToken: "..." } or plain text)
            string? token = null;

            try
            {
                if (!string.IsNullOrWhiteSpace(body) && body.TrimStart().StartsWith("{"))
                {
                    using var doc = JsonDocument.Parse(body);
                    var root = doc.RootElement;

                    if (root.TryGetProperty("token", out var t)) token = t.GetString();
                    else if (root.TryGetProperty("accessToken", out var at)) token = at.GetString();
                    else if (root.TryGetProperty("jwt", out var jwt)) token = jwt.GetString();
                }
            }
            catch
            {
                // ignore JSON parse errors
            }

            if (string.IsNullOrWhiteSpace(token))
                token = body?.Trim().Trim('"');

            if (string.IsNullOrWhiteSpace(token))
            {
                ViewBag.Error = "Login success but token not received. Check API login response.";
                return View(model);
            }

            Response.Cookies.Append("finmate_token", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });

            return RedirectToAction("Index", "Dashboard");
        }

        // ---------------- FORGOT PASSWORD (POST) ----------------
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var baseUrl = _config["ApiBaseUrl"]?.TrimEnd('/');
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                ViewBag.Error = "ApiBaseUrl missing in appsettings.json";
                return View(model);
            }

            var url = $"{baseUrl}/api/Auth/forgot-password";
            var client = _httpClientFactory.CreateClient();

            var payload = new { email = model.Email };
            var json = JsonSerializer.Serialize(payload);

            var resp = await client.PostAsync(url, new StringContent(json, Encoding.UTF8, "application/json"));
            var body = await resp.Content.ReadAsStringAsync();

            if (resp.IsSuccessStatusCode)
            {
                TempData["Success"] = "OTP/Reset code sent to your email.";
                return RedirectToAction("OtpReset", new { email = model.Email });
            }


            ViewBag.Error = string.IsNullOrWhiteSpace(body) ? "Failed to send reset email." : body;
            return View(model);
        }

        // ---------------- RESET PASSWORD (POST) ----------------
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var baseUrl = _config["ApiBaseUrl"]?.TrimEnd('/');
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                ViewBag.Error = "ApiBaseUrl missing in appsettings.json";
                return View(model);
            }

            var url = $"{baseUrl}/api/Auth/reset-password";
            var client = _httpClientFactory.CreateClient();

            var payload = new
            {
                email = model.Email,
                otpCode = model.Token,   // ✅ change token -> otpCode
                newPassword = model.NewPassword,
                confirmPassword = model.ConfirmPassword
            };


            var json = JsonSerializer.Serialize(payload);

            var resp = await client.PostAsync(url, new StringContent(json, Encoding.UTF8, "application/json"));
            var body = await resp.Content.ReadAsStringAsync();

            if (resp.IsSuccessStatusCode)
            {
                TempData["Success"] = string.IsNullOrWhiteSpace(body)
                    ? "Password reset successfully."
                    : body;

                return RedirectToAction("Login");
            }

            ViewBag.Error = string.IsNullOrWhiteSpace(body) ? "Reset failed. Check token and try again." : body;
            return View(model);
        }
        // ---------------- VERIFY RESET CODE ----------------
        [HttpGet]
        public IActionResult VerifyResetCode(string? email)
        {
            ViewBag.Email = email ?? "";
            return View();
        }

        [HttpPost]
        public IActionResult VerifyResetCode(string email, string token)
        {
            return RedirectToAction("ResetPassword", new { email, token });
        }
        // ---------------- OTP RESET (GET) ----------------
        [HttpGet]
        public IActionResult OtpReset(string? email)
        {
            return View(new OtpResetRequest
            {
                Email = email ?? ""
            });
        }

        // ---------------- OTP RESET (POST) ----------------
        [HttpPost]
        public async Task<IActionResult> OtpReset(OtpResetRequest model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var baseUrl = _config["ApiBaseUrl"]?.TrimEnd('/');
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                ViewBag.Error = "ApiBaseUrl missing in appsettings.json";
                return View(model);
            }

            var url = $"{baseUrl}/api/Auth/reset-password";
            var client = _httpClientFactory.CreateClient();

            var payload = new
            {
                email = model.Email,
                otpCode = model.Token,
                newPassword = model.NewPassword,
                confirmPassword = model.ConfirmPassword
            };

            var json = JsonSerializer.Serialize(payload);
            var resp = await client.PostAsync(url, new StringContent(json, Encoding.UTF8, "application/json"));
            var body = await resp.Content.ReadAsStringAsync();

            if (resp.IsSuccessStatusCode)
            {
                TempData["Success"] = string.IsNullOrWhiteSpace(body)
                    ? "Password reset successfully. Please login."
                    : body;

                return RedirectToAction("Login");
            }

            ViewBag.Error = string.IsNullOrWhiteSpace(body)
                ? "Reset failed. Check OTP/token and try again."
                : body;

            return View(model);
        }

        // ---------------- LOGOUT ----------------
        [HttpGet]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("finmate_token");
            return RedirectToAction("Login");
        }
    }
}
