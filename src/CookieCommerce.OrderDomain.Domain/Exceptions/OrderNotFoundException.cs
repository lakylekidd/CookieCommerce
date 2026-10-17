namespace CookieCommerce.OrderDomain.Domain.Exceptions;

public class OrderNotFoundException : Exception
{
    public Guid OrderId { get; }

    public OrderNotFoundException(Guid orderId)
        : base($"Order with ID {orderId} was not found.")
    {
        OrderId = orderId;
    }
}
