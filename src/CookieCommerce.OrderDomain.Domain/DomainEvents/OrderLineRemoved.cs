namespace CookieCommerce.OrderDomain.Domain.DomainEvents;

public class OrderLineRemoved
{
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
}
