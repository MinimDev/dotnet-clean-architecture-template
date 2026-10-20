using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Application.Features.Notifications.Queries.GetUnreadCount;

/// <summary>
/// Query untuk mendapatkan jumlah notifikasi yang belum dibaca (unread count)
/// </summary>
public record GetUnreadCountQuery : IRequest<Result<int>>;

/// <summary>
/// Handler untuk GetUnreadCountQuery
/// </summary>
public class GetUnreadCountQueryHandler : IRequestHandler<GetUnreadCountQuery, Result<int>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetUnreadCountQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<int>> Handle(GetUnreadCountQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = _currentUserService.UserId;

            var unreadCount = await _context.Notifications
                .AsNoTracking()
                .Where(n => (n.UserId == userId || n.UserId == null) && !n.IsRead)
                .CountAsync(cancellationToken);

            return Result<int>.Success(unreadCount);
        }
        catch (Exception ex)
        {
            return Result<int>.Failure($"Failed to get unread count: {ex.Message}");
        }
    }
}
