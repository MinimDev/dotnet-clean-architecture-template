using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Application.Common.Interfaces.Jobs;
using CleanArchitecture.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Infrastructure.Shared.Jobs;

/// <summary>
/// Example implementation of a background job with in-app notification publishing.
/// </summary>
public class ExampleJob : IExampleJob
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<ExampleJob> _logger;

    public ExampleJob(
        INotificationService notificationService,
        ILogger<ExampleJob> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task ExecuteAsync(string message, CancellationToken cancellationToken)
    {
        _logger.LogInformation("\n=== HANGFIRE JOB EXECUTED ===\n{Message}\nTime: {Time}\n=============================\n", 
            message, 
            DateTime.UtcNow);

        // Simulate some background processing work
        await Task.Delay(1000, cancellationToken);

        // Broadcast notification ke sistem
        await _notificationService.BroadcastAsync(
            title: "Background Task Finished",
            message: $"Job completed: {message}",
            type: NotificationType.Info,
            actionUrl: "/hangfire",
            ct: cancellationToken);
    }
}
