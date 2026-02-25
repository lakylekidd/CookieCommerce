using CookieCommerce.Web.Models;
using CookieCommerce.Web.Models.Account;

namespace CookieCommerce.Web.Services;

public interface IAccountService
{
    Task AddToFavoritesAsync(Guid productId);
    Task RemoveFromFavoritesAsync(Guid productId);
    Task<bool> IsFavoriteAsync(Guid productId);
    Task<UserProfileDto> GetProfileAsync();
    Task<ResultDto> UpdateProfileAsync(UserProfileDto profile);
}
