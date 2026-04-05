using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VitaTrack.Api.Common.Models;
using VitaTrack.Api.Workouts.DTOs;
using VitaTrack.Core.Entities;
using VitaTrack.Infrastructure.Persistence;

namespace VitaTrack.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ExercisesController : ControllerBase
{
    private static readonly HashSet<string> AllowedVideoContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "video/mp4", "video/webm", "video/quicktime"
    };

    private readonly VitaTrackDbContext _context;
    private readonly IWebHostEnvironment _env;

    public ExercisesController(VitaTrackDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    private static string ExtensionForMime(string? mime) => mime?.ToLowerInvariant() switch
    {
        "video/mp4" => ".mp4",
        "video/webm" => ".webm",
        "video/quicktime" => ".mov",
        _ => ".bin"
    };

    private static ExerciseDto ToDto(Exercise e) => new(
        e.Id,
        e.Name,
        e.Type,
        e.MuscleGroups,
        e.MeasurementType,
        e.IsDefault,
        e.DemoMediaId.HasValue ? $"exercises/{e.Id}/demo-media" : null);

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<ExerciseDto>>>> GetExercises([FromQuery] string search = "", [FromQuery] int page = 1, [FromQuery] int limit = 20, CancellationToken cancellationToken = default)
    {
        var term = search?.ToLower() ?? "";

        var query = _context.Exercises.AsQueryable();

        if (!string.IsNullOrEmpty(term))
        {
            query = query.Where(e => e.Name.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var exercises = await query
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync(cancellationToken);

        var dtos = exercises.Select(ToDto).ToList();

        var paginatedList = new PaginatedList<ExerciseDto>(dtos, totalCount, page, limit);

        return Ok(new ApiResponse<List<ExerciseDto>>(paginatedList.Items, ResponseMeta.FromPagination(paginatedList)));
    }

    [HttpPost]
    public async Task<ActionResult<ExerciseDto>> CreateExercise([FromBody] CreateExerciseRequest request, CancellationToken cancellationToken)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdString == null) return Unauthorized();
        var userId = long.Parse(userIdString);

        var exercise = new Exercise
        {
            UserId = userId,
            Name = request.Name,
            Type = request.Type,
            MuscleGroups = request.MuscleGroups,
            MeasurementType = request.MeasurementType,
            IsDefault = false
        };

        _context.Exercises.Add(exercise);
        await _context.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetExercises), new { id = exercise.Id }, ToDto(exercise));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ExerciseDto>> UpdateExercise(long id, [FromBody] UpdateExerciseRequest request, CancellationToken cancellationToken)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdString == null) return Unauthorized();
        var userId = long.Parse(userIdString);

        var exercise = await _context.Exercises.FindAsync(new object[] { id }, cancellationToken);

        if (exercise == null) return NotFound("Exercise not found");
        if (exercise.UserId != userId) return Forbid();

        exercise.Name = request.Name;
        exercise.Type = request.Type;
        exercise.MuscleGroups = request.MuscleGroups;
        exercise.MeasurementType = request.MeasurementType;

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(ToDto(exercise));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteExercise(long id, CancellationToken cancellationToken)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdString == null) return Unauthorized();
        var userId = long.Parse(userIdString);

        var exercise = await _context.Exercises.FindAsync(new object[] { id }, cancellationToken);
        if (exercise == null) return NotFound("Exercise not found");
        if (exercise.UserId != userId) return Forbid();

        TryDeleteDemoFile(exercise, userId);
        exercise.DemoMediaId = null;
        exercise.DemoMediaFileName = null;
        exercise.DemoMediaContentType = null;

        _context.Exercises.Remove(exercise);
        await _context.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpPost("{id}/demo-media")]
    [RequestSizeLimit(100_000_000)]
    [RequestFormLimits(MultipartBodyLengthLimit = 100_000_000)]
    public async Task<ActionResult<ExerciseDto>> UploadDemoMedia(long id, IFormFile file, CancellationToken cancellationToken)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdString == null) return Unauthorized();
        var userId = long.Parse(userIdString);

        if (file == null || file.Length == 0)
            return BadRequest("File required");

        var contentType = file.ContentType;
        if (string.IsNullOrEmpty(contentType) || !AllowedVideoContentTypes.Contains(contentType))
            return BadRequest("Allowed types: video/mp4, video/webm, video/quicktime");

        var exercise = await _context.Exercises.FindAsync(new object[] { id }, cancellationToken);
        if (exercise == null) return NotFound("Exercise not found");
        if (exercise.UserId != userId) return Forbid();

        TryDeleteDemoFile(exercise, userId);

        var mediaId = Guid.NewGuid();
        var ext = ExtensionForMime(contentType);
        var userDir = Path.Combine(_env.ContentRootPath, "uploads", "exercise-media", userId.ToString());
        Directory.CreateDirectory(userDir);
        var physicalPath = Path.Combine(userDir, mediaId + ext);

        await using (var stream = System.IO.File.Create(physicalPath))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        exercise.DemoMediaId = mediaId;
        exercise.DemoMediaContentType = contentType;
        exercise.DemoMediaFileName = file.FileName;
        await _context.SaveChangesAsync(cancellationToken);

        return Ok(ToDto(exercise));
    }

    [HttpGet("{id}/demo-media")]
    public async Task<IActionResult> GetDemoMedia(long id, CancellationToken cancellationToken)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdString == null) return Unauthorized();
        var userId = long.Parse(userIdString);

        var exercise = await _context.Exercises.FindAsync(new object[] { id }, cancellationToken);
        if (exercise == null) return NotFound();
        if (exercise.UserId != userId) return Forbid();
        if (exercise.DemoMediaId is null || string.IsNullOrEmpty(exercise.DemoMediaContentType))
            return NotFound();

        var ext = ExtensionForMime(exercise.DemoMediaContentType);
        var path = Path.Combine(_env.ContentRootPath, "uploads", "exercise-media", userId.ToString(), exercise.DemoMediaId + ext);
        if (!System.IO.File.Exists(path))
            return NotFound();

        var stream = System.IO.File.OpenRead(path);
        return File(stream, exercise.DemoMediaContentType, enableRangeProcessing: true);
    }

    [HttpDelete("{id}/demo-media")]
    public async Task<ActionResult<ExerciseDto>> DeleteDemoMedia(long id, CancellationToken cancellationToken)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdString == null) return Unauthorized();
        var userId = long.Parse(userIdString);

        var exercise = await _context.Exercises.FindAsync(new object[] { id }, cancellationToken);
        if (exercise == null) return NotFound("Exercise not found");
        if (exercise.UserId != userId) return Forbid();

        TryDeleteDemoFile(exercise, userId);
        exercise.DemoMediaId = null;
        exercise.DemoMediaFileName = null;
        exercise.DemoMediaContentType = null;
        await _context.SaveChangesAsync(cancellationToken);

        return Ok(ToDto(exercise));
    }

    private void TryDeleteDemoFile(Exercise exercise, long userId)
    {
        if (exercise.DemoMediaId is null || string.IsNullOrEmpty(exercise.DemoMediaContentType))
            return;
        var ext = ExtensionForMime(exercise.DemoMediaContentType);
        var path = Path.Combine(_env.ContentRootPath, "uploads", "exercise-media", userId.ToString(), exercise.DemoMediaId + ext);
        if (System.IO.File.Exists(path))
        {
            try { System.IO.File.Delete(path); } catch { /* ignore */ }
        }
    }
}
