using CookieCommerce.Web.Models.Cart;

namespace CookieCommerce.Web.Services;

public interface ICartService
{
    Task<CartDto> GetCartAsync();
    Task UpdateQuantityAsync(Guid itemId, int quantity);
    Task RemoveItemAsync(Guid itemId);
}
