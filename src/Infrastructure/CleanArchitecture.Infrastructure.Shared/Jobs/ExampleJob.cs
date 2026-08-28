using CleanArchitecture.Application.Common.Interfaces.Jobs;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Infrastructure.Shared.Jobs;

/// <summary>
/// Example implementation of a background job.
/// </summary>
public class ExampleJob : IExampleJob
{
    private readonly ILogger<ExampleJob> _logger;

    public ExampleJob(ILogger<ExampleJob> logger)
    {
        _logger = logger;
    }

    public async Task ExecuteAsync(string message, CancellationToken cancellationToken)
    {
        _logger.LogInformation("\n=== HANGFIRE JOB EXECUTED ===\n{Message}\nTime: {Time}\n=============================\n", 
            message, 
            DateTime.UtcNow);

        // Simulate some background processing work
        await Task.Delay(1000, cancellationToken);
    }
}
