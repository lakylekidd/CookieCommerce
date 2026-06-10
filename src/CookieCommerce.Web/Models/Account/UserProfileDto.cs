using CookieCommerce.Web.Models.Addresses;
using CookieCommerce.Web.Models.Auth;

namespace CookieCommerce.Web.Models.Account;

public class UserProfileDto : UserDto
{
    public string Gender { get; set; } = default!;
    public DateTime? DateOfBirth { get; set; }
    public ICollection<Guid> FavoriteProducts { get; set; } = [];
    public ICollection<AddressBookItemDto> Addresses { get; set; } = [];
}
