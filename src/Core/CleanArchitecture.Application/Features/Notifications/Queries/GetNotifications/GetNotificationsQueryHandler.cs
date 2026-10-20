using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Application.Common.Models;
using CleanArchitecture.Application.Features.Notifications.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Application.Features.Notifications.Queries.GetNotifications;

/// <summary>
/// Handler untuk GetNotificationsQuery
/// </summary>
public class GetNotificationsQueryHandler : IRequestHandler<GetNotificationsQuery, Result<PaginatedList<NotificationDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetNotificationsQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<PaginatedList<NotificationDto>>> Handle(
        GetNotificationsQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = _currentUserService.UserId;

            // Kueri notifikasi: ditujukan untuk user ini ATAU broadcast ke semua user (UserId == null)
            var query = _context.Notifications
                .AsNoTracking()
                .Where(n => n.UserId == userId || n.UserId == null);

            if (request.UnreadOnly.HasValue && request.UnreadOnly.Value)
            {
                query = query.Where(n => !n.IsRead);
            }

            // Urutkan dari yang paling baru
            query = query.OrderByDescending(n => n.CreatedAt);

            var dtos = query.Select(n => new NotificationDto
            {
                Id = n.Id,
                UserId = n.UserId,
                Title = n.Title,
                Message = n.Message,
                Type = n.Type,
                IsRead = n.IsRead,
                ActionUrl = n.ActionUrl,
                CreatedAt = n.CreatedAt,
                ReadAt = n.ReadAt
            });

            var paginated = await PaginatedList<NotificationDto>.CreateAsync(
                dtos,
                request.PageNumber,
                request.PageSize
            );

            return Result<PaginatedList<NotificationDto>>.Success(paginated);
        }
        catch (Exception ex)
        {
            return Result<PaginatedList<NotificationDto>>.Failure($"Failed to retrieve notifications: {ex.Message}");
        }
    }
}
