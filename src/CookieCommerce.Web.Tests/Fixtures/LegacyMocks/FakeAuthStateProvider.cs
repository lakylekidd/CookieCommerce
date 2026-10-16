using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace CookieCommerce.Web.Services;

public class FakeAuthStateProvider(IAuthService authService) : AuthenticationStateProvider
{
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        // First, ensure the service has loaded data from LocalStorage
        if (authService.CurrentUser == null) await authService.InitializeAsync();

        var identity = new ClaimsIdentity();

        if (authService.CurrentUser is { } user)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, user.FirstName),
                new(ClaimTypes.Surname, user.LastName),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Role, user.Role ?? "User") // Assign the role!
            };
            identity = new ClaimsIdentity(claims, "FakeAuth");
        }

        return new AuthenticationState(new ClaimsPrincipal(identity));
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
