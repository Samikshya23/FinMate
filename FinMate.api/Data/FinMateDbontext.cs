using FinMate.api.Models;
using FinMate.Models;
using Microsoft.EntityFrameworkCore;

namespace FinMate.api.Data;

public class FinMateDbContext : DbContext
{
    public FinMateDbContext(DbContextOptions<FinMateDbContext> options)
        : base(options)
    {
    }

    public DbSet<Expense> Expenses { get; set; } = null!;
    public DbSet<Income> Incomes { get; set; } = null!;
    public DbSet<Budget> Budgets { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Goal> Goals { get; set; } = null!;
    public DbSet<GoalContribution> GoalContributions { get; set; } = null!;
    public DbSet<Reminder> Reminders { get; set; } = null!;

}
