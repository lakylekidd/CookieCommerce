namespace CookieCommerce.Web.Models.Backoffice;

public class CookieRankDto
{
    public int Rank { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int SalesCount { get; set; }
}
