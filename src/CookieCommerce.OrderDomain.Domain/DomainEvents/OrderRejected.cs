namespace CookieCommerce.OrderDomain.Domain.DomainEvents;

public class OrderRejected
{
    public Guid OrderId { get; set; }
    public string Reason { get; set; } = default!;
    public DateTime RejectedAtUtc { get; set; }
}
