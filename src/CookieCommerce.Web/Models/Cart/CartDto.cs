namespace CookieCommerce.Web.Models.Cart;

public class CartDto
{
    public Guid CartId { get; set; }
    public List<CartItemDto> Items { get; set; } = new List<CartItemDto>();
    public string CurrencySymbol { get; set; } = "$";
    public decimal Tax => SubTotalPrice * 0.07m;
    public decimal Shipping => SubTotalPrice > 40 ? 0 : 4.95m;
    public decimal SubTotalPrice => Items.Sum(item => item.UnitPrice * item.Quantity);
    public decimal TotalPrice => SubTotalPrice + Tax + Shipping;
    public decimal ToGoUntilFreeShipping => Shipping == 0 ? 0 : 40 - SubTotalPrice;
    public string FreeShippingMessage => Shipping == 0 
        ? "You have free shipping!" 
        : $"Add another box of Tiger Strips to unlock free delivery!";
}
