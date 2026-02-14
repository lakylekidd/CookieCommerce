using CookieCommerce.DomainBuildingBlocks;
using MassTransit;

namespace CookieCommerce.CookieMassTransit;

internal class MassTransitIntegrationEventPublisher(IPublishEndpoint endpoint) : IIntegrationEventPublisher
{
    private readonly IPublishEndpoint _endpoint = endpoint;

    public async Task PublishAsync<T>(T @event, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(@event);

        await _endpoint.Publish(message: @event, cancellationToken: ct);
    }
}
