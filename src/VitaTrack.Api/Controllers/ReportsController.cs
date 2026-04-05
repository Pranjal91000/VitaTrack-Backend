using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VitaTrack.Api.Reports.DTOs;
using VitaTrack.Api.Workouts.DTOs;
using VitaTrack.Infrastructure.Persistence;

namespace VitaTrack.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly VitaTrackDbContext _context;

    public ReportsController(VitaTrackDbContext context)
    {
        _context = context;
    }

    [HttpGet("nutrition")]
    public async Task<ActionResult<NutritionReportDto>> GetNutritionReport([FromQuery] string from, [FromQuery] string to, CancellationToken cancellationToken)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdString == null) return Unauthorized();
        var userId = long.Parse(userIdString);

        if (!DateOnly.TryParse(from, out var fromDate) || !DateOnly.TryParse(to, out var toDate))
            return BadRequest("Invalid date format (yyyy-MM-dd)");
        if (toDate < fromDate)
            return BadRequest("To date cannot be before From date");

        var meals = await _context.Meals
            .Include(m => m.MealFoods)
            .ThenInclude(mf => mf.Food)
            .Include(m => m.MealSlot)
            .Where(m => m.UserId == userId && m.Date >= fromDate && m.Date <= toDate)
            .ToListAsync(cancellationToken);

        var groupedByDate = meals
            .GroupBy(m => m.Date)
            .Select(g => new NutritionReportItem(
                g.Key,
                g.Sum(m => m.MealFoods.Sum(mf => mf.Quantity * mf.Food.Calories)),
                g.Sum(m => m.MealFoods.Sum(mf => (decimal)mf.Quantity * mf.Food.ProteinG)),
                g.Sum(m => m.MealFoods.Sum(mf => (decimal)mf.Quantity * mf.Food.CarbsG)),
                g.Sum(m => m.MealFoods.Sum(mf => (decimal)mf.Quantity * mf.Food.FatG))
            ))
            .OrderBy(x => x.Date)
            .ToList();

        var slotAggregates = meals
            .GroupBy(m => new { m.MealSlotId, SlotName = m.MealSlot.Name })
            .Select(g => new NutritionSlotAggregateDto(
                g.Key.MealSlotId,
                g.Key.SlotName,
                g.Sum(m => m.MealFoods.Sum(mf => mf.Quantity * mf.Food.Calories)),
                g.Sum(m => m.MealFoods.Sum(mf => (decimal)mf.Quantity * mf.Food.ProteinG)),
                g.Sum(m => m.MealFoods.Sum(mf => (decimal)mf.Quantity * mf.Food.CarbsG)),
                g.Sum(m => m.MealFoods.Sum(mf => (decimal)mf.Quantity * mf.Food.FatG)),
                g.Count()
            ))
            .OrderByDescending(x => x.TotalCalories)
            .ToList();

        decimal avgProteinPct = 0;
        if (groupedByDate.Count > 0)
        {
            var proteinCalsPerDay = groupedByDate
                .Where(d => d.Calories > 0)
                .Select(d => (d.ProteinG * 4m) / d.Calories * 100m)
                .ToList();
            if (proteinCalsPerDay.Count > 0)
                avgProteinPct = Math.Round(proteinCalsPerDay.Average(), 1);
        }

        return Ok(new NutritionReportDto(fromDate, toDate, groupedByDate, slotAggregates, avgProteinPct));
    }

    [HttpGet("workouts")]
    public async Task<ActionResult<WorkoutReportDto>> GetWorkoutReport([FromQuery] string from, [FromQuery] string to, CancellationToken cancellationToken)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdString == null) return Unauthorized();
        var userId = long.Parse(userIdString);

        if (!DateOnly.TryParse(from, out var fromDate) || !DateOnly.TryParse(to, out var toDate))
            return BadRequest("Invalid date format (yyyy-MM-dd)");
        if (toDate < fromDate)
            return BadRequest("To date cannot be before From date");

        var workouts = await _context.Workouts
            .Include(w => w.Exercises).ThenInclude(we => we.Sets)
            .Include(w => w.Exercises).ThenInclude(we => we.Exercise)
            .Where(w => w.UserId == userId && w.Date >= fromDate && w.Date <= toDate && !w.IsTemplate)
            .ToListAsync(cancellationToken);

        var allExerciseRows = workouts.SelectMany(w => w.Exercises).ToList();

        var grouped = allExerciseRows
            .GroupBy(we => we.Exercise.Name)
            .Select(g => new WorkoutReportItem(
                g.Key,
                g.Sum(we => we.Sets.Sum(s => (s.WeightKg ?? 0) * (s.Reps ?? 0))),
                g.Sum(we => we.Sets.Count),
                g.SelectMany(we => we.Sets).Where(s => s.Rpe.HasValue).Select(s => s.Rpe!.Value).DefaultIfEmpty(0).Average()
            ))
            .OrderByDescending(x => x.TotalVolume)
            .ToList();

        var totalDurationMinutes = allExerciseRows.Sum(we => we.Sets.Sum(s => (s.DurationSeconds ?? 0) / 60));
        var totalDistanceKm = allExerciseRows.Sum(we => we.Sets.Sum(s => s.DistanceKm ?? 0));

        return Ok(new WorkoutReportDto(fromDate, toDate, grouped, workouts.Count, totalDurationMinutes, totalDistanceKm));
    }

    [HttpGet("exercises/{exerciseId}/monthly")]
    public async Task<ActionResult<ExerciseMonthlyReportDto>> GetExerciseMonthlyReport(long exerciseId, [FromQuery] string month, CancellationToken cancellationToken)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdString == null) return Unauthorized();
        var userId = long.Parse(userIdString);

        if (!DateTime.TryParse($"{month}-01", out var startOfMonth))
            return BadRequest("Invalid month format (yyyy-MM)");

        var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);
        var rangeFrom = DateOnly.FromDateTime(startOfMonth);
        var rangeTo = DateOnly.FromDateTime(endOfMonth);

        var workouts = await _context.Workouts
            .Include(w => w.Exercises).ThenInclude(we => we.Sets)
            .Include(w => w.Exercises).ThenInclude(we => we.Exercise)
            .Where(w => w.UserId == userId && w.Date >= rangeFrom && w.Date <= rangeTo && w.Exercises.Any(e => e.ExerciseId == exerciseId))
            .ToListAsync(cancellationToken);

        var exercise = await _context.Exercises.FirstOrDefaultAsync(e => e.Id == exerciseId, cancellationToken);
        if (exercise == null) return NotFound("Exercise not found");

        var dailySummaries = new List<ExerciseDailySummaryDto>();

        var groupedByDate = workouts.GroupBy(w => w.Date).OrderBy(g => g.Key);

        foreach (var group in groupedByDate)
        {
            var sets = group.SelectMany(w => w.Exercises.Where(we => we.ExerciseId == exerciseId).SelectMany(we => we.Sets)).ToList();
            if (!sets.Any()) continue;

            decimal totalVolume = sets.Sum(s => (s.WeightKg ?? 0) * (s.Reps ?? 0));
            decimal maxWeight = sets.Max(s => s.WeightKg) ?? 0;
            decimal totalDistance = sets.Sum(s => s.DistanceKm) ?? 0;
            int totalDuration = sets.Sum(s => s.DurationSeconds) ?? 0;
            int totalReps = sets.Sum(s => s.Reps) ?? 0;

            decimal avgPace = 0;
            var paceSets = sets.Where(s => s.PaceMinPerKm.HasValue).ToList();
            if (paceSets.Any())
            {
                avgPace = paceSets.Average(s => s.PaceMinPerKm!.Value);
            }
            else if (totalDistance > 0 && totalDuration > 0)
            {
                avgPace = (decimal)totalDuration / 60m / totalDistance;
            }

            dailySummaries.Add(new ExerciseDailySummaryDto(
                group.Key,
                totalVolume,
                maxWeight,
                totalDistance,
                avgPace,
                totalDuration,
                totalReps
            ));
        }

        return Ok(new ExerciseMonthlyReportDto(exerciseId, exercise.Name, dailySummaries));
    }
}
