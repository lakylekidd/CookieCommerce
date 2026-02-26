using CookieCommerce.Web.Models;
using CookieCommerce.Web.Models.Account;
using CookieCommerce.Web.Models.Addresses;
using System;

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
}
