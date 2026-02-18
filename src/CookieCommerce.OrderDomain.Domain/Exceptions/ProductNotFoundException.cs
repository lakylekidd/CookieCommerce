namespace CookieCommerce.OrderDomain.Domain.Exceptions;

/// <summary>
/// Exception thrown when a product is not found or is not available in the catalog.
/// </summary>
public class ProductNotFoundException : Exception
{
    public Guid ProductId { get; }

    public ProductNotFoundException(Guid productId)
        : base($"Product with ID '{productId}' was not found or is not available.")
    {
        ProductId = productId;
    }
}
