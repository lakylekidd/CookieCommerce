namespace CookieCommerce.CustomerDomain.Application;

public interface IIntegrationEventPublisher
{
    Task PublishAsync<T>(T @event, CancellationToken ct);
}
