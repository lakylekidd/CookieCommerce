namespace CookieCommerce.Web.Models.Products;

/// <summary>
/// The storefront product is used to display product information on the storefront 
/// and product details pages. It contains all the necessary information to showcase 
/// the product, including title, description, price, images, and other relevant details. 
/// This DTO is designed for read-only purposes and is optimized for presentation in the UI.
/// </summary>
/// <remarks>
/// This actually represents the flattened product information.
/// </remarks>
public class StorefrontProductDto
{
    public Guid Id { get; init; }
    public string ProductReference { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string CurrencySymbol { get; init; } = "€";
    public decimal Price { get; init; }
    public string ImageUrl { get; init; } = string.Empty;
    public string[] ImageUrls { get; init; } = [];
    public bool IsBestSeller { get; init; }
    public bool IsLimitedEdition { get; init; }
    public bool IsFeatured { get; init; } = false;
    public ICollection<string> Ingredients { get; init; } = [];
    public float Rating { get; init; } = 0.0f;
    public int ReviewCount { get; init; } = 0;
    public ICollection<StorefrontProductVariantDto> Variants { get; init; } = [];
}
