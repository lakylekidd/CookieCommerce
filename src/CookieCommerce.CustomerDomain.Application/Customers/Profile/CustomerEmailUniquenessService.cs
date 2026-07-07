using CookieCommerce.CustomerDomain.Domain.Exceptions;

namespace CookieCommerce.CustomerDomain.Application.Customers.Profile;

public interface ICustomerEmailUniquenessChecker
{
    Task<CustomerEmailUniquenessResult> CheckAsync(
        string requestedEmail,
        Guid currentCustomerId,
        CancellationToken cancellationToken);
}

public sealed record CustomerEmailUniquenessResult(
    string NormalizedEmail,
    bool IsAvailable,
    CustomerEmailConflictKind ConflictKind)
{
    public static CustomerEmailUniquenessResult Available(string normalizedEmail)
    {
        return new CustomerEmailUniquenessResult(normalizedEmail, true, CustomerEmailConflictKind.None);
    }

    public static CustomerEmailUniquenessResult Conflict(string normalizedEmail, CustomerEmailConflictKind conflictKind)
    {
        return new CustomerEmailUniquenessResult(normalizedEmail, false, conflictKind);
    }
}

public enum CustomerEmailConflictKind
{
    None = 0,
    VerifiedEmail = 1,
    PendingEmail = 2
}

public sealed class CustomerEmailUniquenessService
{
    private readonly ICustomerEmailUniquenessChecker _checker;

    public CustomerEmailUniquenessService(ICustomerEmailUniquenessChecker checker)
    {
        _checker = checker;
    }

    public async Task EnsureEmailAvailableForPendingChangeAsync(
        string requestedEmail,
        Guid currentCustomerId,
        CancellationToken cancellationToken)
    {
        var result = await _checker.CheckAsync(requestedEmail, currentCustomerId, cancellationToken);

        if (!result.IsAvailable)
        {
            var message = result.ConflictKind == CustomerEmailConflictKind.PendingEmail
                ? "This email address is already pending verification for another account."
                : "This email address is already registered to another account.";

            throw new CustomerProfileValidationException(
                new Dictionary<string, string[]> { ["email"] = [message] });
        }
    }
}
