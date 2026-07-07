using CookieCommerce.Web.Models;
using CookieCommerce.Web.Models.Account;
using CookieCommerce.Web.Models.Addresses;
using CookieCommerce.Web.Models.Auth;
using CookieCommerce.Web.Models.Order;
using CookieCommerce.Web.Models.Products;
using CookieCommerce.Web.Models.Reviews;
using CookieCommerce.Web.Services.Products;
using CookieCommerce.Web.Services.StaticData;

namespace CookieCommerce.Web.Services;

public class FakeAccountService(IAuthService authService) : IAccountService
{
    public async Task AddToFavoritesAsync(Guid productId)
    {
        if (await GetProfileAsync() is not { } profile) return;
        if (profile.FavoriteProducts.Contains(productId)) return;
        profile.FavoriteProducts.Add(productId);
    }

    public async Task RemoveFromFavoritesAsync(Guid productId)
    {
        if (await GetProfileAsync() is not { } profile) return;
        profile.FavoriteProducts.Remove(productId);
    }

    public async Task<bool> IsFavoriteAsync(Guid productId)
    {
        if (await GetProfileAsync() is not { } profile) return false;
        return profile.FavoriteProducts.Contains(productId);
    }

    public async Task<ICollection<StorefrontProductDto>> GetFavoriteProductsAsync()
    {
        if (await GetProfileAsync() is not { } profile) return [];
        return profile.FavoriteProducts
            .Select(id => StaticStorefrontProductList.Cookies.FirstOrDefault(p => p.Id == id))
            .Where(p => p is not null)
            .ToList()!;
    }

    /// <summary>
    /// Profile operations are now handled by CustomerProfileService for customer-facing surfaces.
    /// This method remains for IAccountService compatibility (favorites, addresses).
    /// </summary>
    public async Task<UserProfileDto?> GetProfileAsync()
    {
        await authService.InitializeAsync();
        if (authService.CurrentUser is not UserDto user) return null;
        // Return a minimal DTO from auth state for non-profile purposes (e.g. favorites lookup).
        // Customer-facing profile display must use CustomerProfileService.
        return new UserProfileDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = user.Role
        };
    }

    /// <summary>
    /// Profile save is now handled by CustomerProfileService.
    /// This stub returns a failure to ensure no mock-data update occurs.
    /// </summary>
    public Task<ResultDto> UpdateProfileAsync(UserProfileDto profile)
    {
        return Task.FromResult(new ResultDto
        {
            Success = false,
            Message = "Profile updates must go through CustomerProfileService."
        });
    }

    public async Task<IList<AddressBookItemDto>> GetAddressesAsync()
    {
        if (await GetProfileAsync() is not UserProfileDto profile) return [];
        return profile.Addresses.ToList();
    }

    public async Task<ResultDto> AddAddressAsync(AddressBookItemDto address)
    {
        if (await GetProfileAsync() is not { } profile)
        {
            return new ResultDto { Success = false, Message = "Sign in to add an address" };
        }

        address.Index = profile.Addresses.Count > 0 ? profile.Addresses.Max(a => a.Index) + 1 : 1;
        profile.Addresses.Add(address);

        foreach (var a in profile.Addresses)
        {
            a.IsDefault = a.Index == address.Index;
        }

        var result = new ResultDto
        {
            Success = true,
            Message = "Address added successfully"
        };
        return result;
    }

    public async Task<ResultDto> DeleteAddressAsync(int index)
    {
        if (await GetProfileAsync() is not { } profile)
        {
            return new ResultDto { Success = false, Message = "Sign in to delete an address" };
        }

        var addressToRemove = profile.Addresses.FirstOrDefault(a => a.Index == index);
        if (addressToRemove != null)
        {
            profile.Addresses.Remove(addressToRemove);
            var result = new ResultDto
            {
                Success = true,
                Message = "Address deleted successfully"
            };
            return result;
        }
        else
        {
            var result = new ResultDto
            {
                Success = false,
                Message = "Address not found"
            };
            return result;
        }
    }

    public async Task<ResultDto> SetAsPrimaryAsync(int index)
    {
        if (await GetProfileAsync() is not { } profile)
        {
            return new ResultDto { Success = false, Message = "Sign in to set a primary address" };
        }

        foreach (var address in profile.Addresses)
        {
            address.IsDefault = address.Index == index;
        }

        var result = new ResultDto
        {
            Success = true,
            Message = "Primary address set successfully"
        };
        return result;
    }

    public async Task<AddressBookItemDto?> GetPrimaryAddressAsync()
    {
        if (await GetProfileAsync() is not { } profile) return null;
        var primaryAddress = profile.Addresses.FirstOrDefault(a => a.IsDefault);
        return primaryAddress;
    }

    public Task<ICollection<OrderListItemDto>> GetOrdersAsync(int page, int pageSize)
    {
        var orders = StaticData.StaticOrders.Orders
         .Skip((page - 1) * pageSize)
         .Take(pageSize)
         .ToList();
        return Task.FromResult((ICollection<OrderListItemDto>)orders);
    }

    public Task<int> GetTotalOrdersCountAsync() => Task.FromResult(StaticData.StaticOrders.Orders.Count);

    public Task<OrderDto?> GetOrderByIdAsync(Guid orderId)
    {
        var order = StaticData.StaticOrders.OrdersE.FirstOrDefault(o => o.Id == orderId);
        if (order is not null)
        {
            return Task.FromResult(order);
        }
        return Task.FromResult<OrderDto?>(null);
    }

    public Task<int> GetOrderCountInNeedOfAttentionAsync()
    {
        var orders = StaticData.StaticOrders.OrdersE;
        var count = orders.Count(o => o.Status.StatusId <= 1);
        return Task.FromResult(count);
    }

    public Task<TrackingDto?> GetTrackingAsync(Guid orderId)
    {
        var order = StaticData.StaticOrders.OrdersE.FirstOrDefault(o => o.Id == orderId);
        if (order is not null)
        {
            return Task.FromResult(order.TrackingInformation);
        }
        return Task.FromResult<TrackingDto?>(null);
    }

    public Task SubmitReviewAsync(Guid productId, ReviewSubmitRequest review)
    {
        // In a real implementation, you would save the review to a database.
        // For this fake service, we'll just simulate the delay and do nothing.
        return Task.Delay(1000);
    }

    public Task<(bool, int)> HasAlreadyReviewedProductIdAsync(Guid productId)
    {
        return Task.FromResult((false, 0));
    }

    /// <summary>
    /// Birthday check is now handled by CustomerProfileService.
    /// Delegates to avoid double-loading; returns false as safe default.
    /// </summary>
    public Task<bool> HasBirthdateAsync() => Task.FromResult(false);
}
