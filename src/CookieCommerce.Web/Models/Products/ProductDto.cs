namespace CookieCommerce.Web.Models.Products;

/// <summary>
/// The ProductDto represents the product information used for inventory management and internal operations.
/// It contains all the necessary details about a product, including its unique identifier, reference code, 
/// name, main image, stock quantity, active status, and associated variants. This DTO is designed for internal 
/// use within the application and may not contain all the presentation-specific details required for storefront display.
/// </summary>
public class ProductDto
{
    public Guid ProductId { get; set; }
    public string ProductReference { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public ProductImageDto? ProductMainImage { get; set; }
    public int Stock { get; set; }
    public bool Active { get; set; }
    public bool IsLimitedEdition { get; init; }
    public bool IsFeatured { get; init; } = false;
    public ICollection<ProductVariantDto> Variants { get; set; } = [];
}
