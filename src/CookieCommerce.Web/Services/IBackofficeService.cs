using CookieCommerce.Web.Models.Backoffice;
using CookieCommerce.Web.Models.Order;
using CookieCommerce.Web.Models.Products;

namespace CookieCommerce.Web.Services;

public interface IBackofficeService
{
    Task SetOrderAsProcessing(Guid orderId);
    Task SetOrderAsReadyForPickup(Guid orderId);
    Task CreateProductAsync(CreateProductDto createProductDto);
    Task<DashboardDto> GetTotalsAsync(string period = "this week");
    Task<ICollection<SkuDto>> GetSkusAsync();
    Task<ProductVariantDto> GetVariantAsync(Guid variantId);
    Task<ICollection<ProductVariantSkuDto>> GetVariantSkus(Guid variantId);
    Task<QuotaDateOrdersDto> GetQuotaDateOrdersAsync(DateTime quotaDate);
    Task<ICollection<ProductDto>> GetInventoryItemsAsync();
    Task ChangeProductStatusAsync(Guid productId, bool isActive);
    Task ChangeVariantStatusAsync(Guid variantId, bool isActive);
    Task ChangeVariantNameAsync(Guid variantId, string name);
    Task ChangeVariantPriceAsync(Guid variantId, decimal price);
    Task DeleteVariantAsync(Guid variantId);
    Task<ProductDto> GetInventoryItemAsync(Guid productId);
    Task AddVariantSkuAsync(Guid productId, Guid skuId, int quantity);
    Task RemoveVariantSku(Guid variantId, Guid skuId);
    Task UpdateVariantSkuQuantity(Guid variantId, Guid skuId, int quantity);
}
