using CleanArchitecture.Application.Common.Interfaces;

namespace CleanArchitecture.Infrastructure.Shared.Services;

/// <summary>
/// DateTime service implementation
/// </summary>
public class DateTimeService : IDateTime
{
    public DateTime Now => DateTime.Now;
    public DateTime UtcNow => DateTime.UtcNow;
}
