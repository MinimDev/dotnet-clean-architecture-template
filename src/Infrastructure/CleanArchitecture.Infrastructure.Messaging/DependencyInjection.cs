using System.Reflection;
using CleanArchitecture.Application.Common.Interfaces;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArchitecture.Infrastructure.Messaging;

/// <summary>
/// Dependency Injection registration untuk Messaging layer (MassTransit).
///
/// ─── Cara Menggunakan ────────────────────────────────────────────────────────
/// Di Program.cs:
/// <code>
///     builder.Services.AddMessaging(builder.Configuration);
/// </code>
///
/// ─── Konfigurasi Transport ───────────────────────────────────────────────────
/// Ubah nilai "MassTransit:Transport" di appsettings.json:
///
///   "InMemory"  → default, cocok untuk lokal/dev/testing (tidak perlu server broker)
///   "RabbitMQ"  → production/staging dengan RabbitMQ server
///   "ActiveMQ"  → production/staging dengan ActiveMQ server
///
/// ─── Disable MassTransit ─────────────────────────────────────────────────────
/// Set "MassTransit:Enabled": false → app tetap berjalan normal tanpa messaging.
///
/// ─── Menambahkan Consumer Baru ───────────────────────────────────────────────
/// 1. Buat class di folder Consumers/ yang implement IConsumer&lt;TMessage&gt;
/// 2. MassTransit akan auto-discover via Assembly scan (tidak perlu register manual)
/// 3. Publish message dari handler via IEventBus.PublishAsync(...)
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddMessaging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var enabledStr = configuration["MassTransit:Enabled"];
        var enabled = string.IsNullOrEmpty(enabledStr) ? true : bool.Parse(enabledStr);
        if (!enabled)
            return services;

        var transport = (configuration["MassTransit:Transport"] ?? "InMemory")
            .Trim()
            .ToLowerInvariant();

        services.AddMassTransit(x =>
        {
            // Auto-discover semua consumer di assembly Infrastructure.Messaging
            x.AddConsumers(Assembly.GetExecutingAssembly());

            switch (transport)
            {
                case "rabbitmq":
                    x.UsingRabbitMq((ctx, cfg) =>
                    {
                        cfg.Host(
                            configuration["MassTransit:RabbitMQ:Host"] ?? "localhost",
                            configuration["MassTransit:RabbitMQ:VirtualHost"] ?? "/",
                            h =>
                            {
                                h.Username(configuration["MassTransit:RabbitMQ:Username"] ?? "guest");
                                h.Password(configuration["MassTransit:RabbitMQ:Password"] ?? "guest");
                            });

                        cfg.ConfigureEndpoints(ctx);
                    });
                    break;

                case "activemq":
                    x.UsingActiveMq((ctx, cfg) =>
                    {
                        cfg.Host(
                            configuration["MassTransit:ActiveMQ:Host"] ?? "localhost",
                            string.IsNullOrEmpty(configuration["MassTransit:ActiveMQ:Port"]) ? (ushort)61616 : ushort.Parse(configuration["MassTransit:ActiveMQ:Port"]!),
                            h =>
                            {
                                h.Username(configuration["MassTransit:ActiveMQ:Username"] ?? "admin");
                                h.Password(configuration["MassTransit:ActiveMQ:Password"] ?? "admin");
                            });

                        cfg.ConfigureEndpoints(ctx);
                    });
                    break;

                default: // InMemory — default untuk lokal/dev/testing
                    x.UsingInMemory((ctx, cfg) =>
                    {
                        cfg.ConfigureEndpoints(ctx);
                    });
                    break;
            }
        });

        // Register IEventBus → implementasi MassTransit
        services.AddScoped<IEventBus, MassTransitEventBus>();

        return services;
    }
}
