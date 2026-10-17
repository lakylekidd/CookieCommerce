using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using CookieCommerce.Web.Models.Auth;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using CookieCommerce.Web.Services.Commerce;

namespace CookieCommerce.Web.Services;

public class KeycloakAuthService(
    HttpClient httpClient,
    ICartService cartService,
    ProtectedLocalStorage localStorage, ILogger<KeycloakAuthService>? logger = null) : IAuthService
{
    private const string TokenKey = "auth_access_token";
    private const string RefreshTokenKey = "auth_refresh_token";

    private UserDto? _currentUser;
    private string? _refreshToken;
    private string? _accessToken;
    private bool _initialized;
    private readonly SemaphoreSlim initializeLock = new(1);
    private bool _prerendering;

    public event Action? OnChange;
    public UserDto? CurrentUser => _currentUser;
    public bool IsLoggedIn => _currentUser != null;
    public bool IsAdmin => string.Equals(_currentUser?.Role, "Admin", StringComparison.OrdinalIgnoreCase);

    private void NotifyStateChanged() => OnChange?.Invoke();

    public async Task InitializeAsync()
    {
        if (_initialized && !_prerendering && (_accessToken is null || ParseJwtToUserDto(_accessToken) is not null)) return;
        await initializeLock.WaitAsync();
        try
        {
            if (_initialized && !_prerendering && (_accessToken is null || ParseJwtToUserDto(_accessToken) is not null)) return;
            await InitializeCoreAsync();
        }
        finally { initializeLock.Release(); }
    }

    private async Task InitializeCoreAsync()
    {
        _prerendering = false;
        try
        {
            var tokenResult = await localStorage.GetAsync<string>(TokenKey);
            if (tokenResult.Success && !string.IsNullOrEmpty(tokenResult.Value))
            {
                _accessToken = tokenResult.Value;
                var user = ParseJwtToUserDto(tokenResult.Value);
                var refreshResult = await localStorage.GetAsync<string>(RefreshTokenKey);
                if (refreshResult.Success) _refreshToken = refreshResult.Value;
                if (user is null && !string.IsNullOrEmpty(_refreshToken))
                {
                    using var response = await httpClient.PostAsJsonAsync("api/auth/refresh", new { refreshToken = _refreshToken });
                    if (response.IsSuccessStatusCode)
                    {
                        var refreshed = await response.Content.ReadFromJsonAsync<RefreshedToken>();
                        if (refreshed is not null)
                        {
                            user = ParseJwtToUserDto(refreshed.AccessToken);
                            _accessToken = refreshed.AccessToken;
                            _refreshToken = refreshed.RefreshToken;
                            await localStorage.SetAsync(TokenKey, refreshed.AccessToken);
                            await localStorage.SetAsync(RefreshTokenKey, refreshed.RefreshToken);
                        }
                    }
                }
                if (user != null)
                {
                    _currentUser = user;
                    await cartService.ConvertToUserCartAsync(user.Id);
                    NotifyStateChanged();
                }
                else
                {
                    _currentUser = null;
                    _accessToken = null;
                    _refreshToken = null;
                    _initialized = true;
                    await localStorage.DeleteAsync(TokenKey);
                    await localStorage.DeleteAsync(RefreshTokenKey);
                    NotifyStateChanged();
                }
            }
            _initialized = true;
        }
        catch (InvalidOperationException)
        {
            // Prerendering blocks JS interop; return anonymous now,
            // but allow re-init once the circuit is live.
            _prerendering = true;
            _initialized = true;
        }
        catch (OperationCanceledException)
        {
            // Circuit was canceled; same treatment.
            _prerendering = true;
            _initialized = true;
        }
        catch (HttpRequestException)
        {
            _currentUser = null;
            _accessToken = null;
            _initialized = true;
            NotifyStateChanged();
        }
        catch (System.Security.Cryptography.CryptographicException)
        {
            _currentUser = null;
            _accessToken = null;
            _initialized = true;
            await localStorage.DeleteAsync(TokenKey);
            await localStorage.DeleteAsync(RefreshTokenKey);
            NotifyStateChanged();
        }
    }

    public async Task<bool> SignInAsync(string email, string password)
    {
        await initializeLock.WaitAsync();
        try
        {
            var response = await httpClient.PostAsJsonAsync("api/auth/login", new { email, password });
            if (!response.IsSuccessStatusCode)
            {
                logger?.LogWarning("Authentication API rejected sign-in with HTTP {StatusCode}", (int)response.StatusCode);
                return false;
            }

            var result = await response.Content.ReadFromJsonAsync<LoginApiResponse>(
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (result == null) return false;

            _currentUser = ParseJwtToUserDto(result.AccessToken) ?? new UserDto
            {
                Id = result.UserId,
                Email = result.Email,
                FirstName = result.FirstName,
                LastName = result.LastName,
                Role = result.Role ?? "User"
            };

            if (string.Equals(result.Role, "Admin", StringComparison.OrdinalIgnoreCase))
                _currentUser.Role = "Admin";
            _refreshToken = result.RefreshToken;
            _accessToken = result.AccessToken;
            await localStorage.SetAsync(TokenKey, result.AccessToken);
            await localStorage.SetAsync(RefreshTokenKey, result.RefreshToken);
            await cartService.ConvertToUserCartAsync(_currentUser.Id);
            _initialized = true;
            NotifyStateChanged();
            return true;
        }
        catch (Exception ex)
        {
            logger?.LogWarning("Customer sign-in session initialization failed ({FailureType})", ex.GetType().Name);
            return false;
        }
        finally { initializeLock.Release(); }
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

    public void Logout() => _ = LogoutAsync();

    public async Task LogoutAsync()
    {
        string? token;
        await initializeLock.WaitAsync();
        try
        {
            token = _refreshToken;
            _currentUser = null;
            _refreshToken = null;
            _accessToken = null;
            _initialized = true;
            await localStorage.DeleteAsync(TokenKey);
            await localStorage.DeleteAsync(RefreshTokenKey);
            if (cartService is CartService persistent) await persistent.ResetIdentityAsync();
            NotifyStateChanged();
        }
        finally { initializeLock.Release(); }

        if (!string.IsNullOrEmpty(token))
        {
            try
            {
                using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                using var response = await httpClient.PostAsJsonAsync("api/auth/logout", new { refreshToken = token }, cancellation.Token);
            }
            catch (Exception ex) when (ex is HttpRequestException or OperationCanceledException) { /* Local credentials have already been removed. */ }
        }
    }

    public async Task SendForgotPasswordLinkAsync(string email)
    {
        using var response = await httpClient.PostAsJsonAsync("api/auth/forgot-password", new { email });
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException("Unable to send a password reset email. Please try again shortly.");
    }

    public async Task<string?> GetAccessTokenAsync()
    {
        await InitializeAsync();
        return _accessToken;
    }

    public Task<AuthResult> UpdatePasswordAsync(string token, string newPassword)
        => Task.FromResult(AuthResult.Failed("Use the secure password reset link in your email. Passwords are updated on our sign-in provider's page."));

    private static UserDto? ParseJwtToUserDto(string token)
    {
        try
        {
            var parts = token.Split('.');
            if (parts.Length != 3) return null;

            var json = Encoding.UTF8.GetString(Base64UrlDecode(parts[1]));
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            if (root.TryGetProperty("exp", out var expires) && expires.GetInt64() <= DateTimeOffset.UtcNow.ToUnixTimeSeconds()) return null;

            var email = root.TryGetProperty("email", out var emailEl) ? emailEl.GetString() ?? "" : "";
            var firstName = root.TryGetProperty("given_name", out var fn) ? fn.GetString() ?? "" : "";
            var lastName = root.TryGetProperty("family_name", out var ln) ? ln.GetString() ?? "" : "";
            var sub = root.TryGetProperty("sub", out var subEl) ? subEl.GetString() ?? "" : "";
            if (root.TryGetProperty("customer_id", out var customerId) && customerId.ValueKind == JsonValueKind.String)
                sub = customerId.GetString() ?? sub;

            var role = HasRole(root, "admin") ? "Admin" : "User";

            return new UserDto
            {
                Id = Guid.TryParse(sub, out var guid) ? guid : throw new FormatException("Invalid user ID."),
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

    private static bool HasRole(JsonElement root, string roleName)
    {
        if (root.TryGetProperty("realm_access", out var realmAccess) &&
            realmAccess.TryGetProperty("roles", out var realmRoles) &&
            ContainsRole(realmRoles, roleName))
        {
            return true;
        }

        if (root.TryGetProperty("resource_access", out var resourceAccess))
        {
            var clientId = root.TryGetProperty("azp", out var party) ? party.GetString() ?? "cookiecommerce-backend" : "cookiecommerce-backend";
            if (resourceAccess.TryGetProperty(clientId, out var clientAccess) && clientAccess.TryGetProperty("roles", out var clientRoles)
                && ContainsRole(clientRoles, roleName)) return true;
        }

        return false;
    }

    private static bool ContainsRole(JsonElement roles, string roleName)
    {
        if (roles.ValueKind != JsonValueKind.Array) return false;

        foreach (var role in roles.EnumerateArray())
        {
            if (string.Equals(role.GetString(), roleName, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    private static byte[] Base64UrlDecode(string value)
    {
        var base64 = value.Replace('-', '+').Replace('_', '/');
        var padding = (4 - base64.Length % 4) % 4;
        base64 += new string('=', padding);
        return Convert.FromBase64String(base64);
    }

    private sealed record LoginApiResponse(
        Guid UserId, string Email, string FirstName, string LastName,
        string AccessToken, string RefreshToken, int ExpiresIn, string? Role);
    private sealed record RefreshedToken(string AccessToken, string RefreshToken, int ExpiresIn);
}
