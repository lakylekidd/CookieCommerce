namespace CookieCommerce.Web.Models.Checkout;

/// <summary>
/// Represents a checkout request made by an existing customer, including the user's email address, name, and user ID.
/// </summary>
/// <remarks>
/// Use this class to submit checkout information for users who have an account and are authenticated. 
/// Inherits all properties and behavior from <see cref="AnonymousCheckoutRequest"/> and adds 
/// a <see cref="UserId"/> property to identify the existing customer.
/// </remarks>
public class ExistingCustomerCheckoutRequest : AnonymousCheckoutRequest
{
    public string UserId { get; set; } = string.Empty;
}
