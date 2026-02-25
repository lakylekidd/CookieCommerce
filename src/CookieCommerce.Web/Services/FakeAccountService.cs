using CookieCommerce.Web.Models;
using CookieCommerce.Web.Models.Account;

namespace CookieCommerce.Web.Services;

public class FakeAccountService() : IAccountService
{
    private List<Guid> _favoriteProductIds = [Guid.Parse("d8f5a8e1-1234-4567-89ab-444444444444")];

    private UserProfileDto _profile = new UserProfileDto
    {
        FirstName = "Cookie",
        LastName = "Lover",
        Email = "hello@tigercookie.com",
        Gender = "male",
        DateOfBirth = new DateTime(1986, 11, 18)
    };

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

    public  Task<UserProfileDto> GetProfileAsync()
    {
        return Task.FromResult(_profile);
    }

    public Task<ResultDto> UpdateProfileAsync(UserProfileDto profile)
    {
        _profile = profile;

        var result = new ResultDto
        {
            Success = true,
            Message = "Profile updated successfully"
        };
        return Task.FromResult(result);
    }
}
