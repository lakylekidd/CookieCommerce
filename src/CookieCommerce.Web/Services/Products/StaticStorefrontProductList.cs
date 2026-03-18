using CookieCommerce.Web.Models.Products;

namespace CookieCommerce.Web.Services.Products;

public static class StaticStorefrontProductList
{
    public static Dictionary<string, bool> BestSellerProductReferences = new()
    {
        { "COOKIE-001", true },
        { "COOKIE-002", false },
        { "COOKIE-003", false },
        { "COOKIE-004", true },
        { "COOKIE-005", false },
        { "COOKIE-006", false }
    };
    public static Dictionary<string, (int Reviews, float Rating)> ProductRatings = new()
    {
        { "COOKIE-001", (128, 4.8f) },
        { "COOKIE-002", (64, 4.5f) },
        { "COOKIE-003", (32, 4.0f) },
        { "COOKIE-004", (256, 4.9f) },
        { "COOKIE-005", (16, 3.5f) },
        { "COOKIE-006", (8, 4.1f) }
    };

    public static StorefrontProductDto CookieChocolateChunk => GetProduct(StaticInventoryList.InventoryChocolateChunk);
    public static StorefrontProductDto CookieMatchaWhiteChocolate => GetProduct(StaticInventoryList.InventoryMatchaWhiteChocolate);
    public static StorefrontProductDto CookieHazelnutPraline => GetProduct(StaticInventoryList.InventoryHazelnutPraline);
    public static StorefrontProductDto CookieRedVelve => GetProduct(StaticInventoryList.InventoryRedVelvet);
    public static StorefrontProductDto CookieCaramelSeaSalt => GetProduct(StaticInventoryList.InventoryCaramelSeaSalt);
    public static StorefrontProductDto CookieCocoaWhiteChocolateChip => GetProduct(StaticInventoryList.InventoryCocoaWhiteChocolateChip);
    

    public static List<StorefrontProductDto> Cookies { get; } = new()
    {
        CookieChocolateChunk,
        CookieMatchaWhiteChocolate,
        CookieHazelnutPraline,
        CookieRedVelve,
        CookieCaramelSeaSalt,
        CookieCocoaWhiteChocolateChip
    };

    private static StorefrontProductDto GetProduct(ProductDto product)
    {
        var variant = product.Variants.FirstOrDefault(v => v.Name.Contains("Single Cookie"));
        ArgumentNullException.ThrowIfNull(variant, $"Product reference for '{product.ProductName}' is null.");
        var sku = variant.Skus.FirstOrDefault(y => y.Name.Contains(product.ProductName));
        ArgumentNullException.ThrowIfNull(sku, $"SKU for product '{product.ProductName}' not found.");
        var ratingInfo = ProductRatings[product.ProductReference];

        return new StorefrontProductDto()
        {
            Id = product.ProductId,
            ProductReference = product.ProductReference,
            Title = product.ProductName,
            Description = sku.Description,
            Price = product.Variants.FirstOrDefault(x => x.Name.Contains("Single Cookie"))?.Price ?? 0.0m,
            ImageUrl = product.ProductMainImage?.Url ?? string.Empty,
            ImageUrls = product.Variants.SelectMany(v => v.Images).Select(s => s.Url).ToArray(),
            IsBestSeller = BestSellerProductReferences[product.ProductReference],
            IsFeatured = product.IsFeatured,
            IsLimitedEdition = product.IsLimitedEdition,
            Rating = ratingInfo.Rating,
            ReviewCount = ratingInfo.Reviews,
            Ingredients = sku.Ingredients
        };
    }
}
