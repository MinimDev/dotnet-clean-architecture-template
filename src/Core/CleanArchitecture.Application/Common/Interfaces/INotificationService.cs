using CleanArchitecture.Domain.Enums;

namespace CleanArchitecture.Application.Common.Interfaces;

/// <summary>
/// Kontrak service untuk mengirim, broadcast, dan mengelola status notifikasi in-app
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Mengirim notifikasi ke pengguna tertentu
    /// </summary>
    Task SendAsync(
        string userId,
        string title,
        string message,
        NotificationType type = NotificationType.Info,
        string? actionUrl = null,
        CancellationToken ct = default);

    /// <summary>
    /// Mempublikasikan notifikasi secara broadcast ke seluruh pengguna
    /// </summary>
    Task BroadcastAsync(
        string title,
        string message,
        NotificationType type = NotificationType.Info,
        string? actionUrl = null,
        CancellationToken ct = default);

    /// <summary>
    /// Menandai satu notifikasi sebagai telah dibaca oleh pengguna tertentu
    /// </summary>
    Task MarkAsReadAsync(Guid notificationId, string userId, CancellationToken ct = default);

    /// <summary>
    /// Menandai seluruh notifikasi milik pengguna sebagai telah dibaca
    /// </summary>
    Task MarkAllAsReadAsync(string userId, CancellationToken ct = default);

    /// <summary>
    /// Mendapatkan total notifikasi yang belum dibaca (unread) untuk pengguna tertentu
    /// </summary>
    Task<int> GetUnreadCountAsync(string userId, CancellationToken ct = default);
}
