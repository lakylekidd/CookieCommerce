using CookieCommerce.Web.Models.Checkout;

namespace CookieCommerce.Web.Services;

public interface ICheckoutService
{
    Task<CheckoutResult> CheckoutGuestAsync(GuestCheckoutRequest request);
    Task<CheckoutResult> CheckoutNewCustomerAsync(NewCustomerCheckoutRequest request);
    Task<CheckoutResult> CheckoutExistingCustomerAsync(ExistingCustomerCheckoutRequest request);
}
