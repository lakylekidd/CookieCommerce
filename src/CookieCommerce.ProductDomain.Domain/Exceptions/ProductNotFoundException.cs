namespace CookieCommerce.ProductDomain.Domain.Exceptions;

/// <summary>
/// Exception thrown when a product is not found.
/// </summary>
public class ProductNotFoundException : Exception
{
    public Guid ProductId { get; }

    public ProductNotFoundException(Guid productId)
        : base($"Product with ID {productId} not found.")
    {
        ProductId = productId;
    }
}
