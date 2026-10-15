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

    public async Task SendPasswordResetAsync(string keycloakId, CancellationToken ct = default)
    {
        var client = _httpClientFactory.CreateClient("keycloak");
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", await GetAdminTokenAsync(ct));
        using var response = await client.PutAsJsonAsync(
            $"/admin/realms/{_options.Realm}/users/{Uri.EscapeDataString(keycloakId)}/execute-actions-email?lifespan=1800",
            new[] { "UPDATE_PASSWORD" }, ct);
        response.EnsureSuccessStatusCode();
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
            attributes = new Dictionary<string, string[]>(),
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

    public async Task AssignRealmRoleAsync(string keycloakId, string roleName, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(roleName)) return;

        var adminToken = await GetAdminTokenAsync(ct);
        var client = _httpClientFactory.CreateClient("keycloak");
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var role = await EnsureRealmRoleAsync(client, roleName, ct);
        var response = await client.PostAsJsonAsync(
            $"/admin/realms/{_options.Realm}/users/{Uri.EscapeDataString(keycloakId)}/role-mappings/realm",
            new[] { role },
            ct);

        response.EnsureSuccessStatusCode();
    }

    public async Task<IReadOnlyList<string>> GetRealmRolesAsync(string keycloakId, CancellationToken ct = default)
    {
        var adminToken = await GetAdminTokenAsync(ct);
        var client = _httpClientFactory.CreateClient("keycloak");
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var response = await client.GetAsync(
            $"/admin/realms/{_options.Realm}/users/{Uri.EscapeDataString(keycloakId)}/role-mappings/realm",
            ct);

        response.EnsureSuccessStatusCode();

        var roles = await response.Content.ReadFromJsonAsync<List<KeycloakRoleResponse>>(cancellationToken: ct);
        return roles?.Select(role => role.Name).ToArray() ?? [];
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

    public async Task UpdateUserEmailAsync(string keycloakId, string verifiedEmail, CancellationToken ct = default)
    {
        var adminToken = await GetAdminTokenAsync(ct);
        var client = _httpClientFactory.CreateClient("keycloak");
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var payload = new
        {
            username = verifiedEmail,
            email = verifiedEmail,
            emailVerified = true,
            enabled = true
        };

        var response = await client.PutAsJsonAsync(
            $"/admin/realms/{_options.Realm}/users/{Uri.EscapeDataString(keycloakId)}",
            payload,
            ct);

        response.EnsureSuccessStatusCode();
    }

    public async Task UpdateUserProfileAsync(
        string keycloakId,
        string firstName,
        string lastName,
        string? gender,
        DateOnly? dateOfBirth,
        CancellationToken ct = default)
    {
        var adminToken = await GetAdminTokenAsync(ct);
        var client = _httpClientFactory.CreateClient("keycloak");
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var existing = await GetUserByIdAsync(client, keycloakId, ct);
        var attributes = existing?.Attributes is null
            ? new Dictionary<string, string[]>()
            : new Dictionary<string, string[]>(existing.Attributes);

        UpsertOrRemoveAttribute(attributes, "gender", gender);
        UpsertOrRemoveAttribute(attributes, "dateOfBirth", dateOfBirth?.ToString("yyyy-MM-dd"));

        var payload = new
        {
            username = existing?.Username,
            email = existing?.Email,
            emailVerified = existing?.EmailVerified,
            firstName,
            lastName,
            enabled = existing?.Enabled ?? true,
            attributes
        };

        var response = await client.PutAsJsonAsync(
            $"/admin/realms/{_options.Realm}/users/{Uri.EscapeDataString(keycloakId)}",
            payload,
            ct);

        response.EnsureSuccessStatusCode();
    }

    public async Task DisableUserAsync(string keycloakId, CancellationToken ct = default)
    {
        var adminToken = await GetAdminTokenAsync(ct);
        var client = _httpClientFactory.CreateClient("keycloak");
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var payload = new { enabled = false };
        var response = await client.PutAsJsonAsync(
            $"/admin/realms/{_options.Realm}/users/{Uri.EscapeDataString(keycloakId)}",
            payload,
            ct);

        response.EnsureSuccessStatusCode();
    }

    private async Task<KeycloakRoleResponse> EnsureRealmRoleAsync(HttpClient client, string roleName, CancellationToken ct)
    {
        var escapedRoleName = Uri.EscapeDataString(roleName);
        var roleResponse = await client.GetAsync($"/admin/realms/{_options.Realm}/roles/{escapedRoleName}", ct);

        if (roleResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            var createResponse = await client.PostAsJsonAsync(
                $"/admin/realms/{_options.Realm}/roles",
                new { name = roleName },
                ct);
            createResponse.EnsureSuccessStatusCode();

            roleResponse = await client.GetAsync($"/admin/realms/{_options.Realm}/roles/{escapedRoleName}", ct);
        }

        roleResponse.EnsureSuccessStatusCode();
        return (await roleResponse.Content.ReadFromJsonAsync<KeycloakRoleResponse>(cancellationToken: ct))!;
    }

    private async Task<KeycloakUserResponse?> GetUserByIdAsync(HttpClient client, string keycloakId, CancellationToken ct)
    {
        var response = await client.GetAsync(
            $"/admin/realms/{_options.Realm}/users/{Uri.EscapeDataString(keycloakId)}",
            ct);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<KeycloakUserResponse>(cancellationToken: ct);
    }

    private static void UpsertOrRemoveAttribute(Dictionary<string, string[]> attributes, string key, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            attributes.Remove(key);
        else
            attributes[key] = [value];
    }

    private sealed class AdminTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = default!;
    }

    private sealed class KeycloakRoleResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = default!;

        [JsonPropertyName("name")]
        public string Name { get; set; } = default!;
    }

    private sealed class KeycloakUserResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = default!;

        [JsonPropertyName("username")]
        public string Username { get; set; } = default!;

        [JsonPropertyName("email")]
        public string Email { get; set; } = default!;

        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; } = true;

        [JsonPropertyName("emailVerified")]
        public bool? EmailVerified { get; set; }

        [JsonPropertyName("firstName")]
        public string FirstName { get; set; } = default!;

        [JsonPropertyName("lastName")]
        public string LastName { get; set; } = default!;

        [JsonPropertyName("attributes")]
        public Dictionary<string, string[]>? Attributes { get; set; }
    }
}
