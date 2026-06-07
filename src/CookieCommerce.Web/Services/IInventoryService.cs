using CookieCommerce.Web.Models.Backoffice;

namespace CookieCommerce.Web.Services;

public interface IInventoryService
{
    Task<InventoryAreaDto> GetInventoryAsync();
    Task<InventoryAreaDto> GetInventoryForDateAsync(DateOnly date);
    Task<IReadOnlyList<BatchProductDto>> GetAvailableProductsAsync();
}
