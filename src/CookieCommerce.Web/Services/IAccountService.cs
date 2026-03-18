using CookieCommerce.Web.Models;
using CookieCommerce.Web.Models.Account;
using CookieCommerce.Web.Models.Addresses;
using CookieCommerce.Web.Models.Order;
using CookieCommerce.Web.Models.Products;
using CookieCommerce.Web.Models.Reviews;

namespace CookieCommerce.Web.Services;

public interface IAccountService
{
    Task AddToFavoritesAsync(Guid productId);
    Task RemoveFromFavoritesAsync(Guid productId);
    Task<bool> IsFavoriteAsync(Guid productId);
    Task<ICollection<StorefrontProductDto>> GetFavoriteProductsAsync();
    Task<UserProfileDto> GetProfileAsync();
    Task<ResultDto> UpdateProfileAsync(UserProfileDto profile);
    Task<IList<AddressBookItemDto>> GetAddressesAsync();
    Task<ResultDto> AddAddressAsync(AddressBookItemDto address);
    Task<ResultDto> DeleteAddressAsync(int index);
    Task<ResultDto> SetAsPrimaryAsync(int index);
    Task<AddressBookItemDto>? GetPrimaryAddressAsync();
    Task<ICollection<OrderListItemDto>> GetOrdersAsync(int page, int pageSize);
    Task<int> GetTotalOrdersCountAsync();
    Task<OrderDto> GetOrderByIdAsync(Guid orderId);
    Task<int> GetOrderCountInNeedOfAttentionAsync();
    Task<TrackingDto> GetTrackingAsync(Guid orderId);
    Task SubmitReviewAsync(Guid productId, ReviewSubmitRequest review);
    Task<(bool,int)> HasAlreadyReviewedProductIdAsync(Guid productId);
}
