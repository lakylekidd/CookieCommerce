namespace CookieCommerce.CustomerDomain.Domain.DomainEvents;

public class CustomerAddressRemoved
{
    public Guid CustomerId { get; set; }
    public Guid AddressId { get; set; }
    public DateTimeOffset DeletedAtUtc { get; set; }
}
