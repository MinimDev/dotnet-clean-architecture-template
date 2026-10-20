using CleanArchitecture.Domain.Common;
using CleanArchitecture.Domain.Entities;

namespace CleanArchitecture.Domain.Events;

/// <summary>
/// Domain Event yang dipicu saat sebuah notifikasi baru dibuat
/// </summary>
public class NotificationCreatedEvent : IDomainEvent
{
    public Notification Notification { get; }
    public DateTime OccurredOn { get; }

    public NotificationCreatedEvent(Notification notification)
    {
        Notification = notification;
        OccurredOn = DateTime.UtcNow;
    }
}
