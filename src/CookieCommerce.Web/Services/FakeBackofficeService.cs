using CookieCommerce.Web.Models.Backoffice;
using CookieCommerce.Web.Models.Order;
using CookieCommerce.Web.Models.Products;
using CookieCommerce.Web.Services.Products;
using CookieCommerce.Web.Services.StaticData;

namespace CookieCommerce.Web.Services;

public class FakeBackofficeService : IBackofficeService
{
    public Task ChangeProductStatusAsync(Guid productId, bool isActive)
    {
        StaticInventoryList.InventoryItems
            .Where(i => i.ProductId == productId)
            .ToList()
            .ForEach(i => i.Active = isActive);
        return Task.Delay(500);
    }

    public Task ChangeVariantNameAsync(Guid variantId, string name)
    {
        StaticInventoryList.InventoryItems
            .SelectMany(i => i.Variants)
            .Where(v => v.Id == variantId)
            .ToList()
            .ForEach(v => v.Name = name);
        return Task.Delay(500);
    }

    public Task ChangeVariantStatusAsync(Guid variantId, bool isActive)
    {
        StaticInventoryList.InventoryItems
            .SelectMany(i => i.Variants)
            .Where(v => v.Id == variantId)
            .ToList()
            .ForEach(v => v.Active = isActive);
        return Task.Delay(500);
    }

    public async Task<InventoryItemWithVariantsDto> GetInventoryItemAsync(Guid productId)
    {
        var item = StaticInventoryList.InventoryItems.FirstOrDefault(i => i.ProductId == productId);
        await Task.Delay(800);
        return item;
    }

    public async Task<ICollection<InventoryItemWithVariantsDto>> GetInventoryItemsAsync()
    {
        var inventoryItems = StaticInventoryList.InventoryItems;
        await Task.Delay(800);
        return inventoryItems;
    }

    public async Task<QuotaDateOrdersDto> GetQuotaDateOrdersAsync(DateTime quotaDate)
    {
        var orders = MockOrderData.GetRecentOrders();
        await Task.Delay(800);
        return new QuotaDateOrdersDto
        {
            DeliveryOn = quotaDate,
            Items = orders
                .SelectMany(o => o.Items)
                .GroupBy(i => i.ProductId)
                .Select(g => new QuotaOrderItemDto
                {
                    ProductId = g.Key,
                    ProductImageUrl = g.First().ProductImageUrl,
                    ProductName = g.First().ProductName,
                    Quantity = g.Sum(i => i.Quantity)
                })
                .ToList(),
            Orders = orders.ToList()
        };
    }

    public async Task<DashboardDto> GetTotalsAsync(string period)
    {
        await Task.Delay(800);
        var dashboard = MockDashboardData.GetDashboardByPeriod(period);
        return dashboard;
    }

}
