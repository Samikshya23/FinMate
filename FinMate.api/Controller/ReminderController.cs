using System.Security.Claims;
using FinMate.api.Data;
using FinMate.api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinMate.api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReminderController : ControllerBase
{
    private readonly FinMateDbContext _context;

    public ReminderController(FinMateDbContext context)
    {
        _context = context;
    }

    private int GetUserId()
    {
        return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
    }

    // POST: api/Reminder
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Reminder reminder)
    {
        if (reminder == null) return BadRequest("Reminder cannot be null.");
        if (string.IsNullOrWhiteSpace(reminder.Title)) return BadRequest("Title is required.");
        if (reminder.DueAt == default) return BadRequest("DueAt is required.");

        int userId = GetUserId();
        reminder.UserId = userId;
        reminder.IsSent = false;
        reminder.CreatedAt = DateTime.UtcNow;

        // If EmailTo not provided, use logged-in user's email
        if (string.IsNullOrWhiteSpace(reminder.EmailTo))
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user != null && !string.IsNullOrWhiteSpace(user.Email))
                reminder.EmailTo = user.Email;
        }

        _context.Reminders.Add(reminder);
        await _context.SaveChangesAsync();

        return Ok(reminder);
    }

    // GET: api/Reminder
    [HttpGet]
    public async Task<IActionResult> List()
    {
        int userId = GetUserId();

        var reminders = await _context.Reminders
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.DueAt)
            .ToListAsync();

        return Ok(reminders);
    }
}
