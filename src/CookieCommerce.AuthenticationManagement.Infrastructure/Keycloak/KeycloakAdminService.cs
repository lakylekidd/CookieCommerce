using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using CookieCommerce.AuthenticationManagement.Application.Keycloak;
using Microsoft.Extensions.Options;

namespace CookieCommerce.AuthenticationManagement.Infrastructure.Keycloak;

public class KeycloakAdminService : IKeycloakAdminService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly KeycloakOptions _options;

    public KeycloakAdminService(IHttpClientFactory httpClientFactory, IOptions<KeycloakOptions> options)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
    }

    private async Task<string> GetAdminTokenAsync(CancellationToken ct = default)
    {
        var client = _httpClientFactory.CreateClient("keycloak");

        var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["client_id"] = "admin-cli",
            ["username"] = _options.AdminUser,
            ["password"] = _options.AdminPassword
        });

        var response = await client.PostAsync(
            $"/realms/master/protocol/openid-connect/token",
            form,
            ct);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<AdminTokenResponse>(
            cancellationToken: ct);

        return result!.AccessToken;
    }

    public async Task<string> RegisterUserAsync(
        string email,
        string password,
        string firstName,
        string lastName,
        CancellationToken ct = default)
    {
        var adminToken = await GetAdminTokenAsync(ct);
        var client = _httpClientFactory.CreateClient("keycloak");
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var userPayload = new
        {
            username = email,
            email = email,
            firstName = firstName,
            lastName = lastName,
            enabled = true,
            emailVerified = true,
            credentials = new[]
            {
                new { type = "password", value = password, temporary = false }
            }
        };

        var response = await client.PostAsJsonAsync(
            $"/admin/realms/{_options.Realm}/users",
            userPayload,
            ct);

        response.EnsureSuccessStatusCode();

        // Parse the userId from the Location header
        var location = response.Headers.Location
            ?? throw new InvalidOperationException("Keycloak did not return a Location header after user registration.");

        var userId = location.Segments.Last().TrimEnd('/');
        return userId;
    }

    public async Task<KeycloakUserInfo?> GetUserByEmailAsync(string email, CancellationToken ct = default)
    {
        var adminToken = await GetAdminTokenAsync(ct);
        var client = _httpClientFactory.CreateClient("keycloak");
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var response = await client.GetAsync(
            $"/admin/realms/{_options.Realm}/users?email={Uri.EscapeDataString(email)}&exact=true",
            ct);

        response.EnsureSuccessStatusCode();

        var users = await response.Content.ReadFromJsonAsync<List<KeycloakUserResponse>>(
            cancellationToken: ct);

        if (users is null || users.Count == 0)
            return null;

        var user = users[0];
        return new KeycloakUserInfo
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName
        };
    }

    private sealed class AdminTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = default!;
    }

    private sealed class KeycloakUserResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = default!;

        [JsonPropertyName("email")]
        public string Email { get; set; } = default!;

        [JsonPropertyName("firstName")]
        public string FirstName { get; set; } = default!;

        [JsonPropertyName("lastName")]
        public string LastName { get; set; } = default!;
    }
}
