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

    public async Task AddToCartAsync(Guid productId, int quantity)
    {
        var productVariantId = StaticStorefrontProductList
            .Cookies
            .FirstOrDefault(p => p.Id == productId)
            ?.Variants.FirstOrDefault()?.Id;

        ArgumentNullException.ThrowIfNull(productVariantId, nameof(productVariantId));

        await AddToCartAsync(productId, productVariantId.Value, quantity);
    }

    public async Task AddToCartAsync(Guid productId, Guid productVariantId, int quantity)
    {
        await AddSingleToCartAsync(productId, productVariantId, quantity);
        await Task.Delay(800);
        NotifyStateChanged();
    }

    public async Task AddMultipleToCartAsync(IEnumerable<(Guid productId, Guid productVariantId, int quantity)> items)
    {
        foreach (var (productId, productVariantId, quantity) in items)
        {
            await AddSingleToCartAsync(productId, productVariantId, quantity);
        }

        await Task.Delay(800);
        NotifyStateChanged();
    }

    public async Task AddMultipleToCartAsync(IEnumerable<(Guid productId, int quantity)> items)
    {
        foreach (var (productId, quantity) in items)
        {
            await AddToCartAsync(productId, quantity);
        }

        await Task.Delay(800);
        NotifyStateChanged();        
    }

    private async Task AddSingleToCartAsync(Guid productId, Guid productVariantId, int quantity)
    {
        var product = StaticStorefrontProductList.Cookies.FirstOrDefault(p => p.Id == productId);
        var variant = product?.Variants.FirstOrDefault(v => v.Id == productVariantId);
        if (product == null || variant == null)
        {
            return;
        }

        var existingItem = _cart.Items.FirstOrDefault(i => i.ProductVariantId == productVariantId);
        if (existingItem is not null)
        {
            existingItem.Quantity += quantity;
            NotifyStateChanged();
            return;
        }

        _cart.Items.Add(new CartItemDto
        {
            CartItemId = Guid.NewGuid(),
            ProductImageUrl = variant.ImageUrls.First(),
            ProductVariantId = variant.Id,
            ProductName = product.Title,
            ProductVariantName = variant.Title,
            Quantity = quantity,
            UnitPrice = variant.Price,
            IsLimitedEdition = product.IsLimitedEdition
        });
    }
}
