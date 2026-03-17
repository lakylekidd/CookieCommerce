using CookieCommerce.Web.Models.Backoffice;
using CookieCommerce.Web.Models.Order;
using CookieCommerce.Web.Models.Products;
using CookieCommerce.Web.Services.Products;
using CookieCommerce.Web.Services.StaticData;

namespace CookieCommerce.Web.Services;

public class FakeBackofficeService : IBackofficeService
{
    public async Task<ICollection<SkuDto>> GetSkusAsync()
    {
        var skus = StaticSkuList.Skus;
        await Task.Delay(800);
        return skus;
    }

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

    public Task ChangeVariantPriceAsync(Guid variantId, decimal price)
    {
        StaticInventoryList.InventoryItems
            .SelectMany(i => i.Variants)
            .Where(v => v.Id == variantId)
            .ToList()
            .ForEach(v => v.Price = price);
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

    public Task DeleteVariantAsync(Guid variantId)
    {
        StaticInventoryList.InventoryItems
            .ToList()
            .ForEach(i => i.Variants.ToList().RemoveAll(v => v.Id == variantId));
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

    public async Task<ICollection<ProductVariantSkuDto>> GetVariantSkus(Guid variantId)
    {
        var skus = StaticInventoryList.InventoryItems
            .SelectMany(i => i.Variants)
            .Where(v => v.Id == variantId)
            .SelectMany(v => v.Skus)
            .ToList();
        await Task.Delay(800);
        return skus;
    }

    public Task AddVariantSkuAsync(Guid productId, Guid skuId, int quantity)
    {
        var inventoryItem = StaticInventoryList.InventoryItems.FirstOrDefault(i => i.ProductId == productId);
        if (inventoryItem != null)
        {
            var variant = inventoryItem.Variants.FirstOrDefault(v => v.Id == productId);
            if (variant != null)
            {
                var sku = StaticSkuList.Skus.FirstOrDefault(s => s.Id == skuId);
                if (sku != null)
                {
                    variant.Skus.Add(new ProductVariantSkuDto
                    {
                        Id = Guid.NewGuid(),
                        Name = sku.Name,
                        Description = sku.Description,
                        Cost = sku.Cost,
                        Quantity = quantity
                    });
                }
            }
        }
        return Task.Delay(500);
    }

    public Task RemoveVariantSku(Guid variantId, Guid skuId)
    {
        StaticInventoryList.InventoryItems
            .SelectMany(i => i.Variants)
            .Where(v => v.Id == variantId)
            .ToList()
            .ForEach(v => v.Skus.ToList().RemoveAll(s => s.Id == skuId));
        return Task.Delay(500);
    }

    public Task UpdateVariantSkuQuantity(Guid variantId, Guid skuId, int quantity)
    {
        StaticInventoryList.InventoryItems
            .SelectMany(i => i.Variants)
            .Where(v => v.Id == variantId)
            .ToList()
            .ForEach(v =>
            {
                var sku = v.Skus.FirstOrDefault(s => s.Id == skuId);
                if (sku != null)
                {
                    sku.Quantity = quantity;
                }                
            });
        return Task.Delay(500);
    }

    public async Task<ProductVariantDto> GetVariantAsync(Guid variantId)
    {
        var variant = StaticInventoryList.InventoryItems
            .SelectMany(i => i.Variants)
            .FirstOrDefault(v => v.Id == variantId);
        await Task.Delay(800);
        return variant;
    }

    public Task CreateProductAsync(CreateProductDto createProductDto)
    {
        var newProductId = Guid.NewGuid();
        StaticInventoryList.InventoryItems.Add(new InventoryItemWithVariantsDto
        {
            ProductId = newProductId,
            ProductReference = createProductDto.ProductReference,
            ProductName = createProductDto.ProductName,
            Variants = createProductDto.Variants.Select(v => new ProductVariantDto
            {
                Id = Guid.NewGuid(),
                Name = v.Name,
                Price = v.Price,
                Active = false,
                Skus = []
            }).ToList()
        });
        return Task.Delay(500);
    }
}
