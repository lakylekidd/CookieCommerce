namespace CookieCommerce.Web.Models.Checkout;

/// <summary>
/// The base class for checkout requests, containing common properties such as 
/// shipping and billing addresses, delivery method, and payment method. 
/// This class can be extended by specific checkout request types, such as anonymous or registered user checkouts.
/// </summary>
public abstract class BaseCheckoutRequest
{
    public string CartId { get; set; } = null!;
    public AddressDto ShippingAddress { get; set; } = null!;
    public AddressDto BillingAddress { get; set; } = null!;
    public string DeliveryMethod { get; set; } = null!;
    public string PaymentMethod { get; set; } = null!;
}
