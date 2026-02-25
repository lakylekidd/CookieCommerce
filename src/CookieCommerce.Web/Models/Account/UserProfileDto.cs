using CookieCommerce.Web.Models.Auth;

namespace CookieCommerce.Web.Models.Account;

public class UserProfileDto : UserDto
{
    public string Gender { get; set; } = default!;
    public DateTime? DateOfBirth { get; set; }
}
