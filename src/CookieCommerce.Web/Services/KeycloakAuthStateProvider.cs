using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace CookieCommerce.Web.Services;

public class KeycloakAuthStateProvider : AuthenticationStateProvider, IDisposable
{
    private readonly IAuthService _authService;

    public KeycloakAuthStateProvider(IAuthService authService)
    {
        _authService = authService;
        _authService.OnChange += NotifyUserChanged;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (_authService.CurrentUser == null)
            await _authService.InitializeAsync();

        if (_authService.CurrentUser is { } user)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.FirstName),
                new(ClaimTypes.Surname, user.LastName),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Role, user.Role ?? "User")
            };
            var identity = new ClaimsIdentity(claims, "Keycloak");
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }

        return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
    }

    public void NotifyUserChanged() =>
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

    public void Dispose() => _authService.OnChange -= NotifyUserChanged;
}
