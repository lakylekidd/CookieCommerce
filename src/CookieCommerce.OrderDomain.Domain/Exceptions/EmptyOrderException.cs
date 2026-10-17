namespace CookieCommerce.OrderDomain.Domain.Exceptions;

public class EmptyOrderException : Exception
{
    public Guid OrderId { get; }

    public EmptyOrderException(Guid orderId)
        : base($"Cannot confirm order {orderId} because it has no order lines.")
    {
        OrderId = orderId;
    }
}
