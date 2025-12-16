using System.Text;
using System.Text.Json;
using FinMate.web.Models;
using FinMate.web.Services;
using Microsoft.AspNetCore.Mvc;

namespace FinMate.web.Controllers
{
    public class IncomesController : Controller
    {
        private readonly ApiClient _api;

        public IncomesController(ApiClient api)
        {
            _api = api;
        }

        // GET: /Incomes
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // login check
            var token = Request.Cookies["finmate_token"];
            if (string.IsNullOrWhiteSpace(token))
                return RedirectToAction("Login", "Auth");

            var vm = new IncomesIndexVm();

            try
            {
                var client = _api.CreateAuthorizedClient();

                // ✅ Load list (GET /api/Incomes)
                var resp = await client.GetAsync("/api/Incomes");
                if (resp.IsSuccessStatusCode)
                {
                    var json = await resp.Content.ReadAsStringAsync();
                    var items = JsonSerializer.Deserialize<List<IncomeItemVm>>(json,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    vm.Items = items ?? new List<IncomeItemVm>();
                }
                else
                {
                    ViewBag.Error = "Could not load incomes list (API).";
                }
            }
            catch
            {
                ViewBag.Error = "API connection failed. Make sure FinMate.api is running.";
            }

            return View(vm);
        }

        // POST: /Incomes/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(IncomesIndexVm model)
        {
            var token = Request.Cookies["finmate_token"];
            if (string.IsNullOrWhiteSpace(token))
                return RedirectToAction("Login", "Auth");

            // We validate only the Form part
            if (!TryValidateModel(model.Form, "Form"))
                return await Index(); // reload page with errors

            try
            {
                var client = _api.CreateAuthorizedClient();

                // Swagger shows id/userId too, but DON'T send them from web.
                // API should take user from token.
                var payload = new
                {
                    source = model.Form.Source,
                    amount = model.Form.Amount,
                    date = model.Form.Date,
                    category = model.Form.Category,
                    note = model.Form.Note
                };

                var json = JsonSerializer.Serialize(payload);
                var resp = await client.PostAsync("/api/Incomes",
                    new StringContent(json, Encoding.UTF8, "application/json"));

                var body = await resp.Content.ReadAsStringAsync();

                if (!resp.IsSuccessStatusCode)
                {
                    TempData["Error"] = string.IsNullOrWhiteSpace(body) ? "Failed to add income." : body;
                    return RedirectToAction("Index");
                }

                TempData["Success"] = "Income added successfully.";
                return RedirectToAction("Index");
            }
            catch
            {
                TempData["Error"] = "API connection failed while adding income.";
                return RedirectToAction("Index");
            }
        }
    }
}
