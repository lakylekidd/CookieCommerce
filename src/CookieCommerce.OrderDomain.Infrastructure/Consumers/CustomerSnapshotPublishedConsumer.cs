using CookieCommerce.OrderDomain.Application.Customers.Integration;
using CookieCommerce.OrderDomain.Application.Customers.ReadModels;
using Marten;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CookieCommerce.OrderDomain.Infrastructure.Consumers;

/// <summary>
/// Consumes CustomerSnapshotPublished integration events and maintains a local read model.
/// This enables eventual consistency without synchronous HTTP calls to Customer domain.
/// </summary>
public class CustomerSnapshotPublishedConsumer(
    IDocumentSession session,
    ILogger<CustomerSnapshotPublishedConsumer> logger) : IConsumer<CustomerSnapshotPublished>
{
    private readonly IDocumentSession _session = session;
    private readonly ILogger<CustomerSnapshotPublishedConsumer> _logger = logger;

    public async Task Consume(ConsumeContext<CustomerSnapshotPublished> context)
    {
        var @event = context.Message;
        
        _logger.LogInformation(
            "Received CustomerSnapshotPublished for customer {CustomerId}", 
            @event.CustomerId);

        // Upsert customer snapshot in local read model
        var snapshot = await _session.LoadAsync<CustomerSnapshotReadModel>(@event.CustomerId) 
            ?? new CustomerSnapshotReadModel { Id = @event.CustomerId };

        snapshot.FirstName = @event.FirstName;
        snapshot.LastName = @event.LastName;
        snapshot.Email = @event.Email;
        snapshot.DateOfBirth = @event.DateOfBirth;
        snapshot.LastUpdated = DateTime.UtcNow;

        _session.Store(snapshot);
        await _session.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation(
            "Updated customer snapshot for {CustomerId}: {FirstName} {LastName}",
            @event.CustomerId, @event.FirstName, @event.LastName);
    }
}
