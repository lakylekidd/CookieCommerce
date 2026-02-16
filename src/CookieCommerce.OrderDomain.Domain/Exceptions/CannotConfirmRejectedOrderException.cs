namespace CookieCommerce.OrderDomain.Domain.Exceptions;

public class CannotConfirmRejectedOrderException : Exception
{
    public Guid OrderId { get; }

    public CannotConfirmRejectedOrderException(Guid orderId) 
        : base($"Cannot confirm order {orderId} because it has been rejected.")
    {
        OrderId = orderId;
    }
}
