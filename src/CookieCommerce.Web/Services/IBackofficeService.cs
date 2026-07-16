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
    Task UpdateProductNameAsync(Guid productId, string name, CancellationToken cancellationToken = default);
    Task ChangeVariantStatusAsync(Guid variantId, bool isActive);
    Task ChangeVariantNameAsync(Guid variantId, string name);
    Task ChangeVariantPriceAsync(Guid variantId, decimal price);
    Task DeleteVariantAsync(Guid variantId);
    Task<ProductDto> GetInventoryItemAsync(Guid productId);
    Task AddVariantSkuAsync(Guid productId, Guid skuId, int quantity);
    Task RemoveVariantSku(Guid variantId, Guid skuId);
    Task UpdateVariantSkuQuantity(Guid variantId, Guid skuId, int quantity);
    Task UpsertProductVariantsAsync(Guid productId, IReadOnlyCollection<UpsertProductVariantDto> variants, CancellationToken cancellationToken = default);
    Task<ProductImageDto> UploadProductMainImageAsync(Guid productId, ProductMediaUploadDto upload, CancellationToken cancellationToken = default);
    Task RemoveProductMainImageAsync(Guid productId, Guid mediaAssetId, CancellationToken cancellationToken = default);
    Task<ProductImageDto> UploadVariantPhotoAsync(Guid productId, Guid variantId, ProductMediaUploadDto upload, CancellationToken cancellationToken = default);
    Task RemoveVariantPhotoAsync(Guid productId, Guid variantId, Guid mediaAssetId, CancellationToken cancellationToken = default);
}
