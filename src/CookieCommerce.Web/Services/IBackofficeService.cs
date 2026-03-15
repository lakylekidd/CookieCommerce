using CookieCommerce.Web.Models.Backoffice;
using CookieCommerce.Web.Models.Order;
using CookieCommerce.Web.Models.Products;

namespace CookieCommerce.Web.Services;

public interface IBackofficeService
{
    Task<DashboardDto> GetTotalsAsync(string period = "this week");
    Task<QuotaDateOrdersDto> GetQuotaDateOrdersAsync(DateTime quotaDate);
    Task<ICollection<InventoryItemWithVariantsDto>> GetInventoryItemsAsync();
    Task ChangeProductStatusAsync(Guid productId, bool isActive);
    Task ChangeVariantStatusAsync(Guid variantId, bool isActive);
    Task ChangeVariantNameAsync(Guid variantId, string name);
    Task ChangeVariantPriceAsync(Guid variantId, decimal price);
    Task DeleteVariantAsync(Guid variantId);
    Task<InventoryItemWithVariantsDto> GetInventoryItemAsync(Guid productId);
}
