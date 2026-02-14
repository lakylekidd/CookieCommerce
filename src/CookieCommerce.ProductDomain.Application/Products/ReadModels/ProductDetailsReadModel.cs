namespace CookieCommerce.ProductDomain.Application.Products.ReadModels;

/// <summary>
/// Read model for displaying full product details.
/// Contains complete product information including description.
/// </summary>
public class ProductDetailsReadModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
}
