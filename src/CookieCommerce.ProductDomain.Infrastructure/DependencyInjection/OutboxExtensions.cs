using CookieCommerce.DomainBuildingBlocks;
using CookieCommerce.ProductDomain.Infrastructure.Outbox;
using Microsoft.Extensions.DependencyInjection;

namespace CookieCommerce.ProductDomain.Infrastructure.DependencyInjection;

public static class OutboxExtensions
{
    /// <summary>
    /// Registers the outbox pattern services including the outbox publisher and background processor.
    /// </summary>
    public static IServiceCollection AddOutboxPattern(this IServiceCollection services)
    {
        // Register the outbox-based integration event publisher
        services.AddScoped<IIntegrationEventPublisher, OutboxIntegrationEventPublisher>();

        // Register the background service to process outbox messages
        services.AddHostedService<OutboxBackgroundPublisher>();

        return services;
    }
}
