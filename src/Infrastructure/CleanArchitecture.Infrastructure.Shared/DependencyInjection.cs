using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Infrastructure.Shared.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArchitecture.Infrastructure.Shared;

/// <summary>
/// Dependency Injection registration untuk Shared services
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddSharedServices(this IServiceCollection services)
    {
        services.AddTransient<IDateTime, DateTimeService>();

        return services;
    }
}
