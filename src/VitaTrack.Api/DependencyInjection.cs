using System.Reflection;
using FluentValidation;


namespace VitaTrack.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());


        return services;
    }
}
