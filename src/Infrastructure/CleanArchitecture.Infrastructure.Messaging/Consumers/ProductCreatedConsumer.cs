using CleanArchitecture.Domain.Events;
using Hangfire;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Infrastructure.Messaging.Consumers;

/// <summary>
/// Consumer MassTransit untuk <see cref="ProductCreatedEvent"/>.
/// 
/// Dipanggil secara async setelah <see cref="CleanArchitecture.Application.Common.Behaviours.DomainEventPublishingBehaviour{TRequest,TResponse}"/>
/// mempublish event ke bus via <see cref="CleanArchitecture.Application.Common.Interfaces.IEventBus"/>.
/// 
/// Pattern: Loose coupling dengan Hangfire.
/// Consumer ini menerima event dari bus, lalu men-delegate pekerjaan berat ke Hangfire job
/// sehingga response API tetap cepat dan job dapat di-retry/di-monitor via Hangfire dashboard.
/// </summary>
public sealed class ProductCreatedConsumer : IConsumer<ProductCreatedEvent>
{
    private readonly IBackgroundJobClient _hangfire;
    private readonly ILogger<ProductCreatedConsumer> _logger;

    public ProductCreatedConsumer(
        IBackgroundJobClient hangfire,
        ILogger<ProductCreatedConsumer> logger)
    {
        _hangfire = hangfire;
        _logger = logger;
    }

    /// <summary>
    /// Dipanggil oleh MassTransit saat ada <see cref="ProductCreatedEvent"/> masuk ke bus.
    /// Event ini secara otomatis dikirimkan oleh DomainEventPublishingBehaviour.
    /// </summary>
    public Task Consume(ConsumeContext<ProductCreatedEvent> context)
    {
        var msg = context.Message;

        _logger.LogInformation(
            "[MassTransit] ProductCreatedConsumer received: ProductId={ProductId}, Name={ProductName}",
            msg.ProductId, msg.ProductName);

        // ─── ALUR LOOSE COUPLING & BACKGROUND PROCESSING ───────────────────────
        // Di sini kita sudah berada di luar request (HTTP Response sudah kembali ke user).
        // 
        // 1. Jika tugas ringan (misal update cache), bisa dilakukan langsung secara await.
        // 2. Jika tugas berat/berpotensi gagal (misal kirim email ke user, sinkronisasi ke sistem ERP), 
        //    sangat disarankan mendelegasikan-nya ke sistem Job Scheduler seperti Hangfire:
        //
        // Contoh Penggunaan Hangfire (Dapat dicoba jika Hangfire sudah jalan):
        // _hangfire.Enqueue<IEmailService>(x => x.SendWelcomeEmailAsync(msg.ProductId));
        //
        // Ini memastikan proses bisnis dijamin berjalan (retry-able) dan terpantau walau MassTransit consumer crash.
        // ─────────────────────────────────────────────────────────────────────────

        return Task.CompletedTask;
    }
}
