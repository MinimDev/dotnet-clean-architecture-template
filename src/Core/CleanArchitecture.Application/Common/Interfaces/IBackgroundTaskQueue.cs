namespace CleanArchitecture.Application.Common.Interfaces;

/// <summary>
/// Contract for a background task queue.
/// Enqueue work items to be processed asynchronously by a background service.
/// </summary>
public interface IBackgroundTaskQueue
{
    /// <summary>Enqueues a background work item to be processed.</summary>
    ValueTask QueueBackgroundWorkItemAsync(Func<CancellationToken, ValueTask> workItem);

    /// <summary>Dequeues and returns the next background work item.</summary>
    ValueTask<Func<CancellationToken, ValueTask>> DequeueAsync(CancellationToken cancellationToken);
}
