namespace CookieCommerce.Web.Models.Order;

public class OrderProductDto
{
    public Guid ProductVariantId { get; set; }
    public string ProductVariantName { get; set; } = "";
    public int UnitsPerItem { get; set; } = 1;
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductImageUrl { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string CurrencySymbol { get; set; } = "$";
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice => UnitPrice * Quantity;
    public bool IsLimitedEdition { get; set; }
}
