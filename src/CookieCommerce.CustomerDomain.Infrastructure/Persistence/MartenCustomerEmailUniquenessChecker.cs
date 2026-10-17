using CookieCommerce.CustomerDomain.Application.Customers.Profile;
using CookieCommerce.CustomerDomain.Application.Customers.ReadModels;
using Marten;
using Marten.Linq;

namespace CookieCommerce.CustomerDomain.Infrastructure.Persistence;

public sealed class MartenCustomerEmailUniquenessChecker : ICustomerEmailUniquenessChecker
{
    private readonly IQuerySession _querySession;

    public MartenCustomerEmailUniquenessChecker(IQuerySession querySession)
    {
        _querySession = querySession;
    }

    public async Task<CustomerEmailUniquenessResult> CheckAsync(
        string requestedEmail,
        Guid currentCustomerId,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = Normalize(requestedEmail);

        var verifiedEmailExists = await _querySession
            .Query<CustomerReadModel>()
            .AnyAsync(customer => customer.Id != currentCustomerId && customer.Email.ToLower() == normalizedEmail, cancellationToken);

        if (verifiedEmailExists)
        {
            return CustomerEmailUniquenessResult.Conflict(normalizedEmail, CustomerEmailConflictKind.VerifiedEmail);
        }

        var pendingEmailExists = await _querySession
            .Query<CustomerReadModel>()
            .AnyAsync(customer => customer.Id != currentCustomerId && customer.PendingEmail != null && customer.PendingEmail.ToLower() == normalizedEmail, cancellationToken);

        return pendingEmailExists
            ? CustomerEmailUniquenessResult.Conflict(normalizedEmail, CustomerEmailConflictKind.PendingEmail)
            : CustomerEmailUniquenessResult.Available(normalizedEmail);
    }

    private static string Normalize(string email)
    {
        return email.Trim().ToLowerInvariant();
    }
}
