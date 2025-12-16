using System.ComponentModel.DataAnnotations;

namespace FinMate.Models;

public class GoalContribution
{
    public int Id { get; set; }

    public int GoalId { get; set; }
    public Goal? Goal { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    public DateTime Date { get; set; } = DateTime.UtcNow;

    [MaxLength(200)]
    public string Note { get; set; } = string.Empty;
}
