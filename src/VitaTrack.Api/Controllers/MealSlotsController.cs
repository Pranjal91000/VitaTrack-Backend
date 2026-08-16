using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VitaTrack.Api.Abstractions;
using VitaTrack.Api.Meals.DTOs;

namespace VitaTrack.Api.Controllers;

[ApiController]
[Route("api/meal-slots")]
[Authorize]
public class MealSlotsController(IMealSlotService mealSlotService) : ControllerBase
{
    private readonly IMealSlotService _mealSlotService = mealSlotService;

    [HttpGet]
    public async Task<ActionResult<List<MealSlotDto>>> GetSlots(CancellationToken cancellationToken)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdString == null) return Unauthorized();
        var userId = long.Parse(userIdString);

        var slots = await _mealSlotService.GetSlotsAsync(userId, cancellationToken);
        return Ok(slots);
    }

    [HttpPost]
    public async Task<ActionResult<MealSlotDto>> CreateSlot([FromBody] CreateMealSlotRequest request, CancellationToken cancellationToken)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdString == null) return Unauthorized();
        var userId = long.Parse(userIdString);

        var dto = await _mealSlotService.CreateSlotAsync(userId, request, cancellationToken);
        return Ok(dto);
    }
}
