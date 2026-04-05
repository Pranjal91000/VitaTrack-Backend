using VitaTrack.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VitaTrack.Infrastructure.Persistence;

namespace VitaTrack.Infrastructure.Jobs;

public class JobsService
{
    private readonly VitaTrackDbContext _context;
    private readonly IEmailService _emailService;
    private readonly ILogger<JobsService> _logger;

    public JobsService(VitaTrackDbContext context, IEmailService emailService, ILogger<JobsService> logger)
    {
        _context = context;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task SendDailyRemindersAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Sending Daily Reminders...");
        var users = await _context.Users.ToListAsync(cancellationToken);
        foreach (var user in users)
        {
            await _emailService.SendEmailAsync(user.Email, "Don't forget to leverage VitaTrack!", "Log your meals and workouts today!");
        }
    }

    public async Task SendWeeklySummariesAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Sending Weekly Summaries...");
        var users = await _context.Users.ToListAsync(cancellationToken);
        foreach (var user in users)
        {
            await _emailService.SendEmailAsync(user.Email, "Your Weekly Summary", "You did great this week! View your dashboard for details.");
        }
    }
}
