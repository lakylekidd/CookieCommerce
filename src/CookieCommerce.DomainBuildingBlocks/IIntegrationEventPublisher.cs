namespace CookieCommerce.DomainBuildingBlocks;

/// <summary>
/// Publishes integration events to the message bus for cross-service communication.
/// </summary>
public interface IIntegrationEventPublisher
{
    /// <summary>
    /// Publishes an integration event to the message bus.
    /// </summary>
    /// <typeparam name="T">Type of the integration event.</typeparam>
    /// <param name="event">The integration event to publish.</param>
    /// <param name="ct">Cancellation token.</param>
    Task PublishAsync<T>(T @event, CancellationToken ct);
}
