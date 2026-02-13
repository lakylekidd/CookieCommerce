namespace CookieCommerce.ProductDomain.Domain.DomainEvents;

public class ProductRenamed
{
    public Guid ProductId { get; }
    public string NewName { get; }

    public ProductRenamed(Guid productId, string newName)
    {
        ProductId = productId;
        NewName = newName;
    }
}
