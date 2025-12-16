using FinMate.api.Data;

using FinMate.DTOs;
using FinMate.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinMate.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GoalsController : ControllerBase
{
    private readonly FinMateDbContext _db;

    public GoalsController(FinMateDbContext db) => _db = db;

    // 1) Create goal
    [HttpPost]
    public async Task<IActionResult> CreateGoal([FromBody] CreateGoalDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var goal = new Goal
        {
            UserId = dto.UserId,
            Name = dto.Name.Trim(),
            TargetAmount = dto.TargetAmount,
            Deadline = dto.Deadline
        };

        _db.Goals.Add(goal);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetGoal), new { id = goal.Id }, goal);
    }

    // Get single goal + progress
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetGoal(int id)
    {
        var goal = await _db.Goals.FirstOrDefaultAsync(g => g.Id == id);
        if (goal == null) return NotFound();

        var progress = goal.TargetAmount <= 0 ? 0 : (goal.SavedAmount / goal.TargetAmount) * 100m;
        progress = Math.Clamp(progress, 0, 100);

        return Ok(new
        {
            goal.Id,
            goal.UserId,
            goal.Name,
            goal.TargetAmount,
            goal.SavedAmount,
            goal.Deadline,
            ProgressPercent = decimal.Round(progress, 2)
        });
    }

    // 2) Add saved amount (contribution)
    [HttpPost("{goalId:int}/save")]
    public async Task<IActionResult> AddSaving(int goalId, [FromBody] AddGoalSavingDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var goal = await _db.Goals.FirstOrDefaultAsync(g => g.Id == goalId);
        if (goal == null) return NotFound("Goal not found.");

        var contrib = new GoalContribution
        {
            GoalId = goalId,
            Amount = dto.Amount,
            Note = dto.Note?.Trim() ?? "",
            Date = dto.Date ?? DateTime.UtcNow
        };

        goal.SavedAmount += dto.Amount;

        _db.GoalContributions.Add(contrib);
        await _db.SaveChangesAsync();

        var progress = goal.TargetAmount <= 0 ? 0 : (goal.SavedAmount / goal.TargetAmount) * 100m;

        return Ok(new
        {
            Message = "Saved amount added.",
            goal.Id,
            goal.Name,
            goal.TargetAmount,
            goal.SavedAmount,
            ProgressPercent = decimal.Round(Math.Clamp(progress, 0, 100), 2)
        });
    }

    // list goals by user
    [HttpGet("user/{userId:int}")]
    public async Task<IActionResult> GetGoalsByUser(int userId)
    {
        var goals = await _db.Goals
            .Where(g => g.UserId == userId)
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync();

        var data = goals.Select(g =>
        {
            var p = g.TargetAmount <= 0 ? 0 : (g.SavedAmount / g.TargetAmount) * 100m;
            return new
            {
                g.Id,
                g.Name,
                g.TargetAmount,
                g.SavedAmount,
                g.Deadline,
                ProgressPercent = decimal.Round(Math.Clamp(p, 0, 100), 2)
            };
        });

        return Ok(data);
    }
}
