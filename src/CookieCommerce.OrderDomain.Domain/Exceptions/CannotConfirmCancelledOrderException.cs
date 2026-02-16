namespace CookieCommerce.OrderDomain.Domain.Exceptions;

public class CannotConfirmCancelledOrderException : Exception
{
    public Guid OrderId { get; }

    public CannotConfirmCancelledOrderException(Guid orderId) 
        : base($"Cannot confirm order {orderId} because it has been cancelled.")
    {
        OrderId = orderId;
    }
}
