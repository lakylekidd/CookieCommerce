namespace CookieCommerce.Web.Services;

public class FakeAccountService : IAccountService
{
    private List<Guid> _favoriteProductIds = [Guid.Parse("d8f5a8e1-1234-4567-89ab-444444444444")];

    public Task AddToFavoritesAsync(Guid productId)
    {
        if (_favoriteProductIds.Contains(productId)) return Task.CompletedTask;
        _favoriteProductIds.Add(productId);
        return Task.CompletedTask;
    }

    public Task RemoveFromFavoritesAsync(Guid productId)
    {
        _favoriteProductIds.Remove(productId);
        return Task.CompletedTask;
    }

    public Task<bool> IsFavoriteAsync(Guid productId)
    {
        return Task.FromResult(_favoriteProductIds.Contains(productId));
    }
}
