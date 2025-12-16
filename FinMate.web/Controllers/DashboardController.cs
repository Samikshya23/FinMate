using System.Text.Json;
using FinMate.web.Models;
using FinMate.web.Services;
using Microsoft.AspNetCore.Mvc;

namespace FinMate.web.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApiClient _api;

        public DashboardController(ApiClient api)
        {
            _api = api;
        }

        public async Task<IActionResult> Index()
        {
            // 1) basic login check
            var token = Request.Cookies["finmate_token"];
            if (string.IsNullOrWhiteSpace(token))
                return RedirectToAction("Login", "Auth");

            // 2) call API with Bearer token
            try
            {
                using var client = _api.CreateAuthorizedClient();

                // ✅ This must match Swagger endpoint exactly
                var resp = await client.GetAsync("/api/Summary");
                var body = await resp.Content.ReadAsStringAsync();

                if (!resp.IsSuccessStatusCode)
                {
                    // show real error coming from API (401/500 etc.)
                    ViewBag.Error = string.IsNullOrWhiteSpace(body)
                        ? "Failed to load dashboard data (API)."
                        : body;

                    return View(new DashboardSummaryVm());
                }

                var vm = JsonSerializer.Deserialize<DashboardSummaryVm>(body, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new DashboardSummaryVm();

                return View(vm);
            }
            catch (HttpRequestException)
            {
                ViewBag.Error = "API connection failed. Make sure FinMate.api is running (https://localhost:7094).";
                return View(new DashboardSummaryVm());
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View(new DashboardSummaryVm());
            }
        }
    }
}
