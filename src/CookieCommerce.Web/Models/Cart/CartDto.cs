namespace CookieCommerce.Web.Models.Cart;

public class CartDto
{
    public const int FREE_SHIPPING_THRESHOLD = 40;
    public const decimal SHIPPING_COST = 4.95m;
    public const decimal TAX_PERCENTAGE = 0.07m;

    public Guid CartId { get; set; }
    public Guid? UserId { get; set; }
    public List<CartItemDto> Items { get; set; } = new List<CartItemDto>();
    public string CurrencySymbol { get; set; } = "€";
    public string CurrencyCode { get; set; } = "EUR";
    public decimal FreeShippingThreshold { get; set; } = FREE_SHIPPING_THRESHOLD;
    public decimal ShippingCost { get; set; } = SHIPPING_COST;
    public decimal TaxRate { get; set; } = TAX_PERCENTAGE;
    public decimal? SelectedShippingCost { get; set; }
    public bool TaxIncluded { get; set; } = true;
    public string Note { get; set; } = string.Empty;
    public CartPromotionDto? Promotion { get; set; } = null;
    public int RewardPointsToRedeem { get; set; }
    public decimal RewardDiscount => Math.Min(SubTotalPrice, RewardPointsToRedeem * 0.01m);
    public bool IsPromoApplied => Promotion != null;
    public decimal Tax => TaxIncluded ? SubTotalPrice * TaxRate / (1 + TaxRate) : SubTotalPrice * TaxRate;
    public decimal Shipping => SubTotalPrice >= FreeShippingThreshold ? 0 : SelectedShippingCost ?? ShippingCost;
    public decimal SubTotalPrice => Items.Sum(item => item.UnitPrice * item.Quantity);
    public decimal TotalPrice => SubTotalPrice + (!TaxIncluded ? Tax : 0) + Shipping - GetPromotionDiscount() - RewardDiscount;
    public decimal ToGoUntilFreeShipping => Shipping == 0 ? 0 : FreeShippingThreshold - SubTotalPrice;
    public string FreeShippingMessage => Shipping == 0
        ? "You have free shipping!"
        : $"Add another box of Tiger Strips to unlock free delivery!";

    public decimal GetPromotionDiscount()
    {
        if (Promotion == null) return 0;
        return Math.Clamp(Promotion.IsPercentage ? decimal.Round(SubTotalPrice * Promotion.DiscountAmount, 2) : Promotion.DiscountAmount, 0, SubTotalPrice);
    }
}
