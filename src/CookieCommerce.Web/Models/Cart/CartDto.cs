namespace CookieCommerce.Web.Models.Cart;

public class CartDto
{
    public const int FREE_SHIPPING_THRESHOLD = 40;
    public const decimal SHIPPING_COST = 4.95m;
    public const decimal TAX_PERCENTAGE = 0.07m;

    public Guid CartId { get; set; }
    public Guid? UserId { get; set; }
    public List<CartItemDto> Items { get; set; } = new List<CartItemDto>();
    public string CurrencySymbol { get; set; } = "$";
    public bool TaxIncluded { get; set; } = true;
    public decimal Tax => SubTotalPrice * TAX_PERCENTAGE;
    public decimal Shipping => SubTotalPrice >= FREE_SHIPPING_THRESHOLD ? 0 : SHIPPING_COST;
    public decimal SubTotalPrice => Items.Sum(item => item.UnitPrice * item.Quantity);
    public decimal TotalPrice => SubTotalPrice + (!TaxIncluded ? Tax : 0) + Shipping;
    public decimal ToGoUntilFreeShipping => Shipping == 0 ? 0 : FREE_SHIPPING_THRESHOLD - SubTotalPrice;
    public string FreeShippingMessage => Shipping == 0 
        ? "You have free shipping!" 
        : $"Add another box of Tiger Strips to unlock free delivery!";
}
