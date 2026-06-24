using VitaTrack.Api.Abstractions;
using VitaTrack.Api.Services;

namespace VitaTrack.Api.Extension
{
    public static class SeviceCollectionExtensions
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddScoped<IWeightTrackService, WeightTrackService>();
            return services;
        }
    }
}
