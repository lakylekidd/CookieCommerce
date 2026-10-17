namespace CookieCommerce.Web.Models.Order;

public class QuotaDateOrdersDto
{
    public DateTime DeliveryOn { get; set; }
    public ICollection<QuotaOrderItemDto> Items { get; set; } = [];
    public ICollection<QuotaOrderDto> Orders { get; set; } = [];
}
