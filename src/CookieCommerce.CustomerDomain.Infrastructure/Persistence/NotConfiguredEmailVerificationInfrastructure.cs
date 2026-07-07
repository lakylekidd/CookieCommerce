using CookieCommerce.CustomerDomain.Application.Customers.Integration;

namespace CookieCommerce.CustomerDomain.Infrastructure.Persistence;

public sealed class NotConfiguredEmailVerificationTokenStore : IEmailVerificationTokenStore
{
    public Task<EmailVerificationTokenReference> CreateAsync(Guid customerId, string pendingEmail, DateTimeOffset expiresAtUtc, CancellationToken cancellationToken)
    {
        throw new NotImplementedException("Email verification token storage is implemented in the US1 infrastructure task.");
    }

    public Task<EmailVerificationTokenReference?> FindAsync(Guid customerId, string token, CancellationToken cancellationToken)
    {
        throw new NotImplementedException("Email verification token storage is implemented in the US1 infrastructure task.");
    }

    public Task MarkConsumedAsync(Guid customerId, string tokenReference, DateTimeOffset consumedAtUtc, CancellationToken cancellationToken)
    {
        throw new NotImplementedException("Email verification token storage is implemented in the US1 infrastructure task.");
    }
}

public sealed class NotConfiguredEmailVerificationDeliverySender : IEmailVerificationDeliverySender
{
    public Task SendAsync(EmailVerificationDeliveryPayload payload, CancellationToken cancellationToken)
    {
        throw new NotImplementedException("Email verification delivery is implemented in the US1 infrastructure task.");
    }
}
