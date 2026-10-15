using CookieCommerce.Web.Models.Addresses;
using CookieCommerce.Web.Models.Checkout;

namespace CookieCommerce.Web.Services;

public class FakeCheckoutService : ICheckoutService
{
    public async Task<IReadOnlyList<DeliveryInfoDto>> GetDeliveryMethodsAsync(AddressBookItemDto address)
    {
        await Task.Delay(800);

        if (string.Equals(address.Country, "The Netherlands", StringComparison.OrdinalIgnoreCase))
        {
            if (string.Equals(address.City, "Amsterdam", StringComparison.OrdinalIgnoreCase))
            {
                return [
                    new DeliveryInfoDto
                    {
                        Title = "Peddler",
                        DeliveryDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
                        Price = 5.95m
                    }
                ];
            }

            return [
                new DeliveryInfoDto
                {
                    Title = "Skynet",
                    DeliveryDate = DateOnly.FromDateTime(DateTime.Today.AddDays(2)),
                    Price = 4.95m
                },
                new DeliveryInfoDto
                {
                    Title = "Easy2Send",
                    DeliveryDate = DateOnly.FromDateTime(DateTime.Today.AddDays(3)),
                    Price = 6.95m
                }
            ];
        }

        return [
            new DeliveryInfoDto
            {
                Title = "PostNL",
                DeliveryDate = DateOnly.FromDateTime(DateTime.Today.AddDays(4)),
                Price = 9.95m
            }
        ];
    }

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
