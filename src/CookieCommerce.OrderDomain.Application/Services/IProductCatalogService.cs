using CookieCommerce.OrderDomain.Domain.ValueObjects;

namespace CookieCommerce.OrderDomain.Application.Services;

/// <summary>
/// Service for fetching product information from the Catalog/Product domain.
/// Used to snapshot product details when adding items to orders.
/// </summary>
public interface IProductCatalogService
{
    /// <summary>
    /// Gets product information by ID
    /// </summary>
    /// <param name="productId">The product ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>ProductSnapshot containing name and price, or null if product not found or inactive</returns>
    Task<ProductSnapshot?> GetProductAsync(Guid productId, CancellationToken cancellationToken = default);
}
