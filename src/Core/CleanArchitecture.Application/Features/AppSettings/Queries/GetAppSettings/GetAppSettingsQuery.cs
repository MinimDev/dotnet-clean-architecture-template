using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Application.Common.Models;
using CleanArchitecture.Application.Features.AppSettings.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Application.Features.AppSettings.Queries.GetAppSettings;

/// <summary>
/// Query untuk mendapatkan daftar konfigurasi aplikasi (opsional filter per grup)
/// </summary>
public record GetAppSettingsQuery(string? Group = null) : IRequest<Result<List<AppSettingDto>>>;

/// <summary>
/// Handler untuk GetAppSettingsQuery
/// </summary>
public class GetAppSettingsQueryHandler : IRequestHandler<GetAppSettingsQuery, Result<List<AppSettingDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetAppSettingsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<AppSettingDto>>> Handle(GetAppSettingsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var query = _context.AppSettings.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(request.Group))
            {
                query = query.Where(s => s.Group.ToLower() == request.Group.ToLower());
            }

            var settings = await query
                .OrderBy(s => s.Group)
                .ThenBy(s => s.Key)
                .Select(s => new AppSettingDto
                {
                    Id = s.Id,
                    Key = s.Key,
                    Value = s.Value,
                    DataType = s.DataType,
                    Description = s.Description,
                    Group = s.Group,
                    IsReadOnly = s.IsReadOnly,
                    ModifiedAt = s.ModifiedAt ?? s.CreatedAt,
                    ModifiedBy = s.ModifiedBy ?? s.CreatedBy
                })
                .ToListAsync(cancellationToken);

            return Result<List<AppSettingDto>>.Success(settings);
        }
        catch (Exception ex)
        {
            return Result<List<AppSettingDto>>.Failure($"Failed to retrieve app settings: {ex.Message}");
        }
    }
}
