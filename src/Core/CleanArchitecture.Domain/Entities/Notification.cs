using CleanArchitecture.Domain.Common;
using CleanArchitecture.Domain.Enums;
using CleanArchitecture.Domain.Events;

namespace CleanArchitecture.Domain.Entities;

/// <summary>
/// Entitas domain untuk sistem notifikasi in-app
/// </summary>
public class Notification : BaseEntity
{
    /// <summary>
    /// ID pengguna tujuan notifikasi (null = broadcast ke seluruh pengguna)
    /// </summary>
    public string? UserId { get; private set; }

    /// <summary>
    /// Judul notifikasi
    /// </summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>
    /// Isi pesan notifikasi
    /// </summary>
    public string Message { get; private set; } = string.Empty;

    /// <summary>
    /// Kategori/tingkat urgensi notifikasi (Info, Success, Warning, Error)
    /// </summary>
    public NotificationType Type { get; private set; } = NotificationType.Info;

    /// <summary>
    /// Status apakah notifikasi sudah dibaca
    /// </summary>
    public bool IsRead { get; private set; }

    /// <summary>
    /// URL rujukan / tautan tindakan yang dapat diklik oleh pengguna
    /// </summary>
    public string? ActionUrl { get; private set; }

    /// <summary>
    /// Waktu pembuatan notifikasi (UTC)
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Waktu ketika notifikasi dibaca (UTC)
    /// </summary>
    public DateTime? ReadAt { get; private set; }

    // Private parameterless constructor untuk EF Core
    private Notification()
    {
    }

    /// <summary>
    /// Factory method untuk membuat entitas notifikasi baru
    /// </summary>
    public static Notification Create(
        string? userId,
        string title,
        string message,
        NotificationType type = NotificationType.Info,
        string? actionUrl = null,
        DateTime? createdAt = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Notification title cannot be empty.", nameof(title));

        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Notification message cannot be empty.", nameof(message));

        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = title.Trim(),
            Message = message.Trim(),
            Type = type,
            IsRead = false,
            ActionUrl = string.IsNullOrWhiteSpace(actionUrl) ? null : actionUrl.Trim(),
            CreatedAt = createdAt ?? DateTime.UtcNow
        };

        notification.AddDomainEvent(new NotificationCreatedEvent(notification));

        return notification;
    }

    /// <summary>
    /// Menandai notifikasi sebagai telah dibaca
    /// </summary>
    public void MarkAsRead(DateTime? readAt = null)
    {
        if (!IsRead)
        {
            IsRead = true;
            ReadAt = readAt ?? DateTime.UtcNow;
        }
    }
}
