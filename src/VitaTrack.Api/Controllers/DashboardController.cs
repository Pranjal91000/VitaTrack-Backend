using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VitaTrack.Api.Dashboard.DTOs;
using VitaTrack.Api.Meals.DTOs;
using VitaTrack.Core.Interfaces;
using VitaTrack.Infrastructure;

namespace VitaTrack.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IAnalyticsService _analyticsService;

    public DashboardController(AppDbContext context, IAnalyticsService analyticsService)
    {
        _context = context;
        _analyticsService = analyticsService;
    }

    [HttpGet("daily")]
    public async Task<ActionResult<DashboardDailyDto>> GetDailyDashboard([FromQuery] string date, CancellationToken cancellationToken)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdString == null) return Unauthorized();
        var userId = long.Parse(userIdString);

        if (!DateOnly.TryParse(date, out var parsedDate))
            parsedDate = DateOnly.FromDateTime(DateTime.UtcNow);

        var meals = await _context.Meals
            .Include(m => m.MealFoods)
            .ThenInclude(mf => mf.Food)
            .Where(m => m.UserId == userId && m.Date == parsedDate)
            .ToListAsync(cancellationToken);

        decimal totalCalories = 0;
        decimal totalProtein = 0;
        decimal totalCarbs = 0;
        decimal totalFat = 0;

        foreach (var meal in meals)
        {
            foreach (var mf in meal.MealFoods)
            {
                totalCalories += (decimal)mf.Quantity * mf.Food.Calories;
                totalProtein += (decimal)mf.Quantity * mf.Food.ProteinG;
                totalCarbs += (decimal)mf.Quantity * mf.Food.CarbsG;
                totalFat += (decimal)mf.Quantity * mf.Food.FatG;
            }
        }

        var mealTotals = new NutrientSummaryDto((int)totalCalories, totalProtein, totalCarbs, totalFat);

        var workoutsCount = await _context.Workouts
            .CountAsync(w => w.UserId == userId && w.Date == parsedDate && !w.IsTemplate, cancellationToken);

        var user = await _context.Users.FindAsync(new object[] { userId }, cancellationToken);
        int calorieGoal = user?.Bmr.HasValue == true ? (int)(user.Bmr.Value * 1.2m) : 2000;

        var streak = await _analyticsService.GetWellnessStreakAsync(userId, cancellationToken);

        var stats = new List<QuickStat>
        {
            new QuickStat("Calories Consumed", ((int)totalCalories).ToString()),
            new QuickStat("Protein", $"{totalProtein:F0}g"),
            new QuickStat("Workouts", workoutsCount.ToString()),
            new QuickStat("Meals logged", meals.Count.ToString()),
        };

        return new DashboardDailyDto(
            parsedDate,
            mealTotals,
            workoutsCount,
            streak,
            calorieGoal,
            meals.Count,
            stats
        );
    }
}
