using VitaTrack.Api.Dashboard.DTOs;

namespace VitaTrack.Api.Abstractions
{
    public interface IDashboardService
    {
        Task<DashboardDailyDto> GetDailyDashboardAsync(long userId, string dateString, CancellationToken cancellationToken = default);
    }
}
