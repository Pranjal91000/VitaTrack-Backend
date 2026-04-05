using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VitaTrack.Api.Users.DTOs;
using VitaTrack.Infrastructure.Persistence;

namespace VitaTrack.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly VitaTrackDbContext _context;

    public UsersController(VitaTrackDbContext context)
    {
        _context = context;
    }

    [HttpGet("profile")]
    public async Task<ActionResult<UserProfileDto>> GetProfile(CancellationToken cancellationToken)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdString == null) return Unauthorized();
        var userId = long.Parse(userIdString);

        var user = await _context.Users.FindAsync(new object[] { userId }, cancellationToken);
        if (user == null) return NotFound("User not found");

        var dto = new UserProfileDto(
            user.Id,
            user.Email,
            user.Name,
            user.Age,
            user.WeightKg,
            user.HeightCm,
            user.Bmr
        );
        return Ok(dto);
    }

    [HttpPut("profile")]
    public async Task<ActionResult<UserProfileDto>> UpdateProfile([FromBody] UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdString == null) return Unauthorized();
        var userId = long.Parse(userIdString);

        var user = await _context.Users.FindAsync(new object[] { userId }, cancellationToken);
        if (user == null) return NotFound("User not found");

        if (request.Name != null) user.Name = request.Name;
        if (request.Age.HasValue) user.Age = request.Age.Value;
        if (request.WeightKg.HasValue) user.WeightKg = request.WeightKg.Value;
        if (request.HeightCm.HasValue) user.HeightCm = request.HeightCm.Value;

        // BMR Calculation (Mifflin-St Jeor) - assuming Male +5, Female -161. Defaulting to Male for now.
        if (user.WeightKg.HasValue && user.HeightCm.HasValue && user.Age.HasValue)
        {
            user.Bmr = (10m * user.WeightKg.Value) + (6.25m * user.HeightCm.Value) - (5m * user.Age.Value) + 5;
        }

        // _context.Users.Update(user); // Tracked
        await _context.SaveChangesAsync(cancellationToken);

        var dto = new UserProfileDto(
            user.Id,
            user.Email,
            user.Name,
            user.Age,
            user.WeightKg,
            user.HeightCm,
            user.Bmr
        );

        return Ok(dto);
    }
}
