using CookieCommerce.Web.Models.Products;
using CookieCommerce.Web.Models.Reviews;
using CookieCommerce.Web.Services.Commerce;

namespace CookieCommerce.Web.Services.Products;

public sealed class CatalogService(ProductApiClient api, ICommerceStore store) : ICookieService
{
    public async Task<IReadOnlyList<StorefrontProductDto>> GetFeaturedCookiesAsync(CancellationToken cancellationToken = default)
    {
        var products = await GetFavoriteCookiesAsync(cancellationToken);
        var settings = await store.LoadAsync<CommerceSettings>("store") ?? new();
        var selected = settings.Preference("flavorOfTheMonth", "");
        var featured = products.Where(p => p.IsFeatured).ToList();
        return (featured.Count > 0 ? featured : products).OrderByDescending(p => p.Id.ToString() == selected)
            .Take(Math.Clamp(settings.Preference("featuredProductCount", 4), 1, 20)).ToList();
    }

    // This interface method is used by the shop's complete product grid.
    public async Task<IReadOnlyList<StorefrontProductDto>> GetFavoriteCookiesAsync(CancellationToken cancellationToken = default)
    {
        var products = await api.ListAsync(cancellationToken);
        var result = new List<StorefrontProductDto>();
        foreach (var product in products.Where(p => p.IsActive && p.Variants.Any(v => v.IsActive)))
            result.Add(await MapAsync(product));
        return result;
    }

    public async Task<StorefrontProductDto?> GetProductByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await api.GetAsync(id, cancellationToken);
        return product is { IsActive: true } && product.Variants.Any(v => v.IsActive) ? await MapAsync(product) : null;
    }

    public async Task<IReadOnlyList<ReviewDto>> GetReviewsByCookieIdAsync(Guid cookieId, CancellationToken cancellationToken = default) =>
        (await store.ListAsync<CustomerReview>()).Where(r => cookieId == Guid.Empty || r.Review.CookieId == cookieId)
            .OrderByDescending(r => r.CreatedAt).Select(r => r.Review).ToList();

    public async Task<IReadOnlyList<StorefrontProductDto>> GetTopRatedAsync() =>
        (await GetFavoriteCookiesAsync()).OrderByDescending(p => p.Rating).ThenByDescending(p => p.ReviewCount).Take(4).ToList();

    private async Task<StorefrontProductDto> MapAsync(CatalogProduct product)
    {
        var metadata = await store.LoadAsync<CatalogMetadata>(product.Id.ToString()) ?? new();
        var settings = await store.LoadAsync<CommerceSettings>("store") ?? new();
        var bestSellers = (await store.ListAsync<CommerceOrder>()).Where(o => o.PaymentStatus == "Paid").SelectMany(o => o.Order.Products)
            .GroupBy(p => p.ProductId).OrderByDescending(g => g.Sum(p => p.Quantity)).Take(4).Select(g => g.Key).ToHashSet();
        var reviews = await GetReviewsByCookieIdAsync(product.Id);
        var variants = product.Variants.Where(v => v.IsActive).ToList();
        var images = variants.SelectMany(v => v.Photos).Select(p => p.Url).ToList();
        if (product.MainImage is not null) images.Insert(0, product.MainImage.Url);
        if (images.Count == 0) images.Add("/images/logo.png");
        return new StorefrontProductDto
        {
            Id = product.Id, ProductReference = metadata.Reference, Title = product.Name,
            Description = product.Description ?? "", Price = variants.Min(v => v.Price), CurrencySymbol = "€",
            ImageUrl = product.MainImage?.Url ?? ProductMainImages.Fallback, ImageUrls = images.Distinct().ToArray(),
            IsFeatured = metadata.Featured || product.Id.ToString() == settings.Preference("flavorOfTheMonth", ""),
            IsLimitedEdition = metadata.LimitedEdition && settings.Preference("showLimitedBadge", true),
            IsBestSeller = settings.Preference("showTopSellerBadge", true) && bestSellers.Contains(product.Id),
            Rating = reviews.Count == 0 ? 0 : (float)reviews.Average(r => r.Rating), ReviewCount = reviews.Count,
            Ingredients = metadata.VariantSkus.Values.SelectMany(v => v).SelectMany(v => v.Ingredients).Distinct().ToList(),
            Variants = variants.Select(v => new StorefrontProductVariantDto
            {
                Id = v.Id, Title = v.Name, Price = v.Price,
                ImageUrls = v.Photos.Count > 0 ? v.Photos.Select(p => p.Url).ToArray() : images.Take(1).ToArray()
            }).ToList()
        };
    }
}
