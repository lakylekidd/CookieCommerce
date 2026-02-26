using CookieCommerce.Web.Models;
using CookieCommerce.Web.Models.Account;
using CookieCommerce.Web.Models.Addresses;

namespace CookieCommerce.Web.Services;

public interface IAccountService
{
    Task AddToFavoritesAsync(Guid productId);
    Task RemoveFromFavoritesAsync(Guid productId);
    Task<bool> IsFavoriteAsync(Guid productId);
    Task<UserProfileDto> GetProfileAsync();
    Task<ResultDto> UpdateProfileAsync(UserProfileDto profile);
    Task<IList<AddressBookItemDto>> GetAddressesAsync();
    Task<ResultDto> AddAddressAsync(AddressBookItemDto address);
    Task<ResultDto> DeleteAddressAsync(int index);
    Task<ResultDto> SetAsPrimaryAsync(int index);
}
