using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VitaTrack.Api.Abstractions;
using VitaTrack.Api.Meals.DTOs;

namespace VitaTrack.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FoodsController(IFoodService foodService) : ControllerBase
{
    private readonly IFoodService _foodService = foodService;

    [HttpGet]
    public async Task<ActionResult<List<FoodDto>>> SearchFoods([FromQuery] string search = "", [FromQuery] int limit = 10, CancellationToken cancellationToken = default)
    {
        var foods = await _foodService.SearchFoodsAsync(search, limit, cancellationToken);
        return Ok(foods);
    }

    [HttpPost]
    public async Task<ActionResult<FoodDto>> CreateFood([FromBody] CreateFoodRequest request, CancellationToken cancellationToken)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userId = userIdString != null ? long.Parse(userIdString) : (long?)null;

        var result = await _foodService.CreateFoodAsync(userId, request, cancellationToken);
        return CreatedAtAction(nameof(SearchFoods), new { search = result.Name }, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<FoodDto>> UpdateFood(long id, [FromBody] UpdateFoodRequest request, CancellationToken cancellationToken)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdString == null) return Unauthorized();
        var userId = long.Parse(userIdString);

        var (food, error) = await _foodService.UpdateFoodAsync(id, userId, request, cancellationToken);
        if (error == "NotFound") return NotFound("Food not found");
        if (error == "Forbid") return Forbid();

        return Ok(food);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFood(long id, CancellationToken cancellationToken)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdString == null) return Unauthorized();
        var userId = long.Parse(userIdString);

        var (success, error) = await _foodService.DeleteFoodAsync(id, userId, cancellationToken);
        if (error == "NotFound") return NotFound("Food not found");
        if (error == "Forbid") return Forbid();

        return NoContent();
    }
}
