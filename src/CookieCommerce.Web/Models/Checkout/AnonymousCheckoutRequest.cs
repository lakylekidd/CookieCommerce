namespace CookieCommerce.Web.Models.Checkout;

/// <summary>
/// Represents a checkout request made by an anonymous user, including the user's email address and name.
/// </summary>
public abstract class AnonymousCheckoutRequest : BaseCheckoutRequest
{
    public string FristName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool SubscribeNewsletter { get; set; }
}
