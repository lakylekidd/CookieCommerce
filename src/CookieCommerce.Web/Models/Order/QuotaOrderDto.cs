namespace CookieCommerce.Web.Models.Order;

public class QuotaOrderDto
{
    public Guid Id { get; set; }
    public string OrderReference { get; set; } = string.Empty;
    public DateTime PlacedOn { get; set; }
    public QuotaOrderAddressDto Address { get; set; } = new();
    public OrderStatusDto Status { get; set; } = new();
    public ShippingInformationDto ShippingInformation { get; set; } = new();
    public ICollection<QuotaOrderItemDto> Items { get; set; } = [];
    public string CurrencySymbol { get; set; } = "$";
    public decimal Total { get; set; }
}
