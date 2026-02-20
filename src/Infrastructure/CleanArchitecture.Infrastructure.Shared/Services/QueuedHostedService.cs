using CleanArchitecture.Application.Common.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Infrastructure.Shared.Services;

/// <summary>
/// Hosted service that processes queued background work items.
/// Inject <see cref="IBackgroundTaskQueue"/> anywhere to queue tasks.
/// </summary>
/// <example>
/// // Enqueue a background job from a controller or handler:
/// await _taskQueue.QueueBackgroundWorkItemAsync(async token =>
/// {
///     await emailService.SendAsync("user@example.com", "Subject", "Body", token);
/// });
/// </example>
public sealed class QueuedHostedService : BackgroundService
{
    private readonly IBackgroundTaskQueue _taskQueue;
    private readonly ILogger<QueuedHostedService> _logger;

    public QueuedHostedService(
        IBackgroundTaskQueue taskQueue,
        ILogger<QueuedHostedService> logger)
    {
        _taskQueue = taskQueue;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Queued Background Service started.");

        await ProcessQueueAsync(stoppingToken);
    }

    private async Task ProcessQueueAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var workItem = await _taskQueue.DequeueAsync(stoppingToken);

                _logger.LogInformation("Processing background work item.");

                await workItem(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Expected on shutdown
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred executing background work item.");
            }
        }

        _logger.LogInformation("Queued Background Service stopped.");
    }
}
