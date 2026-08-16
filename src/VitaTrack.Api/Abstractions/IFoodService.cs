using VitaTrack.Api.Meals.DTOs;

namespace VitaTrack.Api.Abstractions
{
    public interface IFoodService
    {
        Task<List<FoodDto>> SearchFoodsAsync(string search, int limit, CancellationToken cancellationToken = default);
        Task<FoodDto> CreateFoodAsync(long? userId, CreateFoodRequest request, CancellationToken cancellationToken = default);
        Task<(FoodDto? Food, string? Error)> UpdateFoodAsync(long id, long userId, UpdateFoodRequest request, CancellationToken cancellationToken = default);
        Task<(bool Success, string? Error)> DeleteFoodAsync(long id, long userId, CancellationToken cancellationToken = default);
    }
}
