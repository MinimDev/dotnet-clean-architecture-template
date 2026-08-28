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
        // Date/Time
        services.AddTransient<IDateTime, DateTimeService>();

        // Background Task Queue
        services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
        services.AddHostedService<QueuedHostedService>();

        // Email Service
        services.AddTransient<IEmailService, SmtpEmailService>();

        // Background Jobs
        services.AddTransient<CleanArchitecture.Application.Common.Interfaces.Jobs.IExampleJob, CleanArchitecture.Infrastructure.Shared.Jobs.ExampleJob>();

        return services;
    }
}
