using Microsoft.EntityFrameworkCore;
using VitaTrack.Core.Abstraction;
using VitaTrack.Core.Entities;
using VitaTrack.Core.Models;
namespace VitaTrack.Infrastructure.Repositories
{
    public class WeightTrackRepository(AppDbContext appDbContext) : IWeightTrackRepository
    {
        private readonly AppDbContext _appDbContext = appDbContext;

        public async Task<bool> SaveWeightAsync(WeightTrack data)
        {
            await _appDbContext.WeightTrackers.AddAsync(data);
            return await _appDbContext.SaveChangesAsync() > 0;
        }
        public async Task<bool> UpdateWeightAsync(WeightTrack input)
        {
            var data = await _appDbContext.WeightTrackers.Where(x => x.Id == input.Id).FirstOrDefaultAsync();
            if (data == null) return false;

            data.Weight = input.Weight;
            data.DateRecordedOn = input.DateRecordedOn;

            return await _appDbContext.SaveChangesAsync() > 0;
        }
        public async Task<bool> DeleteWeightAsync(long id)
        {
            var data = await _appDbContext.WeightTrackers.FirstOrDefaultAsync(x => x.Id == id);
            if (data == null) return false;

            _appDbContext.WeightTrackers.Remove(data);
            return await _appDbContext.SaveChangesAsync() > 0;
        }
        public async Task<GetWeightByDate> GetWeightById(long? id)
        {
            var data = await _appDbContext.WeightTrackers
                .Where(x => id == null || x.Id == id)
                .OrderByDescending(x => x.DateRecordedOn)
                .FirstOrDefaultAsync();

            if (data == null) return null!;

            return new GetWeightByDate
            {
                RecordedOn = data.DateRecordedOn,
                Weight = data.Weight
            };
        }
        public async Task<List<GetWeightByDate>> GetWeightHistory()
        {
            var data = await _appDbContext.WeightTrackers.Select(x => new GetWeightByDate
            {
               RecordedOn = x.DateRecordedOn,
               Weight =  x.Weight
            }).ToListAsync();

            return data;
        }
    }
}
