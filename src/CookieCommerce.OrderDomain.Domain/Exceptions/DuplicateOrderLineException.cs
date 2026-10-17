namespace CookieCommerce.OrderDomain.Domain.Exceptions;

public class DuplicateOrderLineException : Exception
{
    public Guid OrderId { get; }
    public Guid ProductId { get; }

    public DuplicateOrderLineException(Guid orderId, Guid productId)
        : base($"Order {orderId} already contains a line for product {productId}.")
    {
        OrderId = orderId;
        ProductId = productId;
    }
}
