using System.ComponentModel.DataAnnotations;

namespace FinMate.DTOs;

public record CreateGoalDto(
    int UserId,
    [Required, MaxLength(100)] string Name,
    [Range(0.01, double.MaxValue)] decimal TargetAmount,
    DateTime? Deadline
);

public record AddGoalSavingDto(
    [Range(0.01, double.MaxValue)] decimal Amount,
    string? Note,
    DateTime? Date
);
