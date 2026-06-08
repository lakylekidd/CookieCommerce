namespace CookieCommerce.AuthDomain.Application.Keycloak;

public interface IKeycloakTokenService
{
    Task<KeycloakTokenResponse> LoginAsync(string email, string password, CancellationToken ct = default);
    Task RevokeTokenAsync(string refreshToken, CancellationToken ct = default);
}

public class KeycloakTokenResponse
{
    public string AccessToken { get; set; } = default!;
    public string RefreshToken { get; set; } = default!;
    public int ExpiresIn { get; set; }
}
