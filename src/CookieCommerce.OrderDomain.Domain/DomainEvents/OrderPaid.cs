namespace CookieCommerce.OrderDomain.Domain.DomainEvents;

public class OrderPaid
{
    public Guid OrderId { get; set; }
    public DateTime PaidAtUtc { get; set; }
}
