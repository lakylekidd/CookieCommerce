using CookieCommerce.Web.Models.Backoffice;
using CookieCommerce.Web.Services.StaticData;

namespace CookieCommerce.Web.Services;

public class FakeInventoryService : IInventoryService
{
    public Task<InventoryAreaDto> GetInventoryAsync()
    {
        var inventory = MockInventoryData.InventoryArea;
        return Task.FromResult(inventory);
    }
}
