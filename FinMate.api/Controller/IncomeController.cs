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
    public class IncomesController : ControllerBase
    {
        private readonly FinMateDbContext _context;

        public IncomesController(FinMateDbContext context)
        {
            _context = context;
        }

        private int GetUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Income>>> GetIncomes()
        {
            int userId = GetUserId();

            var incomes = await _context.Incomes
                .Where(i => i.UserId == userId)
                .OrderByDescending(i => i.Date)
                .ToListAsync();

            return Ok(incomes);
        }

        [HttpPost]
        [HttpPost]
        public async Task<ActionResult<Income>> PostIncome([FromBody] Income income)
        {
            int userId = GetUserId();
            income.UserId = userId;

            // Always store positive amount
            income.Amount = Math.Abs(income.Amount);

            if (income.Date == default)
                income.Date = DateTime.UtcNow;

            _context.Incomes.Add(income);
            await _context.SaveChangesAsync();

            return Ok(income);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> PutIncome(int id, Income updated)
        {
            int userId = GetUserId();

            var income = await _context.Incomes
                .FirstOrDefaultAsync(i => i.Id == id && i.UserId == userId);

            if (income == null)
                return NotFound();

            income.Amount = Math.Abs(updated.Amount);
            income.Source = updated.Source;
            income.Note = updated.Note;
            income.Date = updated.Date == default ? income.Date : updated.Date;

            await _context.SaveChangesAsync();
            return NoContent();
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteIncome(int id)
        {
            int userId = GetUserId();

            var income = await _context.Incomes
                .FirstOrDefaultAsync(i => i.Id == id && i.UserId == userId);

            if (income == null)
                return NotFound();

            _context.Incomes.Remove(income);
            await _context.SaveChangesAsync();
            return NoContent();
        }

    }
}
