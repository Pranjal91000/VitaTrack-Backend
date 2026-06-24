using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VitaTrack.Core.Abstraction;
using VitaTrack.Core.Interfaces;
using VitaTrack.Infrastructure.Repositories;

namespace VitaTrack.Infrastructure.Extension;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"),
                builder => builder.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));


        services.AddScoped<IAnalyticsService, Services.AnalyticsService>();
        services.AddScoped<Services.IEmailService, Services.EmailService>();
        services.AddScoped<IIdentityService, Identity.IdentityService>();
        services.AddScoped<IWeightTrackRepository, WeightTrackRepository>();

        services.AddScoped<DbInitializer>();

        services.AddTransient<Jobs.DailyValuesJob>();
        services.AddTransient<Jobs.JobsService>();

        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(c => c.UseNpgsqlConnection(configuration.GetConnectionString("DefaultConnection"))));

        services.AddHangfireServer();

        return services;
    }
}
