namespace CookieCommerce.CustomerDomain.Domain.DomainEvents;

public class CustomerDefaultAddressChanged
{
    public Guid CustomerId { get; set; }
    public Guid AddressId { get; set; }
    public DateTimeOffset ChangedAtUtc { get; set; }
}
