namespace CookieCommerce.Web.Models.Order;

public class ShippingInformationDto
{
    public string Icon { get; set; } = string.Empty;
    public string Carrier { get; set; } = string.Empty;
    public string TrackingNumber { get; set; } = string.Empty;
    public DateTime EstimatedDelivery { get; set; }
}
