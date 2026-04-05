using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using VitaTrack.Core.Interfaces;
using VitaTrack.Infrastructure.Persistence;

namespace VitaTrack.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<VitaTrackDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"),
                builder => builder.MigrationsAssembly(typeof(VitaTrackDbContext).Assembly.FullName)));


        services.AddScoped<IAnalyticsService, VitaTrack.Infrastructure.Services.AnalyticsService>();
        services.AddScoped<VitaTrack.Infrastructure.Services.IEmailService, VitaTrack.Infrastructure.Services.EmailService>();
        services.AddScoped<IIdentityService, VitaTrack.Infrastructure.Identity.IdentityService>();

        services.AddScoped<VitaTrackDbContextInitialiser>();

        services.AddTransient<VitaTrack.Infrastructure.Jobs.DailyValuesJob>();
        services.AddTransient<VitaTrack.Infrastructure.Jobs.JobsService>();

        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(c => c.UseNpgsqlConnection(configuration.GetConnectionString("DefaultConnection"))));

        services.AddHangfireServer();

        return services;
    }
}
