using CleanArchitecture.Application.Common.Interfaces;
using Mediator;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Application.Common.Behaviours;

/// <summary>
/// Mediator Pipeline Behaviour untuk mem-publish Domain Events ke Event Bus (MassTransit).
/// 
/// ALUR KERJA (PIPELINE FLOW):
/// 1. Command masuk ke sistem (misal: CreateProductCommand).
/// 2. Pipeline ini mencegat (intercept) eksekusi sebelum masuk ke Handler.
/// 3. Memanggil `next()` untuk membiarkan Handler bekerja murni pada domain logic dan
///    menyimpan data via `UnitOfWork.SaveChangesAsync()`. (Tidak ada logic messaging di Handler!)
/// 4. Setelah Handler selesai dan data sukses tersimpan di database, Behaviour ini akan 
///    mengambil semua Domain Events (misal: ProductCreatedEvent) yang sempat dicatat (di-track) oleh EF Core.
/// 5. Terakhir, mem-publish event tersebut ke Message Broker (MassTransit) untuk dieksekusi secara asinkron
///    oleh service atau komponen lain tanpa menghalangi (blocking) response ke user.
/// </summary>
public class DomainEventPublishingBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull, IMessage
{
    private readonly ILogger<DomainEventPublishingBehaviour<TRequest, TResponse>> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventBus _eventBus;

    public DomainEventPublishingBehaviour(
        ILogger<DomainEventPublishingBehaviour<TRequest, TResponse>> logger,
        IUnitOfWork unitOfWork,
        IEventBus eventBus)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
        _eventBus = eventBus;
    }

    public async ValueTask<TResponse> Handle(TRequest request, MessageHandlerDelegate<TRequest, TResponse> next, CancellationToken cancellationToken)
    {
        // TAHAP 1: EKSEKUSI HANDLER UTAMA
        var response = await next(request, cancellationToken);

        // TAHAP 2: PENGUMPULAN EVENT
        var domainEvents = _unitOfWork.GetAndClearDomainEvents();

        if (!domainEvents.Any())
            return response;

        // TAHAP 3: PUBLISH KE EVENT BUS (MASSTRANSIT)
        foreach (var domainEvent in domainEvents)
        {
            _logger.LogInformation("[DomainEventPublisher] Meneruskan domain event ke message bus: {Event}", domainEvent.GetType().Name);
            await _eventBus.PublishAsync(domainEvent, cancellationToken);
        }

        return response;
    }
}
