namespace CookieCommerce.Web.Models.Order;

public class TrackingDto
{
    public string Carrier { get; set; } = null!;
    public string TrackingNumber { get; set; } = null!;
    public int TrackingStatus { get; set; }
    public string TrackingStatusDescription { get; set; } = null!;
}
