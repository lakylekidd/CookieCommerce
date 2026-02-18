using CookieCommerce.OrderDomain.Application.Products.Integration;
using CookieCommerce.OrderDomain.Application.Products.ReadModels;
using Marten;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CookieCommerce.OrderDomain.Infrastructure.Consumers;

/// <summary>
/// Consumes ProductSnapshotPublished integration events and maintains a local read model.
/// This enables eventual consistency without synchronous HTTP calls to Product domain.
/// </summary>
public class ProductSnapshotPublishedConsumer(
    IDocumentSession session,
    ILogger<ProductSnapshotPublishedConsumer> logger) : IConsumer<ProductSnapshotPublished>
{
    private readonly IDocumentSession _session = session;
    private readonly ILogger<ProductSnapshotPublishedConsumer> _logger = logger;

    public async Task Consume(ConsumeContext<ProductSnapshotPublished> context)
    {
        var @event = context.Message;
        
        _logger.LogInformation(
            "Received ProductSnapshotPublished for product {ProductId}", 
            @event.ProductId);

        // Upsert product snapshot in local read model
        var snapshot = await _session.LoadAsync<ProductSnapshotReadModel>(@event.ProductId) 
            ?? new ProductSnapshotReadModel { Id = @event.ProductId };

        snapshot.Name = @event.Name;
        snapshot.Description = @event.Description;
        snapshot.Price = @event.Price;
        snapshot.IsActive = @event.IsActive;
        snapshot.LastUpdated = DateTime.UtcNow;

        _session.Store(snapshot);
        await _session.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation(
            "Updated product snapshot for {ProductId}: {Name} (Price: {Price}, Active: {IsActive})",
            @event.ProductId, @event.Name, @event.Price, @event.IsActive);
    }
}
