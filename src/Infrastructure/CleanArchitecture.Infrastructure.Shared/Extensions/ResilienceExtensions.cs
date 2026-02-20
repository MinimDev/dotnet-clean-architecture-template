using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;

namespace CleanArchitecture.Infrastructure.Shared.Extensions;

/// <summary>
/// Extension methods for adding Polly resilience policies to HttpClient registrations.
/// </summary>
/// <example>
/// // In your DI registration:
/// services.AddHttpClient&lt;IMyService, MyService&gt;()
///         .AddStandardResilience();
///
/// // Or with the lightweight preset:
/// services.AddHttpClient&lt;IMyService, MyService&gt;()
///         .AddLightweightResilience();
/// </example>
public static class ResilienceExtensions
{
    /// <summary>
    /// Adds standard resilience handler:
    /// Retry (3x exponential backoff) + Circuit Breaker + Timeout.
    /// </summary>
    public static IHttpClientBuilder AddStandardResilience(this IHttpClientBuilder builder)
    {
        builder.AddStandardResilienceHandler(options =>
        {
            // Retry: 3 attempts with exponential backoff (2s, 4s, 8s)
            options.Retry.MaxRetryAttempts = 3;
            options.Retry.Delay = TimeSpan.FromSeconds(2);

            // Circuit Breaker: open after 50% failures in 30s window (min 5 requests)
            options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
            options.CircuitBreaker.MinimumThroughput = 5;
            options.CircuitBreaker.FailureRatio = 0.5;
            options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(15);

            // Timeout: per-attempt 10s, total 30s
            options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(10);
            options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(30);
        });

        return builder;
    }

    /// <summary>
    /// Adds a lightweight resilience handler optimized for internal/fast APIs.
    /// Retry (2x) + Timeout only (no circuit breaker).
    /// </summary>
    public static IHttpClientBuilder AddLightweightResilience(this IHttpClientBuilder builder)
    {
        builder.AddStandardResilienceHandler(options =>
        {
            options.Retry.MaxRetryAttempts = 2;
            options.Retry.Delay = TimeSpan.FromMilliseconds(500);
            options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(5);
            options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(15);
        });

        return builder;
    }
}
