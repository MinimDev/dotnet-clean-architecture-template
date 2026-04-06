using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using CleanArchitecture.Application.Common.Behaviours;
using CleanArchitecture.Application.Features.Products.Queries.GetProductsList;
using CleanArchitecture.Domain.Entities;

namespace CleanArchitecture.Application;

/// <summary>
/// Dependency Injection registration untuk Application layer
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register MediatR
        services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });

        // Register Pipeline Behaviors
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehaviour<,>));

        // Register FluentValidation validators
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // Register Mapster (Optional explicit config if needed)
        var config = Mapster.TypeAdapterConfig.GlobalSettings;
        config.Scan(Assembly.GetExecutingAssembly());
        services.AddSingleton(config);
        services.AddScoped<MapsterMapper.IMapper, MapsterMapper.ServiceMapper>();

        return services;
    }
}
