using System.Security.Claims;
using FinMate.api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinMate.api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly FinMateDbContext _context;

    public DashboardController(FinMateDbContext context)
    {
        _context = context;
    }

    private int GetUserId()
    {
        return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
    }

    // 1️⃣ Monthly Income vs Expense
    // GET: api/Dashboard/monthly?year=2025
    [HttpGet("monthly")]
    public async Task<IActionResult> Monthly([FromQuery] int year)
    {
        int userId = GetUserId();

        var income = await _context.Incomes
            .Where(i => i.UserId == userId && i.Date.Year == year)
            .GroupBy(i => i.Date.Month)
            .Select(g => new { Month = g.Key, Total = g.Sum(x => x.Amount) })
            .ToListAsync();

        var expense = await _context.Expenses
            .Where(e => e.UserId == userId && e.Date.Year == year)
            .GroupBy(e => e.Date.Month)
            .Select(g => new { Month = g.Key, Total = g.Sum(x => x.Amount) })
            .ToListAsync();

        var result = Enumerable.Range(1, 12).Select(m => new
        {
            month = m,
            income = income.FirstOrDefault(x => x.Month == m)?.Total ?? 0,
            expense = expense.FirstOrDefault(x => x.Month == m)?.Total ?? 0
        });

        return Ok(result);
    }

    // 2️⃣ Category-wise spending (Pie chart)
    // GET: api/Dashboard/category?month=2025-01
    [HttpGet("category")]
    public async Task<IActionResult> CategoryWise([FromQuery] string month)
    {
        if (string.IsNullOrWhiteSpace(month))
            return BadRequest("Month required (yyyy-MM)");

        var parts = month.Split('-');
        if (parts.Length != 2) return BadRequest("Invalid month");

        int year = int.Parse(parts[0]);
        int mon = int.Parse(parts[1]);

        int userId = GetUserId();

        var data = await _context.Expenses
            .Where(e => e.UserId == userId && e.Date.Year == year && e.Date.Month == mon)
            .GroupBy(e => e.Category)
            .Select(g => new
            {
                category = g.Key,
                total = g.Sum(x => x.Amount)
            })
            .OrderByDescending(x => x.total)
            .ToListAsync();

        return Ok(data);
    }

    // 3️⃣ Last 7 days trend
    // GET: api/Dashboard/last7days
    [HttpGet("last7days")]
    public async Task<IActionResult> Last7Days()
    {
        int userId = GetUserId();
        var today = DateTime.Today;
        var start = today.AddDays(-6);

        var incomes = await _context.Incomes
            .Where(i => i.UserId == userId && i.Date >= start)
            .GroupBy(i => i.Date.Date)
            .Select(g => new { Date = g.Key, Total = g.Sum(x => x.Amount) })
            .ToListAsync();

        var expenses = await _context.Expenses
            .Where(e => e.UserId == userId && e.Date >= start)
            .GroupBy(e => e.Date.Date)
            .Select(g => new { Date = g.Key, Total = g.Sum(x => x.Amount) })
            .ToListAsync();

        var result = Enumerable.Range(0, 7).Select(i =>
        {
            var date = start.AddDays(i).Date;
            return new
            {
                date = date.ToString("yyyy-MM-dd"),
                income = incomes.FirstOrDefault(x => x.Date == date)?.Total ?? 0,
                expense = expenses.FirstOrDefault(x => x.Date == date)?.Total ?? 0
            };
        });

        return Ok(result);
    }
}
