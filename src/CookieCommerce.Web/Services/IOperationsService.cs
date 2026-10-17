using CookieCommerce.Web.Models;

namespace CookieCommerce.Web.Services;

public interface IOperationsService
{
    Task<StoreDto> GetStoreAsync();
}
