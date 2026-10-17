namespace CookieCommerce.Web.Models.Checkout;

public class DeliveryInfoDto
{
    public string Title { get; set; } = string.Empty;
    public DateOnly DeliveryDate { get; set; }
    public decimal Price { get; set; }
}
