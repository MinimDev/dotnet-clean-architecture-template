using Asp.Versioning;
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
    private readonly ILogger<JobsController> _logger;

    public JobsController(
        IBackgroundJobClient backgroundJobClient,
        IRecurringJobManager recurringJobManager,
        ILogger<JobsController> logger)
    {
        _backgroundJobClient = backgroundJobClient;
        _recurringJobManager = recurringJobManager;
        _logger = logger;
    }

    /// <summary>
    /// 1. Fire-and-Forget Job: Executed only once and almost immediately.
    /// Good for sending emails, triggering background processing, etc.
    /// </summary>
    [HttpPost("fire-and-forget")]
    public IActionResult CreateFireAndForgetJob([FromBody] JobRequest request)
    {
        var jobId = _backgroundJobClient.Enqueue(() =>
            ExecuteTask($"Fire-and-forget task for {request.Name} completed at {DateTime.UtcNow}"));

        return Accepted(new { JobId = jobId, Message = "Job has been enqueued." });
    }

    /// <summary>
    /// 2. Delayed Job: Executed only once but after a specific delay.
    /// Good for sending follow-up emails, reminding users, etc.
    /// </summary>
    [HttpPost("delayed")]
    public IActionResult CreateDelayedJob([FromBody] JobRequest request, [FromQuery] int delayMinutes = 1)
    {
        var jobId = _backgroundJobClient.Schedule(() =>
            ExecuteTask($"Delayed task for {request.Name} executed after {delayMinutes} minutes"),
            TimeSpan.FromMinutes(delayMinutes));

        return Accepted(new { JobId = jobId, Message = $"Job scheduled to run in {delayMinutes} minutes." });
    }

    /// <summary>
    /// 3. Recurring Job: Executed repeatedly on a CRON schedule.
    /// Good for daily reports, database cleanup, etc.
    /// </summary>
    [HttpPost("recurring")]
    public IActionResult CreateRecurringJob([FromBody] JobRequest request)
    {
        // Executes every day at 00:00 (midnight). You can change the CRON expression.
        _recurringJobManager.AddOrUpdate(
            "daily-report-job",
            () => ExecuteTask($"Daily recurring task for {request.Name}"),
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

    /// <summary>
    /// Simulated background task method. 
    /// Note: In real apps, this should be a public method in a Scoped/Transient interface (e.g., IEmailService).
    /// Hangfire needs the method and its parent class to be public to serialize and instantiate it.
    /// </summary>
    [NonAction]
    public void ExecuteTask(string message)
    {
        // Replace this with actual business logic or service call
        _logger.LogInformation("\n=== HANGFIRE JOB EXECUTED ===\n{Message}\n=============================\n", message);
    }
}

public class JobRequest
{
    public string Name { get; set; } = string.Empty;
}
