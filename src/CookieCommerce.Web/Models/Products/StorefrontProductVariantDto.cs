namespace CookieCommerce.Web.Models.Products;

public class StorefrontProductVariantDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public ICollection<string> ImageUrls { get; init; } = [];
}