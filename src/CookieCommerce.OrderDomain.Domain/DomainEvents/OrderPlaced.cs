namespace CookieCommerce.OrderDomain.Domain.DomainEvents;

public class OrderPlaced
{
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public DateTime PlacedAtUtc { get; set; }
}
