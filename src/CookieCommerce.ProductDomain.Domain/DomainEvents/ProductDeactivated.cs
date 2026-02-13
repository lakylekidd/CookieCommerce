namespace CookieCommerce.ProductDomain.Domain.DomainEvents;

public class ProductDeactivated
{
    public Guid ProductId { get; }

    public ProductDeactivated(Guid productId)
    {
        ProductId = productId;
    }
}
