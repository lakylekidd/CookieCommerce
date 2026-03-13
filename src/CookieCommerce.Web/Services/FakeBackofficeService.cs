using CookieCommerce.Web.Models.Backoffice;
using CookieCommerce.Web.Services.StaticData;

namespace CookieCommerce.Web.Services;

public class FakeBackofficeService : IBackofficeService
{
    public Task<DashboardDto> GetTotalsAsync(string period)
    {
        var dashboard = MockDashboardData.GetDashboardByPeriod(period);
        return Task.FromResult(dashboard);
    }
}
