using CookieCommerce.OrderDomain.Domain.ValueObjects;

namespace CookieCommerce.OrderDomain.Domain.DomainEvents;

public class OrderLineAdded
{
    public Guid OrderId { get; set; }
    public OrderLine OrderLine { get; set; } = default!;
}
