using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VitaTrack.Api.Meals.DTOs;
using VitaTrack.Core.Entities;
using VitaTrack.Infrastructure;

namespace VitaTrack.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FoodsController : ControllerBase
{
    private readonly AppDbContext _context;

    public FoodsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<FoodDto>>> SearchFoods([FromQuery] string search = "", [FromQuery] int limit = 10, CancellationToken cancellationToken = default)
    {
        var term = search?.ToLower() ?? "";

        var query = _context.Foods.AsQueryable();

        if (!string.IsNullOrEmpty(term))
        {
            query = query.Where(f => f.Name.ToLower().Contains(term));
        }

        var foods = await query.Take(limit).ToListAsync(cancellationToken);

        var dtos = foods.Select(f => new FoodDto(
            f.Id,
            f.Name,
            f.ServingSize,
            f.Unit,
            f.Calories,
            f.ProteinG,
            f.CarbsG,
            f.FatG
        )).ToList();

        return Ok(dtos);
    }

    [HttpPost]
    public async Task<ActionResult<FoodDto>> CreateFood([FromBody] CreateFoodRequest request, CancellationToken cancellationToken)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        // Foods can be global or user specific. If user is logged in, use their ID.
        var userId = userIdString != null ? long.Parse(userIdString) : (long?)null;

        var food = new Food
        {
            UserId = userId,
            Name = request.Name,
            ServingSize = request.ServingSize,
            Unit = request.Unit,
            Calories = request.Calories,
            ProteinG = request.ProteinG,
            CarbsG = request.CarbsG,
            FatG = request.FatG
        };

        _context.Foods.Add(food);
        await _context.SaveChangesAsync(cancellationToken);

        var result = new FoodDto(
            food.Id,
            food.Name,
            food.ServingSize,
            food.Unit,
            food.Calories,
            food.ProteinG,
            food.CarbsG,
            food.FatG
        );
        return CreatedAtAction(nameof(SearchFoods), new { search = result.Name }, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<FoodDto>> UpdateFood(long id, [FromBody] UpdateFoodRequest request, CancellationToken cancellationToken)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdString == null) return Unauthorized();
        var userId = long.Parse(userIdString);

        var food = await _context.Foods.FindAsync(new object[] { id }, cancellationToken);

        if (food == null) return NotFound("Food not found");
        if (food.UserId != userId) return Forbid();

        food.Name = request.Name;
        food.ServingSize = request.ServingSize;
        food.Unit = request.Unit;
        food.Calories = request.Calories;
        food.ProteinG = request.Protein;
        food.CarbsG = request.Carbs;
        food.FatG = request.Fat;

        _context.Foods.Update(food);
        await _context.SaveChangesAsync(cancellationToken);

        var result = new FoodDto(
            food.Id,
            food.Name,
            food.ServingSize,
            food.Unit,
            food.Calories,
            food.ProteinG,
            food.CarbsG,
            food.FatG
        );

        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFood(long id, CancellationToken cancellationToken)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdString == null) return Unauthorized();
        var userId = long.Parse(userIdString);

        var food = await _context.Foods.FindAsync(new object[] { id }, cancellationToken);
        if (food == null) return NotFound("Food not found");
        if (food.UserId != userId) return Forbid();

        _context.Foods.Remove(food);
        await _context.SaveChangesAsync(cancellationToken);

        return NoContent();
    }
}
