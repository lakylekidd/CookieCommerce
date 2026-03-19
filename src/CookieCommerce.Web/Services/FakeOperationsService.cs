using CookieCommerce.Web.Models;

namespace CookieCommerce.Web.Services;

public class FakeOperationsService : IOperationsService
{
    public Task<StoreDto> GetStoreAsync()
    {
        var store = new StoreDto
        {
            NextDayDeliveryBeforeTime = TimeSpan.FromHours(18)
        };
        return Task.FromResult(store);
    }
}
