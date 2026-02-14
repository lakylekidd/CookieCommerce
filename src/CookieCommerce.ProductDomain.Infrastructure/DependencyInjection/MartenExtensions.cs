using CookieCommerce.ProductDomain.Infrastructure.Outbox;
using CookieCommerce.ProductDomain.Infrastructure.Projections;
using Marten;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CookieCommerce.ProductDomain.Infrastructure.DependencyInjection;

public static class MartenExtensions
{
    public static IServiceCollection AddMartenConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMarten(opts =>
        {
            var connectionString = configuration.GetConnectionString("ProductDomain");

            if (string.IsNullOrEmpty(connectionString))
                throw new InvalidOperationException("Connection string 'ProductDomain' not found.");

            opts.Connection(connectionString);

            // Register projections for product read models
            opts.Projections.Add<ProductListItemProjection>(JasperFx.Events.Projections.ProjectionLifecycle.Inline);
            opts.Projections.Add<ProductDetailsProjection>(JasperFx.Events.Projections.ProjectionLifecycle.Inline);

            // Register OutboxMessage as a document
            opts.Schema.For<OutboxMessage>()
                .Index(x => x.IsProcessed)
                .Index(x => x.CreatedAt);

            // Configure event metadata
            // Marten automatically stores event metadata including:
            // - Timestamp (captured at event persistence)
            // - Stream ID (aggregate ID)
            // - Version (event sequence number)
            // Additional metadata like causation ID and correlation ID can be set via IEvent.Headers
            opts.Events.MetadataConfig.CausationIdEnabled = true;
            opts.Events.MetadataConfig.CorrelationIdEnabled = true;
            opts.Events.MetadataConfig.HeadersEnabled = true;

            // Auto-create schema objects (tables, functions, etc.)
            opts.AutoCreateSchemaObjects = JasperFx.AutoCreate.All;
        })
        .ApplyAllDatabaseChangesOnStartup();

        return services;
    }
}
