using CookieCommerce.Web.Models.Backoffice;
using CookieCommerce.Web.Models.Order;

namespace CookieCommerce.Web.Services;

public interface IBackofficeService
{
    Task<DashboardDto> GetTotalsAsync(string period = "this week");
    Task<QuotaDateOrdersDto> GetQuotaDateOrdersAsync(DateTime quotaDate);
    
}
