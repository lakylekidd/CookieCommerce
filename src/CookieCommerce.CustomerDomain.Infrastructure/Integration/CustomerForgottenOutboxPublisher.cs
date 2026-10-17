using CookieCommerce.CustomerDomain.Application.Customers.Integration;
using CookieCommerce.DomainBuildingBlocks;

namespace CookieCommerce.CustomerDomain.Infrastructure.Integration;

public sealed class CustomerForgottenOutboxPublisher(IIntegrationEventPublisher publisher) : ICustomerForgottenOutboxPublisher
{
    public Task PublishCustomerForgottenAsync(
        Guid customerId,
        string? identitySubject,
        string reason,
        DateTimeOffset occurredAtUtc,
        string correlationId,
        CancellationToken cancellationToken)
    {
        var integrationEvent = new CustomerForgottenIntegrationEvent(
            EventId: Guid.CreateVersion7(),
            OccurredAtUtc: occurredAtUtc,
            CustomerId: customerId,
            IdentitySubject: identitySubject,
            Reason: reason,
            CorrelationId: correlationId);

        return publisher.PublishAsync(integrationEvent, cancellationToken);
    }
}
