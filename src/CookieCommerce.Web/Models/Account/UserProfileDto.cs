using CookieCommerce.Web.Models.Addresses;
using CookieCommerce.Web.Models.Auth;

namespace CookieCommerce.Web.Models.Account;

public class UserProfileDto : UserDto
{
    /// <summary>Editable gender (Male, Female, Prefer not to disclose).</summary>
    public string? Gender { get; set; }

    /// <summary>Editable date of birth.</summary>
    public DateOnly? DateOfBirth { get; set; }

    /// <summary>
    /// Current verified email — remains the profile/login email while a pending email exists.
    /// </summary>
    public new string Email { get; set; } = string.Empty;

    /// <summary>Pending unverified email; null when no email change is in progress.</summary>
    public string? PendingEmail { get; set; }

    /// <summary>Verified or PendingVerification.</summary>
    public string EmailVerificationStatus { get; set; } = "Verified";

    public ICollection<Guid> FavoriteProducts { get; set; } = [];
    public ICollection<AddressBookItemDto> Addresses { get; set; } = [];
}
