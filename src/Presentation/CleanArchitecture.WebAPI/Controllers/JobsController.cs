using Asp.Versioning;
using CleanArchitecture.Application.Common.Interfaces.Jobs;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitecture.WebAPI.Controllers;

/// <summary>
/// Boilerplate controller demonstrating how to use Hangfire Background Jobs.
/// </summary>
[Authorize(Roles = "Admin")]
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class JobsController : ControllerBase
{
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly IRecurringJobManager _recurringJobManager;

    public JobsController(
        IBackgroundJobClient backgroundJobClient,
        IRecurringJobManager recurringJobManager)
    {
        _backgroundJobClient = backgroundJobClient;
        _recurringJobManager = recurringJobManager;
    }

    /// <summary>
    /// 1. Fire-and-Forget Job: Executed only once and almost immediately.
    /// </summary>
    [HttpPost("fire-and-forget")]
    public IActionResult CreateFireAndForgetJob([FromBody] JobRequest request)
    {
        var jobId = _backgroundJobClient.Enqueue<IExampleJob>(job =>
            job.ExecuteAsync($"Fire-and-forget task for {request.Name}", CancellationToken.None));

        return Accepted(new { JobId = jobId, Message = "Job has been enqueued." });
    }

    /// <summary>
    /// 2. Delayed Job: Executed only once but after a specific delay.
    /// </summary>
    [HttpPost("delayed")]
    public IActionResult CreateDelayedJob([FromBody] JobRequest request, [FromQuery] int delayMinutes = 1)
    {
        var jobId = _backgroundJobClient.Schedule<IExampleJob>(job =>
            job.ExecuteAsync($"Delayed task for {request.Name}", CancellationToken.None),
            TimeSpan.FromMinutes(delayMinutes));

        return Accepted(new { JobId = jobId, Message = $"Job scheduled to run in {delayMinutes} minutes." });
    }

    /// <summary>
    /// 3. Recurring Job: Executed repeatedly on a CRON schedule.
    /// </summary>
    [HttpPost("recurring")]
    public IActionResult CreateRecurringJob([FromBody] JobRequest request)
    {
        // Executes every day at 00:00 (midnight).
        _recurringJobManager.AddOrUpdate<IExampleJob>(
            "daily-report-job",
            job => job.ExecuteAsync($"Daily recurring task for {request.Name}", CancellationToken.None),
            Cron.Daily);

        return Ok(new { Message = "Recurring job registered successfully." });
    }

    /// <summary>
    /// Trigger a previously registered recurring job immediately.
    /// </summary>
    [HttpPost("recurring/trigger")]
    public IActionResult TriggerRecurringJob()
    {
        _recurringJobManager.Trigger("daily-report-job");
        return Ok(new { Message = "Recurring job triggered." });
    }
}

public class JobRequest
{
    public string Name { get; set; } = string.Empty;
}
