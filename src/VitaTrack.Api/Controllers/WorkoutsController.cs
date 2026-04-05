using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VitaTrack.Api.Workouts.DTOs;
using VitaTrack.Core.Entities;
using VitaTrack.Infrastructure.Persistence;

namespace VitaTrack.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WorkoutsController : ControllerBase
{
    private readonly VitaTrackDbContext _context;

    public WorkoutsController(VitaTrackDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<DailyWorkoutsDto>> GetWorkouts([FromQuery] string date, CancellationToken cancellationToken)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdString == null) return Unauthorized();
        var userId = long.Parse(userIdString);

        if (!DateOnly.TryParse(date, out var parsedDate))
            return BadRequest("Invalid date format (yyyy-MM-dd)");

        var workouts = await _context.Workouts
           .Include(w => w.Exercises).ThenInclude(we => we.Exercise)
           .Include(w => w.Exercises).ThenInclude(we => we.Sets)
           .Where(w => w.UserId == userId && w.Date == parsedDate && !w.IsTemplate)
           .ToListAsync(cancellationToken);

        var dtos = workouts.Select(w =>
        {
            var exercises = w.Exercises.OrderBy(e => e.Order).Select(we =>
            {
                var sets = we.Sets.OrderBy(s => s.SetNumber).Select(s =>
                {
                    decimal? oneRepMax = null;
                    if (s.WeightKg.HasValue && s.Reps.HasValue && s.Reps > 0)
                    {
                        oneRepMax = s.WeightKg.Value * (1 + (decimal)s.Reps.Value / 30m);
                    }
                    return new SetDto(s.SetNumber, s.Reps, s.WeightKg, s.DurationSeconds, s.Rpe, oneRepMax, s.DistanceKm, s.ElevationGainM, s.PaceMinPerKm, s.Pace);
                }).ToList();
                return new WorkoutExerciseDto(we.ExerciseId, we.Exercise?.Name ?? "Unknown", we.Order, sets);
            }).ToList();

            decimal volume = exercises.Sum(e => e.Sets.Sum(s => (s.WeightKg ?? 0) * (s.Reps ?? 0)));

            return new WorkoutDto(w.Id, w.Name, w.Date, w.DurationMinutes, exercises, volume, w.RecurrencePattern, w.IsTemplate);
        }).ToList();

        return Ok(new DailyWorkoutsDto(dtos));
    }

    [HttpPost]
    public async Task<ActionResult<WorkoutDto>> CreateWorkout([FromBody] CreateWorkoutRequest request, CancellationToken cancellationToken)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdString == null) return Unauthorized();
        var userId = long.Parse(userIdString);

        var workout = new Workout
        {
            UserId = userId,
            Date = request.Date,
            Name = request.Name,
            DurationMinutes = request.DurationMinutes,
            Notes = request.Notes,
            RecurrencePattern = null,
            IsTemplate = false
        };

        int order = 1;
        var exerciseIds = request.Exercises.Select(e => e.ExerciseId).Distinct().ToList();
        var exercises = await _context.Exercises
            .Where(e => exerciseIds.Contains(e.Id))
            .ToListAsync(cancellationToken);

        foreach (var exReq in request.Exercises)
        {
            var we = new WorkoutExercise
            {
                ExerciseId = exReq.ExerciseId,
                Order = order++
            };

            foreach (var setReq in exReq.Sets)
            {
                we.Sets.Add(new Set
                {
                    SetNumber = setReq.SetNumber,
                    Reps = setReq.Reps,
                    WeightKg = setReq.WeightKg,
                    DurationSeconds = setReq.DurationSeconds,
                    Rpe = setReq.Rpe,
                    DistanceKm = setReq.DistanceKm,
                    ElevationGainM = setReq.ElevationGainM,
                    PaceMinPerKm = setReq.PaceMinPerKm
                });
            }
            workout.Exercises.Add(we);
        }

        _context.Workouts.Add(workout);
        await _context.SaveChangesAsync(cancellationToken);

        var dtos = workout.Exercises.Select(we =>
        {
            var exName = exercises.FirstOrDefault(e => e.Id == we.ExerciseId)?.Name ?? "Unknown";
            var sets = we.Sets.Select(s =>
            {
                decimal? oneRepMax = null;
                if (s.WeightKg.HasValue && s.Reps.HasValue && s.Reps > 0)
                {
                    oneRepMax = s.WeightKg.Value * (1 + (decimal)s.Reps.Value / 30m);
                }
                return new SetDto(s.SetNumber, s.Reps, s.WeightKg, s.DurationSeconds, s.Rpe, oneRepMax, s.DistanceKm, s.ElevationGainM, s.PaceMinPerKm, s.Pace);
            }).ToList();
            return new WorkoutExerciseDto(we.ExerciseId, exName, we.Order, sets);
        }).ToList();

        decimal volume = dtos.Sum(e => e.Sets.Sum(s => (s.WeightKg ?? 0) * (s.Reps ?? 0)));
        var result = new WorkoutDto(workout.Id, workout.Name, workout.Date, workout.DurationMinutes, dtos, volume, workout.RecurrencePattern, workout.IsTemplate);

        return CreatedAtAction(nameof(GetWorkouts), new { date = result.Date.ToString("yyyy-MM-dd") }, result);
    }

    [HttpPost("{id}/exercises")]
    public async Task<ActionResult<WorkoutDto>> AppendExercises(long id, [FromBody] AppendExercisesRequest request, CancellationToken cancellationToken)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdString == null) return Unauthorized();
        var userId = long.Parse(userIdString);

        var workout = await _context.Workouts
            .Include(w => w.Exercises).ThenInclude(we => we.Exercise)
            .Include(w => w.Exercises).ThenInclude(we => we.Sets)
            .FirstOrDefaultAsync(w => w.Id == id && w.UserId == userId, cancellationToken);

        if (workout == null) return NotFound("Workout not found");

        var exerciseIds = request.Exercises.Select(e => e.ExerciseId).Distinct().ToList();
        var exercises = await _context.Exercises
            .Where(e => exerciseIds.Contains(e.Id))
            .ToListAsync(cancellationToken);

        int order = workout.Exercises.Any() ? workout.Exercises.Max(e => e.Order) + 1 : 1;

        foreach (var exReq in request.Exercises)
        {
            var we = new WorkoutExercise
            {
                ExerciseId = exReq.ExerciseId,
                Order = order++
            };

            foreach (var setReq in exReq.Sets)
            {
                we.Sets.Add(new Set
                {
                    SetNumber = setReq.SetNumber,
                    Reps = setReq.Reps,
                    WeightKg = setReq.WeightKg,
                    DurationSeconds = setReq.DurationSeconds,
                    Rpe = setReq.Rpe,
                    DistanceKm = setReq.DistanceKm,
                    ElevationGainM = setReq.ElevationGainM,
                    PaceMinPerKm = setReq.PaceMinPerKm
                });
            }
            workout.Exercises.Add(we);
        }

        await _context.SaveChangesAsync(cancellationToken);

        var dtos = workout.Exercises.OrderBy(e => e.Order).Select(we =>
        {
            var exName = we.Exercise?.Name ?? exercises.FirstOrDefault(e => e.Id == we.ExerciseId)?.Name ?? "Unknown";
            var sets = we.Sets.OrderBy(s => s.SetNumber).Select(s =>
            {
                decimal? oneRepMax = null;
                if (s.WeightKg.HasValue && s.Reps.HasValue && s.Reps > 0)
                {
                    oneRepMax = s.WeightKg.Value * (1 + (decimal)s.Reps.Value / 30m);
                }
                return new SetDto(s.SetNumber, s.Reps, s.WeightKg, s.DurationSeconds, s.Rpe, oneRepMax, s.DistanceKm, s.ElevationGainM, s.PaceMinPerKm, s.Pace);
            }).ToList();
            return new WorkoutExerciseDto(we.ExerciseId, exName, we.Order, sets);
        }).ToList();

        decimal volume = dtos.Sum(e => e.Sets.Sum(s => (s.WeightKg ?? 0) * (s.Reps ?? 0)));
        var result = new WorkoutDto(workout.Id, workout.Name, workout.Date, workout.DurationMinutes, dtos, volume, workout.RecurrencePattern, workout.IsTemplate);

        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<WorkoutDto>> UpdateWorkout(long id, [FromBody] CreateWorkoutRequest request, CancellationToken cancellationToken)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdString == null) return Unauthorized();
        var userId = long.Parse(userIdString);

        var workout = await _context.Workouts
            .Include(w => w.Exercises).ThenInclude(we => we.Sets)
            .FirstOrDefaultAsync(w => w.Id == id && w.UserId == userId, cancellationToken);

        if (workout == null) return NotFound("Workout not found");

        workout.Name = request.Name;
        workout.Date = request.Date;
        workout.DurationMinutes = request.DurationMinutes;
        workout.Notes = request.Notes;
        workout.RecurrencePattern = null;
        workout.IsTemplate = false;

        _context.WorkoutExercises.RemoveRange(workout.Exercises);

        int order = 1;
        var exerciseIds = request.Exercises.Select(e => e.ExerciseId).Distinct().ToList();
        var exercises = await _context.Exercises
            .Where(e => exerciseIds.Contains(e.Id))
            .ToListAsync(cancellationToken);

        var newExercises = new List<WorkoutExercise>();

        foreach (var exReq in request.Exercises)
        {
            var we = new WorkoutExercise
            {
                ExerciseId = exReq.ExerciseId,
                Order = order++
            };

            foreach (var setReq in exReq.Sets)
            {
                we.Sets.Add(new Set
                {
                    SetNumber = setReq.SetNumber,
                    Reps = setReq.Reps,
                    WeightKg = setReq.WeightKg,
                    DurationSeconds = setReq.DurationSeconds,
                    Rpe = setReq.Rpe,
                    DistanceKm = setReq.DistanceKm,
                    ElevationGainM = setReq.ElevationGainM,
                    PaceMinPerKm = setReq.PaceMinPerKm
                });
            }
            newExercises.Add(we);
        }

        workout.Exercises = newExercises;
        await _context.SaveChangesAsync(cancellationToken);

        var dtos = workout.Exercises.Select(we =>
        {
            var exName = exercises.FirstOrDefault(e => e.Id == we.ExerciseId)?.Name ?? "Unknown";
            var sets = we.Sets.Select(s =>
            {
                decimal? oneRepMax = null;
                if (s.WeightKg.HasValue && s.Reps.HasValue && s.Reps > 0)
                {
                    oneRepMax = s.WeightKg.Value * (1 + (decimal)s.Reps.Value / 30m);
                }
                return new SetDto(s.SetNumber, s.Reps, s.WeightKg, s.DurationSeconds, s.Rpe, oneRepMax, s.DistanceKm, s.ElevationGainM, s.PaceMinPerKm, s.Pace);
            }).ToList();
            return new WorkoutExerciseDto(we.ExerciseId, exName, we.Order, sets);
        }).ToList();

        decimal volume = dtos.Sum(e => e.Sets.Sum(s => (s.WeightKg ?? 0) * (s.Reps ?? 0)));
        var result = new WorkoutDto(workout.Id, workout.Name, workout.Date, workout.DurationMinutes, dtos, volume, workout.RecurrencePattern, workout.IsTemplate);

        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteWorkout(long id, CancellationToken cancellationToken)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdString == null) return Unauthorized();
        var userId = long.Parse(userIdString);

        var workout = await _context.Workouts.FindAsync(new object[] { id }, cancellationToken);
        if (workout == null) return NotFound("Workout not found");
        if (workout.UserId != userId) return Forbid();

        _context.Workouts.Remove(workout);
        await _context.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    /// <summary>Returns per-day workout counts for the heatmap (actual logged workouts, not templates).</summary>
    [HttpGet("heatmap")]
    public async Task<ActionResult<IReadOnlyList<WorkoutHeatmapDayDto>>> GetWorkoutHeatmap(
        [FromQuery] string from,
        [FromQuery] string to,
        CancellationToken cancellationToken)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdString == null) return Unauthorized();
        var userId = long.Parse(userIdString);

        if (!DateOnly.TryParse(from, out var fromDate) || !DateOnly.TryParse(to, out var toDate))
            return BadRequest("Invalid date format (yyyy-MM-dd)");
        if (fromDate > toDate)
            return BadRequest("'from' must be on or before 'to'");

        var aggregates = await _context.Workouts
            .AsNoTracking()
            .Where(w => w.UserId == userId && !w.IsTemplate && w.Date >= fromDate && w.Date <= toDate)
            .GroupBy(w => w.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var rows = aggregates
            .OrderBy(x => x.Date)
            .Select(x => new WorkoutHeatmapDayDto(x.Date.ToString("yyyy-MM-dd"), x.Count))
            .ToList();

        return Ok(rows);
    }
}
