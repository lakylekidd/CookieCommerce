using CookieCommerce.Web.Models.Cart;
using CookieCommerce.Web.Services.Products;

namespace CookieCommerce.Web.Services;

public class MockCartService : ICartService
{
    private List<CartItemDto> _cartItems = [];
    private CartDto _cart;

    public MockCartService()
    {
        var randomCookies = StaticCookieList.Cookies
            .OrderBy(_ => Random.Shared.Next())
            .Take(3)
            .ToList();

       _cart = new CartDto
        {
            Items = randomCookies.Select(cookie => new CartItemDto
            {
                CurrencySymbol = "$",
                ProductId = cookie.Id,
                ProductImageUrl = cookie.ImageUrls[0],
                ProductName = cookie.Title,
                IsLimitedEdition = cookie.IsLimitedEdition,
                ProductVariantName = "Box of 3",
                UnitPrice = cookie.Price,
                Quantity = 1
            }).ToList()
        };
    }

    public Task<CartDto> GetCartAsync()
    {
        return Task.FromResult(_cart);
    }

    public Task RemoveItemAsync(Guid itemId)
    {
        _cart.Items.RemoveAll(i => i.ProductId == itemId);
        return Task.CompletedTask;
    }

    public Task UpdateQuantityAsync(Guid itemId, int quantity)
    {
        var item = _cart.Items.FirstOrDefault(i => i.ProductId == itemId);
        if (item != null)
        {
            item.Quantity = quantity;
        }
        return Task.CompletedTask;
    }
}
