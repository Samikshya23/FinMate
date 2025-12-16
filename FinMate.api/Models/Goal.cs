using System.ComponentModel.DataAnnotations;

namespace FinMate.Models;

public class Goal
{
    public int Id { get; set; }

    public int UserId { get; set; } // (later link with auth user)

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty; // "Laptop fund"

    [Range(0.01, double.MaxValue)]
    public decimal TargetAmount { get; set; }

    public decimal SavedAmount { get; set; } = 0;

    public DateTime? Deadline { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
