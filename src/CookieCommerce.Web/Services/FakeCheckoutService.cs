using CookieCommerce.Web.Models.Checkout;

namespace CookieCommerce.Web.Services;

public class FakeCheckoutService : ICheckoutService
{
    public Task<CheckoutResult> CheckoutExistingCustomerAsync(ExistingCustomerCheckoutRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<CheckoutResult> CheckoutGuestAsync(GuestCheckoutRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<CheckoutResult> CheckoutNewCustomerAsync(NewCustomerCheckoutRequest request)
    {
        throw new NotImplementedException();
    }
}
