namespace CookieCommerce.Web.Services;

public interface IAccountService
{
    Task AddToFavoritesAsync(Guid productId);
    Task RemoveFromFavoritesAsync(Guid productId);
    Task<bool> IsFavoriteAsync(Guid productId);
}
