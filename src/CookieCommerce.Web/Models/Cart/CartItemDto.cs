namespace CookieCommerce.Web.Models.Cart;

public class CartItemDto
{
    public Guid CartItemId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductVariantName { get; set; } = string.Empty;
    public string ProductImageUrl { get; set; } = string.Empty;
    public bool IsLimitedEdition { get; set; }
    public string CurrencySymbol { get; set; } = "$";
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice => UnitPrice * Quantity;
}
