using VitaTrack.Api.Meals.DTOs;

namespace VitaTrack.Api.Abstractions
{
    public interface IMealService
    {
        Task<DailyMealsDto?> GetDailyMealsAsync(long userId, string dateString, CancellationToken cancellationToken = default);
        Task<(MealDto? Meal, string? Error)> CreateMealAsync(long userId, CreateMealRequest request, CancellationToken cancellationToken = default);
        Task<(bool Success, string? Error)> DeleteMealAsync(long mealId, long userId, CancellationToken cancellationToken = default);
        Task<(NutrientSummaryDto? Summary, string? Error)> UpdateMealFoodAsync(long mealId, long foodId, long userId, UpdateMealFoodRequest request, CancellationToken cancellationToken = default);
    }
}
