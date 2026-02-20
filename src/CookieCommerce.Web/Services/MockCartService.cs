using CookieCommerce.Web.Models.Cart;
using CookieCommerce.Web.Services.Products;

namespace CookieCommerce.Web.Services;

public class MockCartService : ICartService
{
    private CartDto _cart;
    
    public event Action? OnCartUpdated;

    public MockCartService()
    {
       _cart = new CartDto
        {
           CartId = Guid.NewGuid(),
           Items = []
        };
    }

    public async Task AddToCartAsync(Guid productId, int quantity)
    {
        var product = StaticCookieList.Cookies.FirstOrDefault(p => p.Id == productId);
        if (product == null) return;
        
        _cart.Items.Add(new CartItemDto
        {
            CartItemId = Guid.NewGuid(),
            ProductImageUrl = product.ImageUrls[0],
            ProductId = product.Id,
            ProductName = product.Title,
            ProductVariantName = "1 item",
            Quantity = quantity,
            UnitPrice = product.Price,
            IsLimitedEdition = product.IsLimitedEdition
        });
        
        OnCartUpdated?.Invoke();
    }

    public Task<CartDto> GetCartAsync()
    {
        return Task.FromResult(_cart);
    }

    public Task<int> GetCartItemCountAsync()
    {
        return Task.FromResult(_cart.Items.Count);
    }

    public Task RemoveItemAsync(Guid itemId)
    {
        _cart.Items.RemoveAll(i => i.CartItemId == itemId);
        OnCartUpdated?.Invoke();
        return Task.CompletedTask;
    }

    public Task UpdateQuantityAsync(Guid itemId, int quantity)
    {
        var item = _cart.Items.FirstOrDefault(i => i.CartItemId == itemId);
        if (item != null)
        {
            item.Quantity = quantity;
            OnCartUpdated?.Invoke();
        }
        return Task.CompletedTask;
    }
}
