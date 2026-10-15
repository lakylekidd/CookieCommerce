using CookieCommerce.Web.Components.Storefront.Legal;
using CookieCommerce.Web.Models.Addresses;
using CookieCommerce.Web.Models.Cart;

namespace CookieCommerce.Web.Models.Order;

public class OrderDto
{
    public Guid Id { get; set; }
    public string OrderReference { get; set; } = string.Empty;
    public DateTime PlacedOn { get; set; }
    public AddressBookItemDto ShippingAddress { get; set; } = new();
    public AddressBookItemDto BillingAddress { get; set; } = new();
    public ICollection<OrderProductDto> Products { get; set; } = [];
    public OrderStatusDto Status { get; set; } = new();
    public string CurrencySymbol { get; set; } = "€";
    public decimal? ShippingCharge { get; set; }
    public decimal TaxRate { get; set; } = CartDto.TAX_PERCENTAGE;
    public bool TaxIncluded { get; set; } = true;
    public CartPromotionDto? Promotion { get; set; } = null;
    public decimal RewardDiscount { get; set; }
    public bool IsPromoApplied => Promotion != null;
    public decimal Total { get; set; }
    public decimal Tax => TaxIncluded ? SubTotalPrice * TaxRate / (1 + TaxRate) : SubTotalPrice * TaxRate;
    public decimal Shipping => ShippingCharge ?? (SubTotalPrice >= CartDto.FREE_SHIPPING_THRESHOLD ? 0 : CartDto.SHIPPING_COST);
    public decimal SubTotalPrice => Products.Sum(item => item.UnitPrice * item.Quantity);
    public decimal TotalPrice => SubTotalPrice + (!TaxIncluded ? Tax : 0) + Shipping - GetPromotionDiscount() - RewardDiscount;
    public TrackingDto TrackingInformation { get; set; } = new();

    public decimal GetPromotionDiscount()
    {
        if (Promotion == null) return 0;
        return Math.Clamp(Promotion.IsPercentage ? decimal.Round(SubTotalPrice * Promotion.DiscountAmount, 2) : Promotion.DiscountAmount, 0, SubTotalPrice);
    }
}
