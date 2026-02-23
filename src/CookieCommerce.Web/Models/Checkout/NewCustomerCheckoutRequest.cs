namespace CookieCommerce.Web.Models.Checkout;

/// <summary>
/// Represents a checkout request made by a new customer, including the user's email address, name, and password.
/// </summary>
/// <remarks>
/// Use this class to submit checkout information for users who are not registered and need to create an account.
/// Inherits all properties and behavior from <see cref="AnonymousCheckoutRequest"/> and adds
/// properties for the user's email address, name, and password.
/// </remarks>
public class NewCustomerCheckoutRequest : AnonymousCheckoutRequest
{
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}
