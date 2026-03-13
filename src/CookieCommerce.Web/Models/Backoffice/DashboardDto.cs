namespace CookieCommerce.Web.Models.Backoffice;

public class DashboardDto
{
    public string Period { get; set; } = string.Empty;
    public decimal TotalRevenue { get; set; }
    public decimal TotalRevenuePercentageChange { get; set; }
    public int TotalOrders { get; set; }
    public decimal TotalOrdersPercentageChange { get; set; }
    public int TotalCustomers { get; set; }
    public decimal TotalCustomersPercentageChange { get; set; }
    public double AverageRating { get; set; }
    public List<(string Day, decimal Amount)> Revenue { get; set; } = [];
    public List<CookieRankDto> TopSellingCookies { get; set; } = [];
}