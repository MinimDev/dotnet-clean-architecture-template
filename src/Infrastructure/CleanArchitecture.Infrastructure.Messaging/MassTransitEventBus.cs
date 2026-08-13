using CleanArchitecture.Application.Common.Interfaces;
using MassTransit;

namespace CleanArchitecture.Infrastructure.Messaging;

/// <summary>
/// Implementasi <see cref="IEventBus"/> menggunakan MassTransit <see cref="IPublishEndpoint"/>.
/// 
/// PERAN: 
/// Class ini bertindak sebagai Facade / Wrapper (Pola Adapter) agar Application Layer (Core)
/// tidak perlu bergantung langsung pada MassTransit. Application layer hanya memanggil IEventBus.
/// 
/// Di-register via <see cref="DependencyInjection.AddMessaging"/>.
/// Semua message yang dipublish akan dirouting oleh MassTransit ke consumer yang terdaftar
/// sesuai dengan transport yang dikonfigurasi (InMemory / RabbitMQ / ActiveMQ).
/// </summary>
public sealed class MassTransitEventBus : IEventBus
{
    private readonly IPublishEndpoint _publishEndpoint;

    public MassTransitEventBus(IPublishEndpoint publishEndpoint)
        => _publishEndpoint = publishEndpoint;

    /// <inheritdoc />
    public Task PublishAsync<T>(T message, CancellationToken cancellationToken = default)
        where T : class
    {
        // (object)message dan message.GetType() digunakan agar MassTransit mengirim 
        // tipe konkret (misal ProductCreatedEvent), bukan base interface-nya (IDomainEvent).
        // Ini memastikan Consumer dari tipe spesifik tersebut dapat menerimanya dengan benar.
        return _publishEndpoint.Publish((object)message, message.GetType(), cancellationToken);
    }
}
