using CookieCommerce.Web.Models.Addresses;

namespace CookieCommerce.Web.Services;

public interface ILocationService
{
    Task<DutchAddressResult?> LookupDutchAddressAsync(string postCode, string streetNumber);
    Task<DutchAddressResult?> LookupDutchAddressAsync(string postCode, string streetNumber, string addition);
}
