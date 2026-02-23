namespace CookieCommerce.Web.Models.Checkout;

/// <summary>
/// Represents a request to initiate a guest checkout process without requiring user authentication.
/// </summary>
/// <remarks>
/// Use this class to submit checkout information for users who do not have an account or do not wish to
/// create an account. Inherits all properties and behavior from <see cref="AnonymousCheckoutRequest"/>.
/// </remarks>
public class GuestCheckoutRequest : AnonymousCheckoutRequest
{

}
