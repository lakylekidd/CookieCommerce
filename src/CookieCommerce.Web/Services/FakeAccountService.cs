using CookieCommerce.Web.Models;
using CookieCommerce.Web.Models.Account;
using CookieCommerce.Web.Models.Addresses;
using CookieCommerce.Web.Models.Order;
using CookieCommerce.Web.Models.Products;
using CookieCommerce.Web.Models.Reviews;
using CookieCommerce.Web.Services.Products;

namespace CookieCommerce.Web.Services;

public class FakeAccountService() : IAccountService
{
    private List<Guid> _favoriteProductIds = 
        [
            Guid.Parse("d8f5a8e1-1234-4567-89ab-444444444444"),
            Guid.Parse("d8f5a8e1-1234-4567-89ab-222222222222"),
        ];

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

    public Task<ICollection<StorefrontProductDto>> GetFavoriteProductsAsync()
    {
        var favoriteProducts = StaticStorefrontProductList.Cookies
            .Where(p => _favoriteProductIds.Contains(p.Id))
            .ToList();
        return Task.FromResult((ICollection<StorefrontProductDto>)favoriteProducts);
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

    public Task<IList<AddressBookItemDto>> GetAddressesAsync()
    {
        var addresses = StaticData.StaticAddressList.Addresses;
        return Task.FromResult((IList<AddressBookItemDto>)addresses);
    }

    public Task<ResultDto> AddAddressAsync(AddressBookItemDto address)
    {
        var addresses = StaticData.StaticAddressList.Addresses;
        address.Index = addresses.Count > 0 ? addresses.Max(a => a.Index) + 1 : 1;
        addresses.Add(address);

        foreach (var a in addresses)
        {
            a.IsDefault = a.Index == address.Index;
        }

        var result = new ResultDto
        {
            Success = true,
            Message = "Address added successfully"
        };
        return Task.FromResult(result);
    }

    public Task<ResultDto> DeleteAddressAsync(int index)
    {
        var addresses = StaticData.StaticAddressList.Addresses;
        var addressToRemove = addresses.FirstOrDefault(a => a.Index == index);
        if (addressToRemove != null)
        {
            addresses.Remove(addressToRemove);
            var result = new ResultDto
            {
                Success = true,
                Message = "Address deleted successfully"
            };
            return Task.FromResult(result);
        }
        else
        {
            var result = new ResultDto
            {
                Success = false,
                Message = "Address not found"
            };
            return Task.FromResult(result);
        }
    }

    public Task<ResultDto> SetAsPrimaryAsync(int index)
    {
        var addresses = StaticData.StaticAddressList.Addresses;
        foreach (var address in addresses)
        {
            address.IsDefault = address.Index == index;
        }

        var result = new ResultDto
        {
            Success = true,
            Message = "Primary address set successfully"
        };
        return Task.FromResult(result);
    }

    public Task<AddressBookItemDto>? GetPrimaryAddressAsync()
    {
        var addresses = StaticData.StaticAddressList.Addresses;
        var primaryAddress = addresses.FirstOrDefault(a => a.IsDefault);
        if (primaryAddress is not null)
            return Task.FromResult(primaryAddress);
        return Task.FromResult<AddressBookItemDto?>(null);
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

    public Task<OrderDto> GetOrderByIdAsync(Guid orderId)
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

    public Task<TrackingDto> GetTrackingAsync(Guid orderId)
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
}
