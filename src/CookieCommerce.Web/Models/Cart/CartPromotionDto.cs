namespace CookieCommerce.Web.Models.Cart;

public class CartPromotionDto
{
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal DiscountAmount { get; set; }
    public bool IsPercentage { get; set; } = false;
}
