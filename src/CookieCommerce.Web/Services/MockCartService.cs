using CookieCommerce.Web.Models;
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
        var product = StaticStorefrontProductList.Cookies.FirstOrDefault(p => p.Id == productId);
        if (product == null) return;

        var existingItem = _cart.Items.FirstOrDefault(i => i.ProductId == productId); 
        if (existingItem is not null)
        {
            existingItem.Quantity += quantity;
            NotifyStateChanged();
            await Task.Delay(800);
            return;
        }

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

        await Task.Delay(800);

        NotifyStateChanged();
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
        NotifyStateChanged();
        return Task.CompletedTask;
    }

    public Task UpdateQuantityAsync(Guid itemId, int quantity)
    {
        var item = _cart.Items.FirstOrDefault(i => i.CartItemId == itemId);
        if (item != null)
        {
            item.Quantity = quantity;
            NotifyStateChanged();
        }
        return Task.CompletedTask;
    }

    public Task AddNoteToCartAsync(string note)
    {
        _cart.Note = note;
        NotifyStateChanged();
        return Task.CompletedTask;
    }

    private void NotifyStateChanged() => OnCartUpdated?.Invoke();

    public  Task ConvertToUserCartAsync(Guid userId)
    {
        if (_cart.UserId != null)
        {
            _cart = new CartDto
            {
                CartId = Guid.NewGuid(),
                UserId = userId,
                Items = []
            };
            return Task.CompletedTask;
        }
        _cart.UserId = userId;
        return Task.CompletedTask;
    }

    public Task<ResultDto> ApplyPromotionCodeAsync(string code)
    {
        if (code == "DISCOUNT10" && _cart is not null)
        {
            _cart.Promotion = new CartPromotionDto
            {
                Code = code,
                Description = "10% off your order!",
                DiscountAmount = 0.1m,
                IsPercentage = true
            };
            NotifyStateChanged();
            return Task.FromResult(new ResultDto { Success = true, Message = "Promotion code applied successfully." });
        }
        
        return Task.FromResult(new ResultDto { Success = false, Message = "Invalid promotion code." });
    }

    public Task<ResultDto> RemovePromotionCodeAsync(string code)
    {
        if (_cart.Promotion != null && _cart.Promotion.Code == code)
        {
            _cart.Promotion = null;
            NotifyStateChanged();
            return Task.FromResult(new ResultDto { Success = true, Message = "Promotion code removed successfully." });
        }
        
        return Task.FromResult(new ResultDto { Success = false, Message = "Promotion code not found in cart." });
    }

    public Task AddMultipleToCartAsync(IEnumerable<(Guid productId, int quantity)> items)
    {
        foreach (var (productId, quantity) in items)
        {
            var product = StaticStorefrontProductList.Cookies.FirstOrDefault(p => p.Id == productId);
            if (product == null) continue;
            var existingItem = _cart.Items.FirstOrDefault(i => i.ProductId == productId); 
            if (existingItem is not null)
            {
                existingItem.Quantity += quantity;
                continue;
            }
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
        }
        
        NotifyStateChanged();
        return Task.CompletedTask;
    }
}
