using CookieCommerce.Web.Models;
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
    Task AddMultipleToCartAsync(IEnumerable<(Guid productId, int quantity)> items);
    Task AddNoteToCartAsync(string note);
    Task<ResultDto> ApplyPromotionCodeAsync(string code);
    Task<ResultDto> RemovePromotionCodeAsync(string code);
}
