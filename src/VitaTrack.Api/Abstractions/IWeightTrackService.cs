using VitaTrack.Api.Models.WeightTrack;

namespace VitaTrack.Api.Abstractions
{
    public interface IWeightTrackService
    {
        Task<bool> SaveWeightAsync(WeightTrackSaveInputModel input);
        Task<bool> UpdateWeightAsync(WeightTrackUpdateInputModel input);
        Task<bool> DeleteWeightTrackedAsync(long id);
        Task<WeightTrackViewModel> GetWeightTracked(long id);
        Task<List<WeightTrackViewModel>> GetWeightTrackedHistory(DateOnly? FromDate, DateOnly ToDate);
    }
}
