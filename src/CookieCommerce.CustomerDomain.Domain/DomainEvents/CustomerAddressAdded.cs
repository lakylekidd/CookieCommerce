using CookieCommerce.CustomerDomain.Domain.ValueObjects;

namespace CookieCommerce.CustomerDomain.Domain.DomainEvents;

public class CustomerAddressAdded
{
    public Guid CustomerId { get; }
    public Address Address { get; }

    public CustomerAddressAdded(Guid customerId, Address address)
    {
        CustomerId = customerId;
        Address = address;
    }
}
