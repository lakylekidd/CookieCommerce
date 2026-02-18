using CookieCommerce.OrderDomain.Domain.ValueObjects;

namespace CookieCommerce.OrderDomain.Domain.DomainEvents;

public class OrderPlaced
{
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public CustomerSnapshot? CustomerSnapshot { get; set; }
    public OrderAddress? ShippingAddress { get; set; }
    public OrderAddress? BillingAddress { get; set; }
    public DateTime PlacedAtUtc { get; set; }
}
