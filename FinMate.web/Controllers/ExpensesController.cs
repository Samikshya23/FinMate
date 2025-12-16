using System.Text;
using System.Text.Json;
using FinMate.web.Models;
using FinMate.web.Services;
using Microsoft.AspNetCore.Mvc;

namespace FinMate.web.Controllers
{
    public class ExpensesController : Controller
    {
        private readonly ApiClient _api;

        public ExpensesController(ApiClient api)
        {
            _api = api;
        }

        // GET: /Expenses
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var token = Request.Cookies["finmate_token"];
            if (string.IsNullOrWhiteSpace(token))
                return RedirectToAction("Login", "Auth");

            var vm = new ExpensesIndexVm();

            try
            {
                var client = _api.CreateAuthorizedClient();

                // ✅ Load list (GET /api/Expenses)
                var resp = await client.GetAsync("/api/Expenses");
                if (resp.IsSuccessStatusCode)
                {
                    var json = await resp.Content.ReadAsStringAsync();
                    var items = JsonSerializer.Deserialize<List<ExpenseItemVm>>(json,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    vm.Items = items ?? new List<ExpenseItemVm>();
                }
                else
                {
                    ViewBag.Error = "Could not load expenses list (API).";
                }
            }
            catch
            {
                ViewBag.Error = "API connection failed. Make sure FinMate.api is running.";
            }

            return View(vm);
        }

        // POST: /Expenses/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(ExpensesIndexVm model)
        {
            var token = Request.Cookies["finmate_token"];
            if (string.IsNullOrWhiteSpace(token))
                return RedirectToAction("Login", "Auth");

            if (!TryValidateModel(model.Form, "Form"))
                return await Index();

            try
            {
                var client = _api.CreateAuthorizedClient();

                var payload = new
                {
                    title = model.Form.Title,
                    amount = model.Form.Amount,
                    category = model.Form.Category,
                    date = model.Form.Date,
                    source = model.Form.Source,
                    note = model.Form.Note
                };

                var json = JsonSerializer.Serialize(payload);
                var resp = await client.PostAsync("/api/Expenses",
                    new StringContent(json, Encoding.UTF8, "application/json"));

                var body = await resp.Content.ReadAsStringAsync();

                if (!resp.IsSuccessStatusCode)
                {
                    TempData["Error"] = string.IsNullOrWhiteSpace(body) ? "Failed to add expense." : body;
                    return RedirectToAction("Index");
                }

                TempData["Success"] = "Expense added successfully.";
                return RedirectToAction("Index");
            }
            catch
            {
                TempData["Error"] = "API connection failed while adding expense.";
                return RedirectToAction("Index");
            }
        }
    }
}
