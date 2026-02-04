using EcoApi.Application.Interfaces;
using EcoApi.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EcoApi.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddTransient<IDateTime, DateTimeService>();

        return services;
    }
}
