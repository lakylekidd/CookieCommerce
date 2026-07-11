namespace CookieCommerce.ProductDomain.Application.Products.ReadModels;

/// <summary>
/// Read model for displaying products in a list view.
/// Contains essential information for browsing products.
/// </summary>
public class ProductListItemReadModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
    public ProductMediaAssetReadModel? MainImage { get; set; }
    public List<ProductVariantReadModel> Variants { get; set; } = [];
}
