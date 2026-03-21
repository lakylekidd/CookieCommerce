using CookieCommerce.Web.Models.Backoffice;

namespace CookieCommerce.Web.Services;

public interface IInventoryService
{
    Task<InventoryAreaDto> GetInventoryAsync();
}
