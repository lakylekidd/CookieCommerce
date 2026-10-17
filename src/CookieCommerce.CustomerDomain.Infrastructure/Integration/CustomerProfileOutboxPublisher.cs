using CookieCommerce.CustomerDomain.Application.Customers.Integration;
using CookieCommerce.DomainBuildingBlocks;

namespace CookieCommerce.CustomerDomain.Infrastructure.Integration;

public sealed class CustomerProfileOutboxPublisher(IIntegrationEventPublisher publisher) : ICustomerProfileOutboxPublisher
{
    public Task PublishProfileUpdatedAsync(
        Guid customerId,
        string firstName,
        string lastName,
        string? gender,
        DateOnly? dateOfBirth,
        DateTimeOffset occurredAtUtc,
        string correlationId,
        CancellationToken cancellationToken)
    {
        var integrationEvent = new CustomerProfileUpdatedIntegrationEvent(
            EventId: Guid.CreateVersion7(),
            OccurredAtUtc: occurredAtUtc,
            CustomerId: customerId,
            FirstName: firstName,
            LastName: lastName,
            Gender: gender,
            DateOfBirth: dateOfBirth,
            CorrelationId: correlationId);

        return publisher.PublishAsync(integrationEvent, cancellationToken);
    }

    public Task PublishVerifiedEmailChangedAsync(
        Guid customerId,
        string previousEmail,
        string verifiedEmail,
        DateTimeOffset occurredAtUtc,
        string correlationId,
        CancellationToken cancellationToken)
    {
        var integrationEvent = new VerifiedCustomerEmailChangedIntegrationEvent(
            EventId: Guid.CreateVersion7(),
            OccurredAtUtc: occurredAtUtc,
            CustomerId: customerId,
            PreviousEmail: previousEmail,
            VerifiedEmail: verifiedEmail,
            CorrelationId: correlationId);

        return publisher.PublishAsync(integrationEvent, cancellationToken);
    }
}
