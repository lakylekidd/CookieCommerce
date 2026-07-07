namespace CookieCommerce.CustomerDomain.Application.Customers.Integration;

public sealed record VerifiedCustomerEmailChangedIntegrationEvent(
    Guid EventId,
    DateTimeOffset OccurredAtUtc,
    Guid CustomerId,
    string PreviousEmail,
    string VerifiedEmail,
    string CorrelationId);

public sealed record CustomerForgottenIntegrationEvent(
    Guid EventId,
    DateTimeOffset OccurredAtUtc,
    Guid CustomerId,
    string? IdentitySubject,
    string Reason,
    string CorrelationId);
