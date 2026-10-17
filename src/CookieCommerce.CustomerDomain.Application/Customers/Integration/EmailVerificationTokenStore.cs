namespace CookieCommerce.CustomerDomain.Application.Customers.Integration;

public interface IEmailVerificationTokenStore
{
    Task<EmailVerificationTokenReference> CreateAsync(Guid customerId, string pendingEmail, DateTimeOffset expiresAtUtc, CancellationToken cancellationToken);

    Task<EmailVerificationTokenReference?> FindAsync(Guid customerId, string token, CancellationToken cancellationToken);

    Task MarkConsumedAsync(Guid customerId, string tokenReference, DateTimeOffset consumedAtUtc, CancellationToken cancellationToken);
}

public sealed record EmailVerificationTokenReference(
    Guid CustomerId,
    string TokenReference,
    string HashedToken,
    DateTimeOffset ExpiresAtUtc,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? ConsumedAtUtc);
