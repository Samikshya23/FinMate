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
    public class SummaryController : ControllerBase
    {
        private readonly FinMateDbContext _context;

        public SummaryController(FinMateDbContext context)
        {
            _context = context;
        }

        private int GetUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        }

        [HttpGet]
        public async Task<IActionResult> GetOverallSummary()
        {
            int userId = GetUserId();

            var totalIncome = await _context.Incomes
                .Where(i => i.UserId == userId)
                .SumAsync(i => (decimal?)i.Amount) ?? 0m;

            var totalExpense = await _context.Expenses
                .Where(e => e.UserId == userId)
                .SumAsync(e => (decimal?)e.Amount) ?? 0m;

            var summary = new FinancialSummary
            {
                TotalIncome = totalIncome,
                TotalExpense = totalExpense
            };

            return Ok(summary);
        }
    }
}
