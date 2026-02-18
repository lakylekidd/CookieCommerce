namespace CookieCommerce.OrderDomain.Domain.ValueObjects;

/// <summary>
/// Represents a snapshot of product information at the time of order placement.
/// Historical orders remain stable even if catalog prices change.
/// Note: In the future, this will be extended to support product variants and SKUs.
/// The order line should reference the product variant (not just the product).
/// </summary>
public class ProductSnapshot
{
    /// <summary>
    /// Product's unique identifier
    /// </summary>
    public Guid ProductId { get; init; }

    /// <summary>
    /// Product name at the time of order
    /// </summary>
    public string ProductName { get; init; } = string.Empty;

    /// <summary>
    /// Product price at the time of order
    /// </summary>
    public decimal Price { get; init; }

    public ProductSnapshot() { }

    public ProductSnapshot(Guid productId, string productName, decimal price)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("Product ID is required.", nameof(productId));

        if (string.IsNullOrWhiteSpace(productName))
            throw new ArgumentException("Product name is required.", nameof(productName));

        if (price <= 0)
            throw new ArgumentException("Price must be greater than 0.", nameof(price));

        ProductId = productId;
        ProductName = productName;
        Price = price;
    }
}
