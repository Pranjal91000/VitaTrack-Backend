using VitaTrack.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VitaTrack.Infrastructure.Persistence;

namespace VitaTrack.Infrastructure.Jobs;

public class DailyValuesJob
{
    private readonly VitaTrackDbContext _context;
    private readonly ILogger<DailyValuesJob> _logger;

    public DailyValuesJob(VitaTrackDbContext context, ILogger<DailyValuesJob> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task UpdateBmrAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting BMR Update Job");

        // In a real app, process in batches or use a stored procedure.
        // For 10k users, fetching all might be slightly heavy but doable in background.
        var users = await _context.Users.ToListAsync(cancellationToken);

        int updatedCount = 0;
        foreach (var user in users)
        {
            if (user.WeightKg.HasValue && user.HeightCm.HasValue && user.Age.HasValue)
            {
                // Mifflin-St Jeor Equation
                // Men: (10 × weight in kg) + (6.25 × height in cm) - (5 × age in years) + 5
                // Women: (10 × weight in kg) + (6.25 × height in cm) - (5 × age in years) - 161
                // We don't have Gender stored in User entity in prompt spec!
                // Assuming "Male" default or adding +5 for now, or just an average.
                // Let's assume + 5 (Male) for simplicity as gender missing in spec.

                decimal bmr = (10m * user.WeightKg.Value) + (6.25m * user.HeightCm.Value) - (5m * user.Age.Value) + 5;

                if (user.Bmr != bmr)
                {
                    user.Bmr = bmr;
                    // _context.Users.Update(user); // Tracking handles it
                    updatedCount++;
                }
            }
        }

        if (updatedCount > 0)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        _logger.LogInformation($"Completed BMR Update Job. Updated {updatedCount} users.");
    }
}
