using CookieCommerce.OrderDomain.Application.Customers.Integration;
using CookieCommerce.OrderDomain.Application.Customers.ReadModels;
using Marten;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CookieCommerce.OrderDomain.Infrastructure.Consumers;

/// <summary>
/// Consumes CustomerDeletedIntegrationEvent and removes customer from local read model.
/// </summary>
public class CustomerDeletedConsumer(
    IDocumentSession session,
    ILogger<CustomerDeletedConsumer> logger) : IConsumer<CustomerDeletedIntegrationEvent>
{
    private readonly IDocumentSession _session = session;
    private readonly ILogger<CustomerDeletedConsumer> _logger = logger;

    public async Task Consume(ConsumeContext<CustomerDeletedIntegrationEvent> context)
    {
        var @event = context.Message;

        _logger.LogInformation(
            "Received CustomerDeletedIntegrationEvent for customer {CustomerId}",
            @event.CustomerId);

        _session.Delete<CustomerSnapshotReadModel>(@event.CustomerId);
        await _session.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation(
            "Removed customer snapshot for {CustomerId}",
            @event.CustomerId);
    }
}
