namespace CookieCommerce.CustomerDomain.Domain.DomainEvents;

public class CustomerAddressRemoved
{
    public CustomerAddressRemoved()
    {
    }

    public CustomerAddressRemoved(Guid customerId, string label)
    {
        CustomerId = customerId;
        AddressId = Guid.Empty;
        DeletedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid CustomerId { get; set; }
    public Guid AddressId { get; set; }
    public DateTimeOffset DeletedAtUtc { get; set; }
}
