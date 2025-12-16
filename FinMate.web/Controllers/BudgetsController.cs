using FinMate.web.Models;
using FinMate.web.Services;
using Microsoft.AspNetCore.Mvc;

namespace FinMate.web.Controllers
{
    public class BudgetsController : Controller
    {
        private readonly BudgetService _budgetService;

        public BudgetsController(BudgetService budgetService)
        {
            _budgetService = budgetService;
        }

        public async Task<IActionResult> Index(string? month)
        {
            month ??= DateTime.Now.ToString("yyyy-MM");

            var budgets = await _budgetService.GetBudgetsByMonth(month) ?? new List<BudgetVm>();

            ViewBag.Month = month;
            return View(budgets);
        }

        [HttpPost]
        public async Task<IActionResult> Save(BudgetUpsertVm vm)
        {
            if (string.IsNullOrWhiteSpace(vm.Month))
                vm.Month = DateTime.Now.ToString("yyyy-MM");

            if (vm.LimitAmount <= 0)
            {
                TempData["err"] = "Limit must be greater than 0.";
                return RedirectToAction("Index", new { month = vm.Month });
            }

            await _budgetService.UpsertBudget(vm);
            TempData["ok"] = "Budget saved.";
            return RedirectToAction("Index", new { month = vm.Month });
        }
    }
}
