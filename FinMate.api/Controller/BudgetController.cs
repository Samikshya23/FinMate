using System.Security.Claims;
using FinMate.api.Data;
using FinMate.api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinMate.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // ✅ protect budgets
    public class BudgetController : ControllerBase
    {
        private readonly FinMateDbContext _context;

        public BudgetController(FinMateDbContext context)
        {
            _context = context;
        }

        private int GetUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        }

        // ✅ CREATE or UPDATE (Upsert) budget for the logged-in user
        // POST: api/Budget
        [HttpPost]
        public async Task<IActionResult> CreateOrUpdateBudget([FromBody] Budget budget)
        {
            if (budget == null) return BadRequest("Budget cannot be null.");
            if (string.IsNullOrWhiteSpace(budget.Category)) return BadRequest("Category is required.");
            if (budget.LimitAmount <= 0) return BadRequest("LimitAmount must be greater than zero.");
            if (string.IsNullOrWhiteSpace(budget.Month)) return BadRequest("Month is required (example: 2025-12).");

            int userId = GetUserId();

            // Force userId from token (don’t trust client)
            budget.UserId = userId;

            // Upsert: one budget per (UserId, Month, Category)
            var existing = await _context.Budgets.FirstOrDefaultAsync(b =>
                b.UserId == userId &&
                b.Month == budget.Month &&
                b.Category == budget.Category);

            if (existing == null)
            {
                _context.Budgets.Add(budget);
                await _context.SaveChangesAsync();
                return Ok(budget);
            }

            existing.LimitAmount = budget.LimitAmount;
            await _context.SaveChangesAsync();
            return Ok(existing);
        }

        // ✅ GET budgets (only logged-in user)
        // GET: api/Budget?month=2025-12
        [HttpGet]
        public async Task<IActionResult> GetBudgets([FromQuery] string? month)
        {
            int userId = GetUserId();

            var query = _context.Budgets.Where(b => b.UserId == userId);

            if (!string.IsNullOrWhiteSpace(month))
                query = query.Where(b => b.Month == month);

            var budgets = await query
                .OrderBy(b => b.Month)
                .ThenBy(b => b.Category)
                .ToListAsync();

            return Ok(budgets);
        }

        // ✅ GET budget summary (Spent/Remaining) for a month
        // GET: api/Budget/summary?month=2025-12
        [HttpGet("summary")]
        public async Task<IActionResult> GetBudgetSummary([FromQuery] string month)
        {
            if (string.IsNullOrWhiteSpace(month))
                return BadRequest("Month is required (example: 2025-12).");

            if (!TryParseMonth(month, out var start, out var end))
                return BadRequest("Invalid month format. Use YYYY-MM like 2025-12.");

            int userId = GetUserId();

            var budgets = await _context.Budgets
                .Where(b => b.UserId == userId && b.Month == month)
                .ToListAsync();

            var expenses = await _context.Expenses
                .Where(e => e.UserId == userId && e.Date >= start && e.Date < end)
                .ToListAsync();

            var result = budgets.Select(b =>
            {
                var spent = expenses
                    .Where(e => e.Category == b.Category)
                    .Sum(e => e.Amount < 0 ? -e.Amount : e.Amount);

                return new
                {
                    month = b.Month,
                    category = b.Category,
                    limitAmount = b.LimitAmount,
                    spent,
                    remaining = b.LimitAmount - spent,
                    isOverspent = spent > b.LimitAmount
                };
            });

            return Ok(result);
        }

        // ✅ DELETE (only own budget)
        // DELETE: api/Budget/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBudget(int id)
        {
            int userId = GetUserId();

            var budget = await _context.Budgets.FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);
            if (budget == null) return NotFound();

            _context.Budgets.Remove(budget);
            await _context.SaveChangesAsync();

            return Ok("Budget deleted successfully.");
        }

        // helper: parse YYYY-MM to month range
        private static bool TryParseMonth(string month, out DateTime start, out DateTime end)
        {
            start = default;
            end = default;

            var parts = month.Split('-');
            if (parts.Length != 2) return false;

            if (!int.TryParse(parts[0], out int y)) return false;
            if (!int.TryParse(parts[1], out int m)) return false;
            if (m < 1 || m > 12) return false;

            start = new DateTime(y, m, 1);
            end = start.AddMonths(1);
            return true;
        }
    }
}
