using CookieCommerce.DomainBuildingBlocks;
using Marten;
using System.Text.Json;

namespace CookieCommerce.ProductDomain.Infrastructure.Outbox;

/// <summary>
/// Integration event publisher that stores events in an outbox table for transactional publishing.
/// This ensures that integration events are published atomically with domain changes.
/// </summary>
public class OutboxIntegrationEventPublisher(IDocumentSession session) : IIntegrationEventPublisher
{
    private readonly IDocumentSession _session = session;

    public Task PublishAsync<T>(T @event, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(@event);

        var outboxMessage = new OutboxMessage
        {
            Id = Guid.CreateVersion7(),
            EventType = typeof(T).AssemblyQualifiedName!,
            Payload = JsonSerializer.Serialize(@event),
            CreatedAt = DateTime.UtcNow,
            IsProcessed = false
        };

        _session.Store(outboxMessage);
        
        // Note: SaveChanges will be called by the handler that invokes this publisher
        // to ensure atomicity with the domain event persistence
        return Task.CompletedTask;
    }
}
