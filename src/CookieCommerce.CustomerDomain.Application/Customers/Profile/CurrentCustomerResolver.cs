using CookieCommerce.CustomerDomain.Domain.Exceptions;

namespace CookieCommerce.CustomerDomain.Application.Customers.Profile;

public sealed class CurrentCustomerResolver
{
    public Guid Resolve(Guid? authenticatedCustomerId)
    {
        if (authenticatedCustomerId is null || authenticatedCustomerId == Guid.Empty)
            throw new UnauthorizedCustomerProfileAccessException();

        return authenticatedCustomerId.Value;
    }

    public Guid ResolveOwner(Guid? authenticatedCustomerId, Guid requestedCustomerId)
    {
        var currentCustomerId = Resolve(authenticatedCustomerId);

        if (currentCustomerId != requestedCustomerId)
            throw new UnauthorizedCustomerProfileAccessException();

        return currentCustomerId;
    }
}
