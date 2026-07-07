using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using CookieCommerce.Web.Models.Auth;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace CookieCommerce.Web.Services;

public class KeycloakAuthService(
    HttpClient httpClient,
    ICartService cartService,
    ProtectedLocalStorage localStorage) : IAuthService
{
    private const string TokenKey = "auth_access_token";
    private const string RefreshTokenKey = "auth_refresh_token";

    private UserDto? _currentUser;
    private string? _refreshToken;
    private bool _initialized;

    public event Action? OnChange;
    public UserDto? CurrentUser => _currentUser;
    public bool IsLoggedIn => _currentUser != null;
    public bool IsAdmin => _currentUser?.Role == "Admin";

    private void NotifyStateChanged() => OnChange?.Invoke();

    public async Task InitializeAsync()
    {
        if (_initialized) return;
        try
        {
            var tokenResult = await localStorage.GetAsync<string>(TokenKey);
            if (tokenResult.Success && !string.IsNullOrEmpty(tokenResult.Value))
            {
                var user = ParseJwtToUserDto(tokenResult.Value);
                if (user != null)
                {
                    _currentUser = user;
                    var refreshResult = await localStorage.GetAsync<string>(RefreshTokenKey);
                    if (refreshResult.Success)
                        _refreshToken = refreshResult.Value;
                    NotifyStateChanged();
                }
            }
            _initialized = true;
        }
        catch (InvalidOperationException)
        {
            // Prerendering blocks JS interop; try again after app becomes interactive.
        }
        catch (OperationCanceledException)
        {
            // Circuit was canceled (e.g., during navigation or disconnect); ignore gracefully.
        }
    }

    public async Task<bool> SignInAsync(string email, string password)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync("api/auth/login", new { email, password });
            if (!response.IsSuccessStatusCode) return false;

            var result = await response.Content.ReadFromJsonAsync<LoginApiResponse>(
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (result == null) return false;

            _currentUser = ParseJwtToUserDto(result.AccessToken) ?? new UserDto
            {
                Id = result.UserId,
                Email = result.Email,
                FirstName = result.FirstName,
                LastName = result.LastName,
                Role = "User"
            };
            _refreshToken = result.RefreshToken;
            await localStorage.SetAsync(TokenKey, result.AccessToken);
            await localStorage.SetAsync(RefreshTokenKey, result.RefreshToken);
            await cartService.ConvertToUserCartAsync(_currentUser.Id);
            NotifyStateChanged();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<AuthResult> SignUpAsync(SignupDto signupForm)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync("api/auth/register", new
            {
                signupForm.Email,
                signupForm.Password,
                signupForm.FirstName,
                signupForm.LastName
            });

            if (response.IsSuccessStatusCode)
                return AuthResult.Succeeded();

            var errorBody = await response.Content.ReadAsStringAsync();
            return AuthResult.Failed(!string.IsNullOrEmpty(errorBody) ? errorBody : "Registration failed.");
        }
        catch (Exception ex)
        {
            return AuthResult.Failed(ex.Message);
        }
    }

    public void UpdateCurrentUserProfile(string firstName, string lastName, string email)
    {
        if (_currentUser is null) return;

        _currentUser.FirstName = firstName;
        _currentUser.LastName = lastName;
        _currentUser.Email = email;
        NotifyStateChanged();
    }

    public void Logout()
    {
        var token = _refreshToken;
        _currentUser = null;
        _refreshToken = null;
        _initialized = true;
        _ = localStorage.DeleteAsync(TokenKey);
        _ = localStorage.DeleteAsync(RefreshTokenKey);

        if (!string.IsNullOrEmpty(token))
            _ = httpClient.PostAsJsonAsync("api/auth/logout", new { refreshToken = token });

        NotifyStateChanged();
    }

    public Task SendForgotPasswordLinkAsync(string email) => Task.CompletedTask;

    public Task<AuthResult> UpdatePasswordAsync(string token, string newPassword)
        => Task.FromResult(AuthResult.Succeeded());

    private static UserDto? ParseJwtToUserDto(string token)
    {
        try
        {
            var parts = token.Split('.');
            if (parts.Length != 3) return null;

            var payload = parts[1];
            var padding = (4 - payload.Length % 4) % 4;
            payload = payload + new string('=', padding);

            var json = Encoding.UTF8.GetString(Convert.FromBase64String(payload));
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var email = root.TryGetProperty("email", out var emailEl) ? emailEl.GetString() ?? "" : "";
            var firstName = root.TryGetProperty("given_name", out var fn) ? fn.GetString() ?? "" : "";
            var lastName = root.TryGetProperty("family_name", out var ln) ? ln.GetString() ?? "" : "";
            var sub = root.TryGetProperty("sub", out var subEl) ? subEl.GetString() ?? "" : "";

            var role = "User";
            if (root.TryGetProperty("realm_access", out var ra) && ra.TryGetProperty("roles", out var roles))
            {
                foreach (var r in roles.EnumerateArray())
                {
                    if (string.Equals(r.GetString(), "admin", StringComparison.OrdinalIgnoreCase))
                    {
                        role = "Admin";
                        break;
                    }
                }
            }

            return new UserDto
            {
                Id = Guid.TryParse(sub, out var guid) ? guid : Guid.NewGuid(),
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                Role = role
            };
        }
        catch
        {
            return null;
        }
    }

    private sealed record LoginApiResponse(
        Guid UserId, string Email, string FirstName, string LastName,
        string AccessToken, string RefreshToken, int ExpiresIn);
}
