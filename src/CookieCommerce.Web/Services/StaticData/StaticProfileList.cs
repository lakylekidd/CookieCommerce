using CookieCommerce.Web.Models.Account;
using CookieCommerce.Web.Models.Auth;
using CookieCommerce.Web.Services.Products;

namespace CookieCommerce.Web.Services.StaticData;

/// <summary>
/// Static profile data used only for non-customer-facing surfaces (backoffice, development tools).
/// Customer-facing profile load/save/display MUST use CustomerProfileService.
/// Do not reference this class from components or services that serve the customer dashboard.
/// </summary>
public static class StaticProfileList
{
    public static readonly UserProfileDto AdminUserProfile = GetAdminProfile();
    public static readonly UserProfileDto RegularUserProfile = GetRegularUserProfile();

    public static readonly List<UserProfileDto> Profiles =
    [
        AdminUserProfile,
        RegularUserProfile
    ];

    private static UserProfileDto GetAdminProfile()
    {
        var profile = StaticUserList.AdminUser.GetProfile("male");
        profile.Addresses =
            [
                StaticAddressList.Address001.ToAddressBookItem(0, "Home Base", "123-456-7890", profile.FirstName, profile.LastName, "home", true),
                StaticAddressList.Address002.ToAddressBookItem(1, "Work", "098-765-4321", profile.FirstName, profile.LastName, "work")
            ];
        profile.FavoriteProducts = 
            [
                StaticStorefrontProductList.CookieRedVelve.Id, 
                StaticStorefrontProductList.CookieChocolateChunk.Id
            ];
        return profile;
    }

    private static UserProfileDto GetRegularUserProfile()
    {
        var profile = StaticUserList.RegularUser.GetProfile("male");
        profile.Addresses =
            [
                StaticAddressList.Address003.ToAddressBookItem(0, "Tiger Home", "123-456-7890", profile.FirstName, profile.LastName, "home", true),
                StaticAddressList.Address004.ToAddressBookItem(1, "Beach House", "098-765-4321", profile.FirstName, profile.LastName, "luggage")
            ];
        profile.FavoriteProducts =
            [
                StaticStorefrontProductList.CookieCocoaWhiteChocolateChip.Id,
                StaticStorefrontProductList.CookieHazelnutPraline.Id,
                StaticStorefrontProductList.CookieMatchaWhiteChocolate.Id
            ];
        return profile;
    }

    private static UserProfileDto GetProfile(this UserDto fromUser, string gender, int age = 30)
    {
        return new UserProfileDto
        {
            Id = fromUser.Id,
            FirstName = fromUser.FirstName,
            LastName = fromUser.LastName,
            Email = fromUser.Email,
            DateOfBirth = DateOnly.FromDateTime(DateTime.Now.AddYears(-age)),
            Gender = gender
        };
    }
}
