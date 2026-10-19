using System.Reflection;
using CleanArchitecture.Application.Common.Behaviours;
using FluentValidation;
using Mapster;
using MapsterMapper;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArchitecture.Application;

/// <summary>
///     Dependency Injection registration untuk Application layer
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register MediatR
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });

        // Register Pipeline Behaviors
        // URUTAN PENTING: MediatR menjalankan behaviour dari atas ke bawah.
        // 1. IdempotencyBehaviour — intercept paling awal, short-circuit jika duplicate request
        // 2. LoggingBehaviour — log setiap request yang lolos
        // 3. ValidationBehaviour — validasi input
        // 4. DomainEventPublishingBehaviour — publish domain events setelah handler selesai
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(IdempotencyBehaviour<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehaviour<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(DomainEventPublishingBehaviour<,>));

        // Default fallback untuk IdempotencyKeyAccessor jika tidak di-override oleh presentation layer (WebAPI)
        Microsoft.Extensions.DependencyInjection.Extensions.ServiceCollectionDescriptorExtensions.TryAddTransient<CleanArchitecture.Application.Common.Interfaces.IIdempotencyKeyAccessor, CleanArchitecture.Application.Common.Interfaces.NullIdempotencyKeyAccessor>(services);

        // Register FluentValidation validators
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // Register Mapster (Optional explicit config if needed)
        TypeAdapterConfig config = TypeAdapterConfig.GlobalSettings;
        config.Scan(Assembly.GetExecutingAssembly());
        services.AddSingleton(config);
        services.AddScoped<IMapper, ServiceMapper>();

        return services;
    }
}
