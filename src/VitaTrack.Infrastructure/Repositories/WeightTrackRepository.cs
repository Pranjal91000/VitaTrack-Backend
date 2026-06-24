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
            await _appDbContext.WeightTracks.AddAsync(data);

            return await _appDbContext.SaveChangesAsync() > 0;
        }
        public async Task<bool> UpdateWeightAsync(WeightTrack input)
        {
            var data = await _appDbContext.WeightTracks.Where(x => x.Id == input.Id).FirstOrDefaultAsync();
            data.Weight = input.Weight;
            data.DateRecordedOn = input.DateRecordedOn;

            return await _appDbContext.SaveChangesAsync() > 0;
        }
        public async Task<bool> DeleteWeightAsync(long id)
        {
            var data = await _appDbContext.WeightTracks.FirstOrDefaultAsync();
            _appDbContext.WeightTracks.Remove(data);
            return await _appDbContext.SaveChangesAsync() > 0;
        }
        public async Task<GetWeightByDate> GetWeightById(long id)
        {
            var data = await _appDbContext.WeightTracks.Where(x => x.Id == id).FirstOrDefaultAsync();
            return new GetWeightByDate
            {
                RecordedOn = data.DateRecordedOn,
                Weight = data.Weight
            };
        }
        public async Task<List<GetWeightByDate>> GetWeightHistory()
        {
            var data = await _appDbContext.WeightTracks.Select(x => new GetWeightByDate
            {
               RecordedOn = x.DateRecordedOn,
               Weight =  x.Weight
            }).ToListAsync();

            return data;
        }
    }
}
