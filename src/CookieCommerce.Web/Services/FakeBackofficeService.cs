using CookieCommerce.Web.Models.Backoffice;
using CookieCommerce.Web.Services.StaticData;

namespace CookieCommerce.Web.Services;

public class FakeBackofficeService : IBackofficeService
{
    public async Task<DashboardDto> GetTotalsAsync(string period)
    {
        await Task.Delay(800);
        var dashboard = MockDashboardData.GetDashboardByPeriod(period);
        return dashboard;
    }
}
