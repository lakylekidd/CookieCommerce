namespace CookieCommerce.OrderDomain.Domain.Exceptions;

public class CannotModifyNonPendingOrderException : Exception
{
    public Guid OrderId { get; }
    public string Status { get; }

    public CannotModifyNonPendingOrderException(Guid orderId, string status)
        : base($"Cannot modify order {orderId} because it is in {status} status. Only pending orders can be modified.")
    {
        OrderId = orderId;
        Status = status;
    }
}
