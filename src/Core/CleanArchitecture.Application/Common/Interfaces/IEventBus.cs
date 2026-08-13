namespace CleanArchitecture.Application.Common.Interfaces;

/// <summary>
/// Abstraksi untuk publish integration events ke message bus (MassTransit).
/// Application layer tidak bergantung langsung pada MassTransit — hanya interface ini.
/// 
/// Implementasi: <see cref="CleanArchitecture.Infrastructure.Messaging.MassTransitEventBus"/>
/// 
/// Cara menggunakan (dari Command Handler):
/// <code>
/// await _eventBus.PublishAsync(new ProductCreatedMessage(entity.Id, entity.Name, DateTime.UtcNow), ct);
/// </code>
/// </summary>
public interface IEventBus
{
    /// <summary>
    /// Publish sebuah integration message ke message bus.
    /// Message akan diterima oleh semua consumer yang terdaftar untuk tipe T.
    /// </summary>
    Task PublishAsync<T>(T message, CancellationToken cancellationToken = default)
        where T : class;
}
