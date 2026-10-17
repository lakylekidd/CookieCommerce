namespace CookieCommerce.OrderDomain.Domain.Exceptions;

public class OrderLineNotFoundException : Exception
{
    public Guid OrderId { get; }
    public Guid ProductId { get; }

    public OrderLineNotFoundException(Guid orderId, Guid productId)
        : base($"Order line with product ID {productId} was not found in order {orderId}.")
    {
        OrderId = orderId;
        ProductId = productId;
    }
}
