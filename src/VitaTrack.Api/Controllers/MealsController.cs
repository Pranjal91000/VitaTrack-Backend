using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VitaTrack.Api.Meals.DTOs;
using VitaTrack.Core.Entities;
using VitaTrack.Infrastructure.Persistence;

namespace VitaTrack.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MealsController : ControllerBase
{
    private readonly VitaTrackDbContext _context;

    public MealsController(VitaTrackDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<DailyMealsDto>> GetDailyMeals([FromQuery] string date, CancellationToken cancellationToken)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdString == null) return Unauthorized();
        var userId = long.Parse(userIdString);

        if (!DateOnly.TryParse(date, out var parsedDate))
            return BadRequest("Invalid date format (yyyy-MM-dd)");

        var meals = await _context.Meals
            .Include(m => m.MealFoods)
            .ThenInclude(mf => mf.Food)
            .Include(m => m.MealSlot)
            .Where(m => m.UserId == userId && m.Date == parsedDate)
            .ToListAsync(cancellationToken);

        var mealDtos = meals.Select(MapMealToDto).ToList();

        return Ok(new DailyMealsDto(mealDtos));
    }

    [HttpPost]
    public async Task<ActionResult<MealDto>> CreateMeal([FromBody] CreateMealRequest request, CancellationToken cancellationToken)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdString == null) return Unauthorized();
        var userId = long.Parse(userIdString);

        if (!await CanUseMealSlotAsync(userId, request.MealSlotId, cancellationToken))
            return BadRequest("Invalid meal slot.");

        var meal = new Meal
        {
            UserId = userId,
            Date = request.Date,
            MealSlotId = request.MealSlotId,
            Notes = request.Notes
        };

        var foodIds = request.Foods.Select(f => f.FoodId).ToList();
        var foods = await _context.Foods
            .Where(f => foodIds.Contains(f.Id))
            .ToListAsync(cancellationToken);

        if (foods.Count != foodIds.Distinct().Count())
            return BadRequest("One or more foods not found.");

        foreach (var item in request.Foods)
        {
            var food = foods.First(f => f.Id == item.FoodId);
            meal.MealFoods.Add(new MealFood
            {
                FoodId = food.Id,
                Quantity = item.Quantity,
                Food = food
            });
        }

        _context.Meals.Add(meal);
        await _context.SaveChangesAsync(cancellationToken);

        await _context.Entry(meal).Reference(m => m.MealSlot).LoadAsync(cancellationToken);
        await _context.Entry(meal).Collection(m => m.MealFoods).Query().Include(mf => mf.Food).LoadAsync(cancellationToken);

        return CreatedAtAction(nameof(GetDailyMeals), new { date = meal.Date.ToString("yyyy-MM-dd") }, MapMealToDto(meal));
    }

    [HttpDelete("{mealId}")]
    public async Task<IActionResult> DeleteMeal(long mealId, CancellationToken cancellationToken)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdString == null) return Unauthorized();
        var userId = long.Parse(userIdString);

        var meal = await _context.Meals.FindAsync(new object[] { mealId }, cancellationToken);
        if (meal == null) return NotFound();
        if (meal.UserId != userId) return Forbid();

        _context.Meals.Remove(meal);
        await _context.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpPut("{mealId}/foods/{foodId}")]
    public async Task<ActionResult<NutrientSummaryDto>> UpdateMealFood(long mealId, long foodId, [FromBody] UpdateMealFoodRequest request, CancellationToken cancellationToken)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdString == null) return Unauthorized();
        var userId = long.Parse(userIdString);

        var meal = await _context.Meals.FindAsync(new object[] { mealId }, cancellationToken);
        if (meal == null) return NotFound("Meal not found");
        if (meal.UserId != userId) return Forbid();

        var mealFood = await _context.MealFoods
            .Include(mf => mf.Food)
            .FirstOrDefaultAsync(mf => mf.MealId == mealId && mf.FoodId == foodId, cancellationToken);

        if (mealFood == null) return NotFound("Food item not found inside meal");

        if (request.Quantity <= 0)
        {
            _context.MealFoods.Remove(mealFood);
            await _context.SaveChangesAsync(cancellationToken);
            return Ok(new NutrientSummaryDto(0, 0, 0, 0));
        }

        mealFood.Quantity = request.Quantity;
        await _context.SaveChangesAsync(cancellationToken);

        var totals = new NutrientSummaryDto(
                (int)((decimal)mealFood.Quantity * (decimal)mealFood.Food.Calories),
                (decimal)mealFood.Quantity * mealFood.Food.ProteinG,
                (decimal)mealFood.Quantity * mealFood.Food.CarbsG,
                (decimal)mealFood.Quantity * mealFood.Food.FatG
        );

        return Ok(totals);
    }

    private async Task<bool> CanUseMealSlotAsync(long userId, long mealSlotId, CancellationToken cancellationToken)
    {
        return await _context.MealSlots.AnyAsync(
            s => s.Id == mealSlotId && (s.UserId == null || s.UserId == userId),
            cancellationToken);
    }

    private static MealDto MapMealToDto(Meal m)
    {
        var foodDtos = m.MealFoods.Select(mf =>
        {
            var foodDto = new FoodDto(
                mf.Food.Id,
                mf.Food.Name,
                mf.Food.ServingSize,
                mf.Food.Unit,
                mf.Food.Calories,
                mf.Food.ProteinG,
                mf.Food.CarbsG,
                mf.Food.FatG
            );

            var totals = new NutrientSummaryDto(
                (int)((decimal)mf.Quantity * (decimal)mf.Food.Calories),
                (decimal)mf.Quantity * mf.Food.ProteinG,
                (decimal)mf.Quantity * mf.Food.CarbsG,
                (decimal)mf.Quantity * mf.Food.FatG
            );

            return new MealFoodDto(mf.Id, foodDto, mf.Quantity, totals);
        }).ToList();

        var grandTotal = new NutrientSummaryDto(
             foodDtos.Sum(x => x.Totals.Calories),
             foodDtos.Sum(x => x.Totals.ProteinG),
             foodDtos.Sum(x => x.Totals.CarbsG),
             foodDtos.Sum(x => x.Totals.FatG)
        );

        return new MealDto(m.Id, m.MealSlotId, m.MealSlot.Name, m.Date, m.Notes, foodDtos, grandTotal);
    }
}
