namespace CleanArchitecture.Application.Common.Interfaces.Jobs;

/// <summary>
/// Example background job interface.
/// This interface should be implemented in the Infrastructure layer and injected via Hangfire.
/// </summary>
public interface IExampleJob
{
    /// <summary>
    /// Executes the background task.
    /// </summary>
    /// <param name="message">A parameter passed to the job.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    Task ExecuteAsync(string message, CancellationToken cancellationToken);
}
