using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Application.Features.AuditLogs.Queries.GetAuditLogs;

/// <summary>
/// Handler untuk GetAuditLogsQuery
/// </summary>
public class GetAuditLogsQueryHandler : IRequestHandler<GetAuditLogsQuery, Result<PaginatedList<AuditLogDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetAuditLogsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PaginatedList<AuditLogDto>>> Handle(GetAuditLogsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var query = _context.AuditLogs.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(request.EntityName))
            {
                query = query.Where(a => a.EntityName == request.EntityName);
            }

            if (!string.IsNullOrWhiteSpace(request.Action))
            {
                query = query.Where(a => a.Action == request.Action);
            }

            if (!string.IsNullOrWhiteSpace(request.UserId))
            {
                query = query.Where(a => a.UserId == request.UserId || 
                                         (a.UserName != null && a.UserName.ToLower().Contains(request.UserId.ToLower())));
            }

            if (request.DateFrom.HasValue)
            {
                query = query.Where(a => a.Timestamp >= request.DateFrom.Value);
            }

            if (request.DateTo.HasValue)
            {
                query = query.Where(a => a.Timestamp <= request.DateTo.Value);
            }

            // Urutkan dari yang paling baru
            query = query.OrderByDescending(a => a.Timestamp);

            var dtos = query.Select(a => new AuditLogDto
            {
                Id = a.Id,
                EntityName = a.EntityName,
                Action = a.Action,
                PrimaryKey = a.PrimaryKey,
                OldValues = a.OldValues,
                NewValues = a.NewValues,
                AffectedColumns = a.AffectedColumns,
                UserId = a.UserId,
                UserName = a.UserName,
                Timestamp = a.Timestamp
            });

            var paginatedList = await PaginatedList<AuditLogDto>.CreateAsync(
                dtos,
                request.PageNumber,
                request.PageSize
            );

            return Result<PaginatedList<AuditLogDto>>.Success(paginatedList);
        }
        catch (Exception ex)
        {
            return Result<PaginatedList<AuditLogDto>>.Failure($"An error occurred while retrieving audit logs: {ex.Message}");
        }
    }
}
