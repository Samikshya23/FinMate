using FinMate.api.Data;
using Microsoft.EntityFrameworkCore;

namespace FinMate.api.Services
{
    public class ReminderCheckerService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public ReminderCheckerService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();

                    var db = scope.ServiceProvider.GetRequiredService<FinMateDbContext>();
                    var email = scope.ServiceProvider.GetRequiredService<SmtpEmailService>();

                    var now = DateTime.Now; // local time

                    var dueReminders = await db.Reminders
                        .Where(r =>
                            !r.IsSent &&
                            r.SendEmail &&
                            r.DueAt <= now)
                        .ToListAsync(stoppingToken);

                    foreach (var r in dueReminders)
                    {
                        await email.SendEmailAsync(
                            r.EmailTo,
                            $"FinMate Reminder: {r.Title}",
                            string.IsNullOrWhiteSpace(r.Message)
                                ? r.Title
                                : r.Message
                        );

                        r.IsSent = true;
                        r.SentAt = DateTime.UtcNow;
                    }

                    if (dueReminders.Count > 0)
                        await db.SaveChangesAsync(stoppingToken);
                }
                catch
                {
                    // ignore errors, keep service alive
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}
