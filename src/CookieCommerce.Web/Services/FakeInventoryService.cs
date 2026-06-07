using CookieCommerce.Web.Models.Backoffice;
using CookieCommerce.Web.Services.Products;
using CookieCommerce.Web.Services.StaticData;

namespace CookieCommerce.Web.Services;

public class FakeInventoryService : IInventoryService
{
    public async Task<InventoryAreaDto> GetInventoryAsync()
        => await GetInventoryForDateAsync(DateOnly.FromDateTime(DateTime.Today));

    public async Task<InventoryAreaDto> GetInventoryForDateAsync(DateOnly date)
    {
        await Task.Delay(500);
        return MockInventoryData.GetInventoryForDate(date);
    }

    public async Task<IReadOnlyList<BatchProductDto>> GetAvailableProductsAsync()
    {
        await Task.Delay(200);
        return StaticInventoryList.InventoryItems
            .Select(p => new BatchProductDto
            {
                Id = p.ProductId,
                Reference = p.ProductReference,
                Name = p.ProductName,
                ImageUrl = p.ProductMainImage!.Url
            })
            .ToList();
    }
}
