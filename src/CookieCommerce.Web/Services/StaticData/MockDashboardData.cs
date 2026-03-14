using CookieCommerce.Web.Models.Backoffice;
using CookieCommerce.Web.Services.Products;
using System;

namespace CookieCommerce.Web.Services.StaticData;

public static class MockDashboardData
{
    public static DashboardDto GetDashboardByPeriod(string period)
    {
        return period.ToLower() switch
        {
            "this week" => GetWeeklyData(),
            "this month" => GetMonthlyData(),
            "this year" => GetYearlyData(),
            _ => GetWeeklyData()
        };
    }

    private static DashboardDto GetWeeklyData() => new()
    {
        Period = "This Week",
        TotalRevenue = 1250.50m,
        TotalRevenuePercentageChange = 12.5m,
        TotalOrders = 85,
        TotalOrdersPercentageChange = 5.2m,
        TotalCustomers = 42,
        TotalCustomersPercentageChange = 8.1m,
        AverageRating = 4.8,
        Revenue = [
            ("Mon", 150m), ("Tue", 210m), ("Wed", 180m),
            ("Thu", 250m), ("Fri", 320m), ("Sat", 140m), ("Sun", 0m)
        ],
        TopSellingCookies = GetTopCookies()
    };

    private static DashboardDto GetMonthlyData() => new()
    {
        Period = "This Month",
        TotalRevenue = 5420.00m,
        TotalRevenuePercentageChange = -2.4m,
        TotalOrders = 312,
        TotalOrdersPercentageChange = 1.8m,
        TotalCustomers = 128,
        TotalCustomersPercentageChange = 15.3m,
        AverageRating = 4.7,
        Revenue = [
            ("Week 1", 1200m), ("Week 2", 1500m),
            ("Week 3", 1100m), ("Week 4", 1620m)
        ],
        TopSellingCookies = GetTopCookies()
    };

    private static DashboardDto GetYearlyData() => new()
    {
        Period = "This Year",
        TotalRevenue = 62450.00m,
        TotalRevenuePercentageChange = 24.8m,
        TotalOrders = 4120,
        TotalOrdersPercentageChange = 18.2m,
        TotalCustomers = 850,
        TotalCustomersPercentageChange = 32.1m,
        AverageRating = 4.9,
        Revenue = [
            ("Jan", 4500m), ("Feb", 4200m), ("Mar", 5100m), ("Apr", 4800m),
            ("May", 5300m), ("Jun", 6100m), ("Jul", 5900m), ("Aug", 5200m),
            ("Sep", 5400m), ("Oct", 5600m), ("Nov", 6800m), ("Dec", 8500m)
        ],
        TopSellingCookies = GetTopCookies()
    };

    private static List<CookieRankDto> GetTopCookies()
    {
        var cookies = StaticCookieList.Cookies.Take(4).ToList();

        var random = new Random();
        var cookieData = cookies.Select((cookie, index) => new
        {
            cookie.Title,
            cookie.ImageUrl,
            // Generate a random number between 100 and 1000
            Sales = random.Next(100, 1000),
            // Revenue is Sales * a random price between 2.50 and 5.00
            Rev = (decimal)(random.Next(100, 1000) * (random.NextDouble() * (5.0 - 2.5) + 2.5))
        }).ToList();

        return cookieData
            .OrderByDescending(c => c.Sales)
            .Select((c, index) => new CookieRankDto
            {
                Rank = index + 1,
                Name = c.Title,
                ImageUrl = c.ImageUrl,
                SalesCount = c.Sales,
                Revenue = Math.Round(c.Rev, 2)
            })
            .ToList();
    }
}
