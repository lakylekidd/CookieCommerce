using CookieCommerce.Web.Models.Cart;

namespace CookieCommerce.Web.Services;

public interface ICartService
{
    event Action? OnCartUpdated;
    Task ConvertToUserCartAsync(Guid userId);
    Task<int> GetCartItemCountAsync();
    Task<CartDto> GetCartAsync();
    Task UpdateQuantityAsync(Guid itemId, int quantity);
    Task RemoveItemAsync(Guid itemId);
    Task AddToCartAsync(Guid productId, int quantity);
}
