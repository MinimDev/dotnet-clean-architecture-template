using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Application.Features.Notifications.Commands.MarkAllAsRead;

/// <summary>
/// Command untuk menandai seluruh notifikasi milik pengguna aktif sebagai telah dibaca
/// </summary>
public record MarkAllNotificationsAsReadCommand : IRequest<Result>;

/// <summary>
/// Handler untuk MarkAllNotificationsAsReadCommand
/// </summary>
public class MarkAllNotificationsAsReadCommandHandler : IRequestHandler<MarkAllNotificationsAsReadCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public MarkAllNotificationsAsReadCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(MarkAllNotificationsAsReadCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = _currentUserService.UserId;

            var unreadNotifications = await _context.Notifications
                .Where(n => (n.UserId == userId || n.UserId == null) && !n.IsRead)
                .ToListAsync(cancellationToken);

            var now = DateTime.UtcNow;
            foreach (var notification in unreadNotifications)
            {
                notification.MarkAsRead(now);
            }

            if (unreadNotifications.Count > 0)
            {
                await _context.SaveChangesAsync(cancellationToken);
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to mark all notifications as read: {ex.Message}");
        }
    }
}
