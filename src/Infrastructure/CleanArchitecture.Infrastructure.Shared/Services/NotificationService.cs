using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Domain.Entities;
using CleanArchitecture.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Infrastructure.Shared.Services;

/// <summary>
/// Implementasi INotificationService untuk penyimpanan notifikasi ke database
/// </summary>
public class NotificationService : INotificationService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        IApplicationDbContext context,
        ILogger<NotificationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SendAsync(
        string userId,
        string title,
        string message,
        NotificationType type = NotificationType.Info,
        string? actionUrl = null,
        CancellationToken ct = default)
    {
        try
        {
            var notification = Notification.Create(userId, title, message, type, actionUrl);
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("In-App notification '{Title}' sent to user {UserId}.", title, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send notification '{Title}' to user {UserId}.", title, userId);
            throw;
        }
    }

    public async Task BroadcastAsync(
        string title,
        string message,
        NotificationType type = NotificationType.Info,
        string? actionUrl = null,
        CancellationToken ct = default)
    {
        try
        {
            var notification = Notification.Create(null, title, message, type, actionUrl);
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("In-App broadcast notification '{Title}' published to all users.", title);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to broadcast notification '{Title}'.", title);
            throw;
        }
    }

    public async Task MarkAsReadAsync(Guid notificationId, string userId, CancellationToken ct = default)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId, ct);

        if (notification != null && (notification.UserId == null || notification.UserId == userId))
        {
            notification.MarkAsRead();
            await _context.SaveChangesAsync(ct);
        }
    }

    public async Task MarkAllAsReadAsync(string userId, CancellationToken ct = default)
    {
        var unread = await _context.Notifications
            .Where(n => (n.UserId == null || n.UserId == userId) && !n.IsRead)
            .ToListAsync(ct);

        if (unread.Count > 0)
        {
            var now = DateTime.UtcNow;
            foreach (var item in unread)
            {
                item.MarkAsRead(now);
            }
            await _context.SaveChangesAsync(ct);
        }
    }

    public async Task<int> GetUnreadCountAsync(string userId, CancellationToken ct = default)
    {
        return await _context.Notifications
            .AsNoTracking()
            .Where(n => (n.UserId == null || n.UserId == userId) && !n.IsRead)
            .CountAsync(ct);
    }
}
