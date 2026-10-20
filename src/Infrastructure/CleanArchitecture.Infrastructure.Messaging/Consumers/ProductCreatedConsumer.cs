using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Domain.Enums;
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
/// </summary>
public sealed class ProductCreatedConsumer : IConsumer<ProductCreatedEvent>
{
    private readonly IBackgroundJobClient _hangfire;
    private readonly INotificationService _notificationService;
    private readonly ILogger<ProductCreatedConsumer> _logger;

    public ProductCreatedConsumer(
        IBackgroundJobClient hangfire,
        INotificationService notificationService,
        ILogger<ProductCreatedConsumer> logger)
    {
        _hangfire = hangfire;
        _notificationService = notificationService;
        _logger = logger;
    }

    /// <summary>
    /// Dipanggil oleh MassTransit saat ada <see cref="ProductCreatedEvent"/> masuk ke bus.
    /// Event ini secara otomatis dikirimkan oleh DomainEventPublishingBehaviour.
    /// </summary>
    public async Task Consume(ConsumeContext<ProductCreatedEvent> context)
    {
        var msg = context.Message;

        _logger.LogInformation(
            "[MassTransit] ProductCreatedConsumer received: ProductId={ProductId}, Name={ProductName}",
            msg.ProductId, msg.ProductName);

        // Broadcast notifikasi in-app ke seluruh pengguna
        await _notificationService.BroadcastAsync(
            title: "New Product Added",
            message: $"Product '{msg.ProductName}' has been added to catalog.",
            type: NotificationType.Success,
            actionUrl: "/products",
            ct: context.CancellationToken);
    }
}
