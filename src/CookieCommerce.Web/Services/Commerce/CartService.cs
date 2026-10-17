using CookieCommerce.Web.Models;
using CookieCommerce.Web.Models.Cart;
using CookieCommerce.Web.Services.Products;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace CookieCommerce.Web.Services.Commerce;

public sealed class CartService(ICommerceStore store, ICookieService catalog, ProtectedLocalStorage storage, ProductMainImages? mainImages = null) : ICartService
{
    private const string StorageKey = "commerce_cart_key";
    private string? _key;
    private bool _interactive;
    private readonly SemaphoreSlim identityLock = new(1);
    public event Action? OnCartUpdated;

    public async Task RestoreAsync() { await GetKeyAsync(); OnCartUpdated?.Invoke(); }

    public async Task<string> GetKeyAsync()
    {
        if (_interactive) return _key!;
        await identityLock.WaitAsync();
        try
        {
            if (_interactive) return _key!;
            var result = await storage.GetAsync<string>(StorageKey);
            _key = result.Success && !string.IsNullOrWhiteSpace(result.Value) ? result.Value : Guid.CreateVersion7().ToString();
            await storage.SetAsync(StorageKey, _key);
            _interactive = true;
        }
        catch (InvalidOperationException) { _key ??= Guid.CreateVersion7().ToString(); }
        catch (System.Security.Cryptography.CryptographicException)
        {
            _key = Guid.CreateVersion7().ToString();
            await storage.SetAsync(StorageKey, _key);
            _interactive = true;
        }
        finally { identityLock.Release(); }
        return _key;
    }

    public async Task<CartDto> GetCartAsync()
    {
        var cart = (await store.LoadAsync<StoredCart>(await GetKeyAsync()) ?? new()).Cart;
        var config = await GetSettingsAsync();
        cart.FreeShippingThreshold = config.FreeShippingThreshold;
        cart.ShippingCost = config.StandardShippingCost;
        cart.TaxRate = config.VatRate / 100;
        if (mainImages is not null) await mainImages.ApplyAsync(cart.Items);
        return cart;
    }

    public async Task<int> GetCartItemCountAsync() => (await store.LoadAsync<StoredCart>(await GetKeyAsync()))?.Cart.Items.Sum(i => i.Quantity) ?? 0;

    public async Task ConvertToUserCartAsync(Guid userId)
    {
        var oldKey = await GetKeyAsync();
        var newKey = $"customer:{userId}";
        if (oldKey == newKey) return;
        var converted = await store.ExecuteAsync([oldKey, newKey, "loyalty"], async tx =>
        {
            var anonymous = await tx.LoadAsync<StoredCart>(oldKey);
            var target = await tx.LoadAsync<StoredCart>(newKey) ?? new();
            if (anonymous?.Cart.UserId is { } previousOwner && previousOwner != userId ||
                target.PendingOrderId is not null && anonymous?.PendingOrderId is null && (anonymous is null || anonymous.Cart.Items.Count == 0))
            {
                target.Cart.UserId = userId;
                await tx.SaveAsync(newKey, target);
                return true;
            }
            if (anonymous?.PendingOrderId is not null || target.PendingOrderId is not null)
            {
                if (anonymous is not null && (anonymous.Cart.UserId is null || anonymous.Cart.UserId == userId))
                {
                    anonymous.Cart.UserId = userId;
                    if (anonymous.PendingOrderId is { } orderId && await tx.LoadAsync<CommerceOrder>(orderId.ToString()) is { } order)
                    { order.CustomerId = userId; await tx.SaveAsync(orderId.ToString(), order); }
                    await tx.SaveAsync(oldKey, anonymous);
                }
                return false;
            }
            if (anonymous is not null && (anonymous.Cart.UserId is null || anonymous.Cart.UserId == userId))
            {
                foreach (var item in anonymous.Cart.Items)
                {
                    var existing = target.Cart.Items.FirstOrDefault(i => i.ProductVariantId == item.ProductVariantId);
                    if (existing is null) target.Cart.Items.Add(item);
                    else existing.Quantity = checked(existing.Quantity + item.Quantity);
                }
                await tx.DeleteAsync<StoredCart>(oldKey);
            }
            target.Cart.UserId = userId;
            await tx.SaveAsync(newKey, target);
            return true;
        });
        if (!converted) { OnCartUpdated?.Invoke(); return; }
        _key = newKey;
        await storage.SetAsync(StorageKey, _key);
        OnCartUpdated?.Invoke();
    }

    public async Task ResetIdentityAsync()
    {
        _key = Guid.CreateVersion7().ToString();
        await storage.SetAsync(StorageKey, _key);
        _interactive = true;
        OnCartUpdated?.Invoke();
    }

    public Task RemoveItemAsync(Guid itemId) => MutateAsync(cart => cart.Items.RemoveAll(i => i.CartItemId == itemId));
    public Task UpdateQuantityAsync(Guid itemId, int quantity) => MutateAsync(cart =>
    {
        if (quantity < 0 || quantity > 120) throw new InvalidOperationException("Quantity must be between 0 and 120.");
        if (quantity == 0) cart.Items.RemoveAll(i => i.CartItemId == itemId);
        else if (cart.Items.FirstOrDefault(i => i.CartItemId == itemId) is { } item) item.Quantity = quantity;
    });
    public Task AddNoteToCartAsync(string note) => MutateAsync(cart =>
    {
        if (note.Length > 2000) throw new InvalidOperationException("Order notes can contain up to 2000 characters.");
        cart.Note = note.Trim();
    });
    public Task SetShippingAsync(decimal? price) => MutateAsync(cart => cart.SelectedShippingCost = price);

    public async Task AddToCartAsync(Guid productId, int quantity)
    {
        var product = await catalog.GetProductByIdAsync(productId) ?? throw new InvalidOperationException("This product is unavailable.");
        await AddToCartAsync(productId, product.Variants.First().Id, quantity);
    }

    public async Task AddToCartAsync(Guid productId, Guid productVariantId, int quantity)
    {
        if (quantity <= 0 || quantity > 120) throw new InvalidOperationException("Quantity must be between 1 and 120.");
        var product = await catalog.GetProductByIdAsync(productId) ?? throw new InvalidOperationException("This product is unavailable.");
        var variant = product.Variants.FirstOrDefault(v => v.Id == productVariantId) ?? throw new InvalidOperationException("This variant is unavailable.");
        await MutateAsync(cart =>
        {
            var existing = cart.Items.FirstOrDefault(i => i.ProductVariantId == productVariantId);
            if (existing is not null) existing.Quantity = checked(existing.Quantity + quantity);
            else cart.Items.Add(new CartItemDto
            {
                CartItemId = Guid.CreateVersion7(), ProductId = productId, ProductVariantId = productVariantId,
                ProductName = product.Title, ProductVariantName = variant.Title, UnitPrice = variant.Price,
                ProductImageUrl = product.ImageUrl,
                CurrencySymbol = "€", Quantity = quantity, IsLimitedEdition = product.IsLimitedEdition
            });
        });
    }

    public async Task AddMultipleToCartAsync(IEnumerable<(Guid productId, int quantity)> items)
    { foreach (var (id, quantity) in items) await AddToCartAsync(id, quantity); }
    public async Task AddMultipleToCartAsync(IEnumerable<(Guid productId, Guid productVariantId, int quantity)> items)
    { foreach (var (id, variant, quantity) in items) await AddToCartAsync(id, variant, quantity); }

    public async Task<ResultDto> ApplyPromotionCodeAsync(string code)
    {
        if ((await GetCartAsync()).RewardPointsToRedeem > 0) return ResultDto.Fail("Remove your reward points before applying a promotion code.");
        var promo = (await GetSettingsAsync()).Promotions.FirstOrDefault(p => p.IsActive && p.Code.Equals(code.Trim(), StringComparison.OrdinalIgnoreCase));
        if (promo is null) return ResultDto.Fail("Invalid promotion code.");
        try
        {
            await MutateAsync(cart =>
            {
                if (cart.RewardPointsToRedeem > 0) throw new InvalidOperationException("Remove reward points before applying a promotion.");
                cart.Promotion = new CartPromotionDto { Code = promo.Code, Description = promo.Description, IsPercentage = promo.IsPercentage, DiscountAmount = promo.IsPercentage ? promo.Discount / 100 : promo.Discount };
            });
        }
        catch (InvalidOperationException ex) { return ResultDto.Fail(ex.Message); }
        return ResultDto.Ok("Promotion applied.");
    }

    public async Task<int> GetRewardBalanceAsync()
    {
        var cart = await GetCartAsync();
        return cart.UserId is { } id ? LoyaltyRules.Balance(await store.ListAsync<CommerceOrder>(), id) : 0;
    }

    public async Task<ResultDto> ApplyRewardPointsAsync(int points)
    {
        var config = await GetSettingsAsync();
        var key = await GetKeyAsync();
        var result = await store.ExecuteAsync(key, async tx =>
        {
            var state = await tx.LoadAsync<StoredCart>(key) ?? new();
            if (state.PendingOrderId is not null) return ResultDto.Fail("Finish or cancel checkout before changing rewards.");
            if (points == 0) { state.Cart.RewardPointsToRedeem = 0; await tx.SaveAsync(key, state); return ResultDto.Ok("Reward points removed."); }
            if (state.Cart.UserId is not { } customerId) return ResultDto.Fail("Sign in to use reward points.");
            if (!config.Preference("allowPointRedemption", true)) return ResultDto.Fail("Reward redemption is currently disabled.");
            if (state.Cart.Promotion is not null) return ResultDto.Fail("Remove the promotion code before applying reward points.");
            var balance = LoyaltyRules.Balance(await store.ListAsync<CommerceOrder>(), customerId);
            if (points < Math.Max(1, config.Preference("minPointsToRedeem", 100)) || points > balance || points * LoyaltyRules.PointValue > state.Cart.SubTotalPrice)
                return ResultDto.Fail("Check your balance, minimum redemption and cart subtotal.");
            state.Cart.RewardPointsToRedeem = points;
            await tx.SaveAsync(key, state);
            return ResultDto.Ok("Reward points applied.");
        });
        OnCartUpdated?.Invoke();
        return result;
    }

    public async Task<ResultDto> RemovePromotionCodeAsync(string code)
    {
        await MutateAsync(cart => cart.Promotion = null);
        return ResultDto.Ok("Promotion removed.");
    }

    private async Task MutateAsync(Action<CartDto> action)
    {
        var key = await GetKeyAsync();
        var config = await GetSettingsAsync();
        await store.ExecuteAsync(key, async tx =>
        {
            var state = await tx.LoadAsync<StoredCart>(key) ?? new();
            if (state.PendingOrderId is not null) throw new InvalidOperationException("Finish or cancel the current checkout before changing your cart.");
            action(state.Cart);
            if (state.Cart.Items.Sum(i => i.Quantity) > config.MaxItemsPerOrder) throw new InvalidOperationException($"An order can contain at most {config.MaxItemsPerOrder} items.");
            await tx.SaveAsync(key, state);
            return true;
        });
        OnCartUpdated?.Invoke();
    }

    private async Task<CommerceSettings> GetSettingsAsync() => await store.LoadAsync<CommerceSettings>("store") ?? new();
}
