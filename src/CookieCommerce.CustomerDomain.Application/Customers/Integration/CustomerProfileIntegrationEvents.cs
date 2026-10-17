namespace CookieCommerce.CustomerDomain.Application.Customers.Integration;

public sealed record VerifiedCustomerEmailChangedIntegrationEvent(
    Guid EventId,
    DateTimeOffset OccurredAtUtc,
    Guid CustomerId,
    string PreviousEmail,
    string VerifiedEmail,
    string CorrelationId);

public sealed record CustomerProfileUpdatedIntegrationEvent(
    Guid EventId,
    DateTimeOffset OccurredAtUtc,
    Guid CustomerId,
    string FirstName,
    string LastName,
    string? Gender,
    DateOnly? DateOfBirth,
    string CorrelationId);

public sealed record CustomerForgottenIntegrationEvent(
    Guid EventId,
    DateTimeOffset OccurredAtUtc,
    Guid CustomerId,
    string? IdentitySubject,
    string Reason,
    string CorrelationId);

public interface ICustomerProfileOutboxPublisher
{
    Task PublishProfileUpdatedAsync(
        Guid customerId,
        string firstName,
        string lastName,
        string? gender,
        DateOnly? dateOfBirth,
        DateTimeOffset occurredAtUtc,
        string correlationId,
        CancellationToken cancellationToken);

    Task PublishVerifiedEmailChangedAsync(
        Guid customerId,
        string previousEmail,
        string verifiedEmail,
        DateTimeOffset occurredAtUtc,
        string correlationId,
        CancellationToken cancellationToken);
}

public interface ICustomerForgottenOutboxPublisher
{
    Task PublishCustomerForgottenAsync(
        Guid customerId,
        string? identitySubject,
        string reason,
        DateTimeOffset occurredAtUtc,
        string correlationId,
        CancellationToken cancellationToken);
}
