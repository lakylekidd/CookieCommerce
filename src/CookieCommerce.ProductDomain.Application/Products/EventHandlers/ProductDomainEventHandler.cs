using CookieCommerce.DomainBuildingBlocks;
using CookieCommerce.ProductDomain.Application.Products.Integration;
using CookieCommerce.ProductDomain.Domain.DomainEvents;
using Marten;

namespace CookieCommerce.ProductDomain.Application.Products.EventHandlers;

/// <summary>
/// Listens to domain events and publishes corresponding integration events.
/// This ensures integration events are published transactionally with domain events via the outbox pattern.
/// </summary>
public class ProductDomainEventHandler
{
    private readonly IIntegrationEventPublisher _publisher;
    private readonly IDocumentSession _session;

    public ProductDomainEventHandler(IIntegrationEventPublisher publisher, IDocumentSession session)
    {
        _publisher = publisher;
        _session = session;
    }

    /// <summary>
    /// Handles ProductCreated domain event and publishes ProductSnapshotPublished integration event.
    /// </summary>
    public async Task Handle(ProductCreated @event, CancellationToken ct)
    {
        var integrationEvent = new ProductSnapshotPublished(
            @event.ProductId,
            @event.Name,
            @event.Description,
            @event.Price,
            IsActive: true);

        await _publisher.PublishAsync(integrationEvent, ct);
    }

    /// <summary>
    /// Handles ProductDeactivated domain event and publishes ProductDeactivatedIntegrationEvent.
    /// </summary>
    public async Task Handle(ProductDeactivated @event, CancellationToken ct)
    {
        var integrationEvent = new ProductDeactivatedIntegrationEvent(@event.ProductId);
        await _publisher.PublishAsync(integrationEvent, ct);
    }
}
