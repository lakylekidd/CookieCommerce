using CookieCommerce.Web.Models.Backoffice;

namespace CookieCommerce.Web.Services;

public interface IBackofficeService
{
    Task<DashboardDto> GetTotalsAsync(string period = "this week");
}
