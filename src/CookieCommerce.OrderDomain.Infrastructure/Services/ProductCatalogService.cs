using CookieCommerce.OrderDomain.Application.Services;
using CookieCommerce.OrderDomain.Domain.ValueObjects;
using System.Net.Http.Json;

namespace CookieCommerce.OrderDomain.Infrastructure.Services;

/// <summary>
/// Implementation of product catalog service that fetches product information
/// from the Product domain API via HTTP.
/// </summary>
public class ProductCatalogService : IProductCatalogService
{
    private readonly HttpClient _httpClient;

    public ProductCatalogService(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task<ProductSnapshot?> GetProductAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/product/{productId}", cancellationToken);

            if (!response.IsSuccessStatusCode)
                return null;

            var product = await response.Content.ReadFromJsonAsync<ProductDto>(cancellationToken);

            if (product == null || !product.IsActive)
                return null;

            return new ProductSnapshot(product.Id, product.Name, product.Price);
        }
        catch
        {
            // Log exception in production
            return null;
        }
    }

    /// <summary>
    /// DTO for deserializing product API response
    /// </summary>
    private class ProductDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public bool IsActive { get; set; }
    }
}
