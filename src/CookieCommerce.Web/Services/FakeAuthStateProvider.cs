using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace CookieCommerce.Web.Services;

public class FakeAuthStateProvider(IAuthService authService) : AuthenticationStateProvider
{
    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var identity = new ClaimsIdentity();

        if (authService.IsLoggedIn && authService.CurrentUser != null)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, authService.CurrentUser.Email),
                new(ClaimTypes.Email, authService.CurrentUser.Email),
                new(ClaimTypes.Role, authService.CurrentUser.Role ?? "User") // Assign the role!
            };
            identity = new ClaimsIdentity(claims, "FakeAuth");
        }

        return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity)));
    }

    public void NotifyUserAuthentication(string email, string role)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, email),
            new(ClaimTypes.Role, role)
        };
        var identity = new ClaimsIdentity(claims, "FakeAuth");
        var user = new ClaimsPrincipal(identity);

        // This is the magic line that tells Blazor the user changed!
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
    }

    // This is crucial: Call this when your FakeAuthService logs someone in!
    public void NotifyUserChanged() => NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
}
