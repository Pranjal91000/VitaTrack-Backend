using VitaTrack.Api.Meals.DTOs;

namespace VitaTrack.Api.Abstractions
{
    public interface IMealSlotService
    {
        Task<List<MealSlotDto>> GetSlotsAsync(long userId, CancellationToken cancellationToken = default);
        Task<MealSlotDto> CreateSlotAsync(long userId, CreateMealSlotRequest request, CancellationToken cancellationToken = default);
    }
}
