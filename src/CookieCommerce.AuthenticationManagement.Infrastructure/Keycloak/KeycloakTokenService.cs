using System.Net.Http.Json;
using System.Text.Json.Serialization;
using CookieCommerce.AuthenticationManagement.Application.Keycloak;
using CookieCommerce.AuthenticationManagement.Domain.Exceptions;
using Microsoft.Extensions.Options;

namespace CookieCommerce.AuthenticationManagement.Infrastructure.Keycloak;

public class KeycloakTokenService : IKeycloakTokenService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly KeycloakOptions _options;

    public KeycloakTokenService(IHttpClientFactory httpClientFactory, IOptions<KeycloakOptions> options)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
    }

    public async Task<KeycloakTokenResponse> LoginAsync(string email, string password, CancellationToken ct = default)
    {
        var client = _httpClientFactory.CreateClient("keycloak");

        var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["client_id"] = _options.ClientId,
            ["client_secret"] = _options.ClientSecret,
            ["username"] = email,
            ["password"] = password,
            ["scope"] = "openid"
        });

        var response = await client.PostAsync(
            $"/realms/{_options.Realm}/protocol/openid-connect/token",
            form,
            ct);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ct);
            throw new InvalidCredentialsException($"Keycloak {(int)response.StatusCode}: {error}");
        }

        var result = await response.Content.ReadFromJsonAsync<KeycloakLoginResponse>(
            cancellationToken: ct);

        return new KeycloakTokenResponse
        {
            AccessToken = result!.AccessToken,
            RefreshToken = result.RefreshToken,
            ExpiresIn = result.ExpiresIn
        };
    }

    public async Task RevokeTokenAsync(string refreshToken, CancellationToken ct = default)
    {
        var client = _httpClientFactory.CreateClient("keycloak");

        var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["client_id"] = _options.ClientId,
            ["client_secret"] = _options.ClientSecret,
            ["token"] = refreshToken,
            ["token_type_hint"] = "refresh_token"
        });

        // Best-effort: ignore non-success responses
        await client.PostAsync(
            $"/realms/{_options.Realm}/protocol/openid-connect/revoke",
            form,
            ct);
    }

    private sealed class KeycloakLoginResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = default!;

        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; } = default!;

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
    }
}
