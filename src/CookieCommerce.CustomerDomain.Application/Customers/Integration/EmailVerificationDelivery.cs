namespace CookieCommerce.CustomerDomain.Application.Customers.Integration;

public interface IEmailVerificationDeliverySender
{
    Task SendAsync(EmailVerificationDeliveryPayload payload, CancellationToken cancellationToken);
}

public sealed record EmailVerificationDeliveryPayload(
    Guid CustomerId,
    string PendingEmail,
    string TokenReference,
    DateTimeOffset ExpiresAtUtc,
    Uri? VerificationUrl,
    string VerificationRoute,
    IReadOnlyDictionary<string, string> RouteValues,
    string Purpose,
    string CorrelationId,
    string IdempotencyKey);
