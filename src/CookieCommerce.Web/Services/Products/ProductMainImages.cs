using CookieCommerce.Web.Models.Backoffice;
using CookieCommerce.Web.Models.Cart;
using CookieCommerce.Web.Models.Order;

namespace CookieCommerce.Web.Services.Products;

/// <summary>Refreshes product thumbnails from main images, including inactive products in old orders.</summary>
public sealed class ProductMainImages(ProductApiClient api)
{
    public const string Fallback = "/images/logo.png";

    public async Task ApplyAsync(IEnumerable<OrderProductDto> items)
    {
        var lines = items.ToList();
        if (lines.Count == 0) return;
        var images = await LoadAsync();
        foreach (var line in lines) line.ProductImageUrl = images.GetValueOrDefault(line.ProductId, Fallback);
    }

    public async Task ApplyAsync(IEnumerable<CartItemDto> items)
    {
        var lines = items.ToList();
        if (lines.Count == 0) return;
        var images = await LoadAsync();
        foreach (var line in lines) line.ProductImageUrl = images.GetValueOrDefault(line.ProductId, Fallback);
    }

    public async Task ApplyAsync(IEnumerable<BatchProductDto> items)
    {
        var products = items.ToList();
        if (products.Count == 0) return;
        var images = await LoadAsync();
        foreach (var product in products) product.ImageUrl = images.GetValueOrDefault(product.Id, Fallback);
    }

    private async Task<Dictionary<Guid, string>> LoadAsync() => (await api.ListAsync()).ToDictionary(p => p.Id,
        p => string.IsNullOrWhiteSpace(p.MainImage?.Url) ? Fallback : p.MainImage.Url);
}
