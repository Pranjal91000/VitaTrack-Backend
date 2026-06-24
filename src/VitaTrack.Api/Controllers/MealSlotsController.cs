using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VitaTrack.Api.Meals.DTOs;
using VitaTrack.Core.Entities;
using VitaTrack.Infrastructure;

namespace VitaTrack.Api.Controllers;

[ApiController]
[Route("api/meal-slots")]
[Authorize]
public class MealSlotsController : ControllerBase
{
    private readonly AppDbContext _context;

    public MealSlotsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<MealSlotDto>>> GetSlots(CancellationToken cancellationToken)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdString == null) return Unauthorized();
        var userId = long.Parse(userIdString);

        var slots = await _context.MealSlots
            .Where(s => s.UserId == null || s.UserId == userId)
            .OrderBy(s => s.SortOrder)
            .ThenBy(s => s.Name)
            .Select(s => new MealSlotDto(s.Id, s.UserId, s.Name, s.SortOrder))
            .ToListAsync(cancellationToken);

        return Ok(slots);
    }

    [HttpPost]
    public async Task<ActionResult<MealSlotDto>> CreateSlot([FromBody] CreateMealSlotRequest request, CancellationToken cancellationToken)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdString == null) return Unauthorized();
        var userId = long.Parse(userIdString);

        var maxOrder = await _context.MealSlots
            .Where(s => s.UserId == userId)
            .Select(s => (int?)s.SortOrder)
            .MaxAsync(cancellationToken) ?? 99;

        var slot = new MealSlot
        {
            UserId = userId,
            Name = request.Name.Trim(),
            SortOrder = maxOrder + 1
        };

        _context.MealSlots.Add(slot);
        await _context.SaveChangesAsync(cancellationToken);

        var dto = new MealSlotDto(slot.Id, slot.UserId, slot.Name, slot.SortOrder);
        return Ok(dto);
    }
}
