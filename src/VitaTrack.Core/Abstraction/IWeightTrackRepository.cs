using VitaTrack.Core.Entities;
using VitaTrack.Core.Models;

namespace VitaTrack.Core.Abstraction
{
    public interface IWeightTrackRepository
    {
        Task<bool> SaveWeightAsync(WeightTrack data);
        Task<bool> UpdateWeightAsync(WeightTrack input);
        Task<bool> DeleteWeightAsync(long id);
        Task<GetWeightByDate> GetWeightById(long id);
        Task<List<GetWeightByDate>> GetWeightHistory();

    }
}
