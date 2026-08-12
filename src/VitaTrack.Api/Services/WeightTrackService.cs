using VitaTrack.Api.Abstractions;
using VitaTrack.Api.Models.WeightTrack;
using VitaTrack.Core.Abstraction;
using VitaTrack.Core.Entities;

namespace VitaTrack.Api.Services
{
    public class WeightTrackService(IWeightTrackRepository weightTrackRepository) : IWeightTrackService
    {
        private readonly IWeightTrackRepository _weightTrackRepository = weightTrackRepository; 
        public async Task<bool> SaveWeightAsync(WeightTrackSaveInputModel input)
        {
            var data = new WeightTrack
            {
                DateRecordedOn = input.RecordedOn,
                Weight = input.Weight
            };

            return await _weightTrackRepository.SaveWeightAsync(data);
        }
        public async Task<bool> UpdateWeightAsync(WeightTrackUpdateInputModel input)
        {
            var data = new WeightTrack
            {
                Id = input.Id,
                DateRecordedOn = input.RecordedOn,
                Weight = input.Weight
            };

            return await _weightTrackRepository.UpdateWeightAsync(data);
        }
        public async Task<bool> DeleteWeightTrackedAsync(long id)
        {
            return await _weightTrackRepository.DeleteWeightAsync(id);
        }
        public async Task<WeightTrackViewModel> GetWeightTracked(long? id)
        {
            var data = await _weightTrackRepository.GetWeightById(id);
            return new WeightTrackViewModel
            {
                RecordedOn = data.RecordedOn,
                Weight = data.Weight
            };
        }
        public async Task<List<WeightTrackViewModel>> GetWeightTrackedHistory(DateOnly? FromDate, DateOnly ToDate)
        {
            var data = await _weightTrackRepository.GetWeightHistory();
            return data.Select(x => new WeightTrackViewModel
            {
                RecordedOn = x.RecordedOn,
                Weight = x.Weight
            }).ToList();
        }
    }
}
