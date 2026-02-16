namespace CookieCommerce.OrderDomain.Domain.DomainEvents;

public class OrderConfirmed
{
    public Guid OrderId { get; set; }
    public DateTime ConfirmedAtUtc { get; set; }
}
