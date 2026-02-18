namespace CookieCommerce.OrderDomain.Application.Products.Integration;

/// <summary>
/// Integration event received when product data is created or updated.
/// Mirrors CookieCommerce.ProductDomain.Application.Products.Integration.ProductSnapshotPublished
/// </summary>
public class ProductSnapshotPublished
{
    public Guid ProductId { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
}
