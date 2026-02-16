namespace CookieCommerce.OrderDomain.Domain.DomainEvents;

public class OrderCancelled
{
    public Guid OrderId { get; set; }
    public string Reason { get; set; } = default!;
    public DateTime CancelledAtUtc { get; set; }
}
