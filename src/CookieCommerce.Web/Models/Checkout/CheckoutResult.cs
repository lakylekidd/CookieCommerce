namespace CookieCommerce.Web.Models.Checkout;

public class CheckoutResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public Guid? OrderId { get; set; }
    public string? PaymentUrl { get; set; }
}
