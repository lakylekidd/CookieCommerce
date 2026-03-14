using CookieCommerce.Web.Models.Backoffice;
using CookieCommerce.Web.Models.Order;
using CookieCommerce.Web.Services.StaticData;

namespace CookieCommerce.Web.Services;

public class FakeBackofficeService : IBackofficeService
{
    public async Task<QuotaDateOrdersDto> GetQuotaDateOrdersAsync(DateTime quotaDate)
    {
        var orders = MockOrderData.GetRecentOrders();
        await Task.Delay(800);
        return new QuotaDateOrdersDto
        {
            DeliveryOn = quotaDate,
            Items = orders
                .SelectMany(o => o.Items)
                .GroupBy(i => i.ProductId)
                .Select(g => new QuotaOrderItemDto
                {
                    ProductId = g.Key,
                    ProductImageUrl = g.First().ProductImageUrl,
                    ProductName = g.First().ProductName,
                    Quantity = g.Sum(i => i.Quantity)
                })
                .ToList(),
            Orders = orders.ToList()
        };
    }

    public async Task<DashboardDto> GetTotalsAsync(string period)
    {
        await Task.Delay(800);
        var dashboard = MockDashboardData.GetDashboardByPeriod(period);
        return dashboard;
    }
}
