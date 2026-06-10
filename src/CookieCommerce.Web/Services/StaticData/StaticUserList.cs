using CookieCommerce.Web.Models.Auth;

namespace CookieCommerce.Web.Services.StaticData;

public static class StaticUserList
{
    public static readonly UserDto AdminUser = new()
    {
        Role = "Admin",
        Email = "admin@cookie.com",
        Password = "123",
        FirstName = "Tiger",
        LastName = "van Cookie"
    };
    public static readonly UserDto RegularUser = new()
    {
        Email = "customer@cookie.com",
        Password = "123",
        FirstName = "Cookie",
        LastName = "Lover"
    };

    public static readonly List<UserDto> Users =
    [
        AdminUser,
        RegularUser
    ];
}
