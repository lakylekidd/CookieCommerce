using CookieCommerce.Web.Models.Account;

namespace CookieCommerce.Web.Services;

/// <summary>
/// Scoped shared state that holds the live customer profile and notifies
/// components to re-render immediately after a successful save — without a
/// manual page reload.
/// </summary>
public sealed class CustomerProfileState
{
    private UserProfileDto? _profile;

    /// <summary>The current live profile (null until first load).</summary>
    public UserProfileDto? Profile => _profile;

    /// <summary>Raised whenever the profile is updated so dependent components can refresh.</summary>
    public event Action? OnProfileChanged;

    /// <summary>Store an updated profile and notify subscribers.</summary>
    public void SetProfile(UserProfileDto profile)
    {
        _profile = profile;
        OnProfileChanged?.Invoke();
    }

    /// <summary>Clear cached profile (e.g. on sign-out).</summary>
    public void Clear()
    {
        _profile = null;
        OnProfileChanged?.Invoke();
    }
}
