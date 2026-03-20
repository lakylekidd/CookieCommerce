using CookieCommerce.Web.Models.Addresses;
using CookieCommerce.Web.Models.Checkout;

namespace CookieCommerce.Web.Services;

public interface ICheckoutService
{
    Task<IReadOnlyList<DeliveryInfoDto>> GetDeliveryMethodsAsync(AddressBookItemDto address);
    Task<CheckoutResult> CheckoutGuestAsync(GuestCheckoutRequest request);
    Task<CheckoutResult> CheckoutNewCustomerAsync(NewCustomerCheckoutRequest request);
    Task<CheckoutResult> CheckoutExistingCustomerAsync(ExistingCustomerCheckoutRequest request);
}
