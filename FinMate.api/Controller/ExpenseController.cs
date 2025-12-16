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
    [Authorize]
    public class ExpensesController : ControllerBase
    {
        private readonly FinMateDbContext _context;

        public ExpensesController(FinMateDbContext context)
        {
            _context = context;
        }

        private int GetUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        }

        // -------------------------
        // GET: api/Expenses
        // -------------------------
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Expense>>> GetExpenses()
        {
            int userId = GetUserId();

            var expenses = await _context.Expenses
                .Where(e => e.UserId == userId)
                .OrderByDescending(e => e.Date)
                .ToListAsync();

            return Ok(expenses);
        }

        // -------------------------
        // GET: api/Expenses/5
        // -------------------------
        [HttpGet("{id}")]
        public async Task<ActionResult<Expense>> GetExpense(int id)
        {
            int userId = GetUserId();

            var expense = await _context.Expenses
                .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);

            if (expense == null)
                return NotFound();

            return Ok(expense);
        }

        // -------------------------
        // POST: api/Expenses
        // -------------------------
        // -------------------------
        // POST: api/Expenses
        // -------------------------
        [HttpPost]
        public async Task<IActionResult> PostExpense([FromBody] Expense expense)
        {
            int userId = GetUserId();
            expense.UserId = userId;

            // always store positive amount
            expense.Amount = Math.Abs(expense.Amount);

            if (expense.Date == default)
                expense.Date = DateTime.UtcNow;

            // 1) Save expense first
            _context.Expenses.Add(expense);
            await _context.SaveChangesAsync();

            // 2) Check budget warning (same month + same category)
            string month = expense.Date.ToString("yyyy-MM");

            var budget = await _context.Budgets
                .FirstOrDefaultAsync(b =>
                    b.UserId == userId &&
                    b.Month == month &&
                    b.Category == expense.Category);

            if (budget == null)
            {
                // No budget set -> just return expense
                return Ok(new
                {
                    message = "Expense added. No budget set for this category/month.",
                    expense
                });
            }

            // Total spent in this category for that month
            var monthStart = new DateTime(expense.Date.Year, expense.Date.Month, 1);
            var monthEnd = monthStart.AddMonths(1);

            var spent = await _context.Expenses
                .Where(e => e.UserId == userId
                         && e.Category == expense.Category
                         && e.Date >= monthStart && e.Date < monthEnd)
                .SumAsync(e => (decimal?)e.Amount) ?? 0m;

            var remaining = budget.LimitAmount - spent;

            if (spent > budget.LimitAmount)
            {
                return Ok(new
                {
                    message = $"⚠ Budget exceeded for {expense.Category} ({month}). Exceeded by Rs. {spent - budget.LimitAmount}.",
                    expense,
                    budget = new
                    {
                        budget.Category,
                        budget.Month,
                        budget.LimitAmount,
                        spent,
                        remaining
                    }
                });
            }

            return Ok(new
            {
                message = $"Expense added. Remaining budget for {expense.Category} ({month}) is Rs. {remaining}.",
                expense,
                budget = new
                {
                    budget.Category,
                    budget.Month,
                    budget.LimitAmount,
                    spent,
                    remaining
                }
            });
        }


        // -------------------------
        // PUT: api/Expenses/5
        // -------------------------
        [HttpPut("{id}")]
        public async Task<IActionResult> PutExpense(int id, [FromBody] Expense updated)
        {
            int userId = GetUserId();

            if (id != updated.Id)
                return BadRequest();

            var existing = await _context.Expenses
                .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);

            if (existing == null)
                return NotFound();

            // Update fields based on your model
            existing.Amount = Math.Abs(updated.Amount);
            existing.Title = updated.Title;
            existing.Category = updated.Category;
            existing.Date = updated.Date == default ? existing.Date : updated.Date;
            existing.Source = updated.Source;
            existing.LimitAmount = updated.LimitAmount;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // -------------------------
        // DELETE: api/Expenses/5
        // -------------------------
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteExpense(int id)
        {
            int userId = GetUserId();

            var expense = await _context.Expenses
                .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);

            if (expense == null)
                return NotFound();

            _context.Expenses.Remove(expense);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
