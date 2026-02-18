using CookieCommerce.OrderDomain.Domain.ValueObjects;

namespace CookieCommerce.OrderDomain.Domain.DomainEvents;

public class OrderPlaced
{
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public CustomerSnapshot? CustomerSnapshot { get; set; }
    public DateTime PlacedAtUtc { get; set; }
}
