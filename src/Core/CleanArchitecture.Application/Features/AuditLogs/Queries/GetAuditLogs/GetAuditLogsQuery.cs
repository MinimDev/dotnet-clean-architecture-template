using CleanArchitecture.Application.Common.Models;
using MediatR;

namespace CleanArchitecture.Application.Features.AuditLogs.Queries.GetAuditLogs;

/// <summary>
/// Query untuk mendapatkan daftar audit logs dengan filter dan pagination
/// </summary>
public record GetAuditLogsQuery : IRequest<Result<PaginatedList<AuditLogDto>>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? EntityName { get; init; }
    public string? Action { get; init; }
    public string? UserId { get; init; }
    public DateTime? DateFrom { get; init; }
    public DateTime? DateTo { get; init; }
}

/// <summary>
/// Data Transfer Object untuk rekaman Audit Log
/// </summary>
public class AuditLogDto
{
    public Guid Id { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string PrimaryKey { get; set; } = string.Empty;
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string[]? AffectedColumns { get; set; }
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public DateTime Timestamp { get; set; }
}
