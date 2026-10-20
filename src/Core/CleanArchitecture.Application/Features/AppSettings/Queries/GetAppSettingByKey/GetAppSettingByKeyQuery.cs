using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Application.Common.Models;
using CleanArchitecture.Application.Features.AppSettings.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Application.Features.AppSettings.Queries.GetAppSettingByKey;

/// <summary>
/// Query untuk mendapatkan konfigurasi berdasarkan Key
/// </summary>
public record GetAppSettingByKeyQuery(string Key) : IRequest<Result<AppSettingDto>>;

/// <summary>
/// Handler untuk GetAppSettingByKeyQuery
/// </summary>
public class GetAppSettingByKeyQueryHandler : IRequestHandler<GetAppSettingByKeyQuery, Result<AppSettingDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAppSettingByKeyQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<AppSettingDto>> Handle(GetAppSettingByKeyQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var setting = await _context.AppSettings
                .AsNoTracking()
                .Where(s => s.Key == request.Key)
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
                .FirstOrDefaultAsync(cancellationToken);

            if (setting == null)
            {
                return Result<AppSettingDto>.Failure($"Setting with key '{request.Key}' was not found.");
            }

            return Result<AppSettingDto>.Success(setting);
        }
        catch (Exception ex)
        {
            return Result<AppSettingDto>.Failure($"Failed to retrieve setting: {ex.Message}");
        }
    }
}
