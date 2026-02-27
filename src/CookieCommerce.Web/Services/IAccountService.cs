using CookieCommerce.Web.Models;
using CookieCommerce.Web.Models.Account;
using CookieCommerce.Web.Models.Addresses;
using CookieCommerce.Web.Models.Order;

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
    Task<AddressBookItemDto>? GetPrimaryAddressAsync();
    Task<ICollection<OrderListItemDto>> GetOrdersAsync();
    Task<OrderDto> GetOrderByIdAsync(Guid orderId);
    Task<int> GetOrderCountInNeedOfAttentionAsync();
    Task<TrackingDto> GetTrackingAsync(Guid orderId);
}
