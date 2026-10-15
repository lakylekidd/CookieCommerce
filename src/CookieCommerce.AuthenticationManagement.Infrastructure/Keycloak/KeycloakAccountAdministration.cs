using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using CookieCommerce.AuthenticationManagement.Application.Keycloak;

namespace CookieCommerce.AuthenticationManagement.Infrastructure.Keycloak;

public partial class KeycloakAdminService
{
    private async Task<HttpClient> AdminClientAsync(CancellationToken ct)
    {
        var client = _httpClientFactory.CreateClient("keycloak");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetAdminTokenAsync(ct));
        return client;
    }

    private static KeycloakUserInfo MapUser(KeycloakUserResponse user) => new()
    {
        Id = user.Id, Email = user.Email, FirstName = user.FirstName, LastName = user.LastName,
        CustomerId = user.Attributes?.TryGetValue("cookiecommerce_customer_id", out var ids) == true
            && Guid.TryParse(ids.FirstOrDefault(), out var id) ? id : Guid.TryParse(user.Id, out id) ? id : null
    };

    public async Task<KeycloakUserInfo?> GetUserByIdAsync(string keycloakId, CancellationToken ct = default)
    {
        using var client = await AdminClientAsync(ct);
        var user = await GetUserByIdAsync(client, keycloakId, ct);
        return user is null ? null : MapUser(user);
    }

    public async Task<KeycloakUserInfo?> GetUserByCustomerIdAsync(Guid customerId, CancellationToken ct = default)
    {
        using var client = await AdminClientAsync(ct);
        var linked = await client.GetFromJsonAsync<List<KeycloakUserResponse>>(
            $"/admin/realms/{_options.Realm}/users?q={Uri.EscapeDataString("cookiecommerce_customer_id:" + customerId)}", ct) ?? [];
        if (linked.Count > 1) throw new InvalidOperationException("This customer has ambiguous login identities.");
        if (linked.Count == 1) return MapUser(linked[0]);
        var direct = await GetUserByIdAsync(client, customerId.ToString(), ct);
        return direct is null || MapUser(direct).CustomerId != customerId ? null : MapUser(direct);
    }

    public async Task<KeycloakAccountStatus> GetAccountStatusAsync(string keycloakId, CancellationToken ct = default)
    {
        using var client = await AdminClientAsync(ct);
        var user = await GetUserByIdAsync(client, keycloakId, ct);
        if (user is null) return new(null, "Missing login identity");
        var failures = await client.GetFromJsonAsync<JsonElement>(
            $"/admin/realms/{_options.Realm}/attack-detection/brute-force/users/{Uri.EscapeDataString(keycloakId)}", ct);
        var count = failures.TryGetProperty("numFailures", out var n) ? n.GetInt32() : 0;
        var locked = failures.TryGetProperty("disabled", out var disabled) && disabled.GetBoolean();
        var credentials = await client.GetFromJsonAsync<List<JsonElement>>(
            $"/admin/realms/{_options.Realm}/users/{Uri.EscapeDataString(keycloakId)}/credentials", ct) ?? [];
        var hasPassword = credentials.Any(c => c.TryGetProperty("type", out var type) && type.GetString() == "password");
        var state = !user.Enabled ? "Disabled" : locked ? "Locked" : !hasPassword || user.RequiredActions.Contains("UPDATE_PASSWORD")
            ? "Password setup required" : user.RequiredActions.Contains("VERIFY_EMAIL") ? "Email verification required"
            : user.RequiredActions.Length > 0 ? "Account action required" : "Active";
        return new(keycloakId, state, count);
    }

    public async Task ClearLoginFailuresAsync(string keycloakId, CancellationToken ct = default)
    {
        using var client = await AdminClientAsync(ct);
        using var response = await client.DeleteAsync($"/admin/realms/{_options.Realm}/attack-detection/brute-force/users/{Uri.EscapeDataString(keycloakId)}", ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task SetPasswordAsync(string keycloakId, string password, CancellationToken ct = default)
    {
        using var client = await AdminClientAsync(ct);
        using var response = await client.PutAsJsonAsync($"/admin/realms/{_options.Realm}/users/{Uri.EscapeDataString(keycloakId)}/reset-password",
            new { type = "password", value = password, temporary = false }, ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task EnableUserAsync(string keycloakId, CancellationToken ct = default)
    {
        using var client = await AdminClientAsync(ct);
        using var response = await client.PutAsJsonAsync($"/admin/realms/{_options.Realm}/users/{Uri.EscapeDataString(keycloakId)}", new { enabled = true, requiredActions = Array.Empty<string>() }, ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task<string> ProvisionCustomerAsync(Guid customerId, string email, string firstName, string lastName, CancellationToken ct = default)
    {
        var linked = await GetUserByCustomerIdAsync(customerId, ct);
        if (linked is not null) return linked.Id;
        if (await GetUserByEmailAsync(email, ct) is not null)
            throw new InvalidOperationException("This email already belongs to another login identity. Review the identity link before inviting this customer.");
        using var client = await AdminClientAsync(ct);
        using var response = await client.PostAsJsonAsync($"/admin/realms/{_options.Realm}/users", new
        {
            username = email, email, firstName, lastName, enabled = true, emailVerified = false,
            requiredActions = new[] { "UPDATE_PASSWORD" },
            attributes = new Dictionary<string, string[]> { ["cookiecommerce_customer_id"] = [customerId.ToString()] }
        }, ct);
        response.EnsureSuccessStatusCode();
        var identityId = response.Headers.Location?.Segments.Last().TrimEnd('/') ?? throw new InvalidOperationException("Login identity was not returned.");
        await AssignRealmRoleAsync(identityId, "User", ct);
        return identityId;
    }

    public async Task ConfigureEmailAsync(KeycloakEmailSettings settings, CancellationToken ct = default)
    {
        using var client = await AdminClientAsync(ct);
        var realm = await client.GetFromJsonAsync<JsonObject>($"/admin/realms/{_options.Realm}", ct) ?? new();
        var attributes = realm["attributes"]?.DeepClone().AsObject() ?? new JsonObject();
        attributes["cookiecommerce.storeUrl"] = settings.PublicStoreUrl;
        using var response = await client.PutAsJsonAsync($"/admin/realms/{_options.Realm}", new
        {
            resetPasswordAllowed = true, attributes,
            smtpServer = new Dictionary<string, string>
            {
                ["host"] = settings.Host, ["port"] = settings.Port.ToString(), ["from"] = settings.From,
                ["fromDisplayName"] = settings.SenderName, ["replyTo"] = settings.ReplyTo,
                ["auth"] = (!string.IsNullOrWhiteSpace(settings.Username)).ToString().ToLowerInvariant(),
                ["user"] = settings.Username, ["password"] = settings.Password,
                ["ssl"] = (settings.Security == "Ssl").ToString().ToLowerInvariant(),
                ["starttls"] = (settings.Security == "StartTls").ToString().ToLowerInvariant()
            }
        }, ct);
        response.EnsureSuccessStatusCode();
        var clients = await client.GetFromJsonAsync<List<JsonObject>>($"/admin/realms/{_options.Realm}/clients?clientId={Uri.EscapeDataString(_options.ClientId)}", ct);
        var appClient = clients?.SingleOrDefault() ?? throw new InvalidOperationException("Login client was not found.");
        var redirects = appClient["redirectUris"]?.AsArray().Select(x => x!.GetValue<string>()).ToHashSet() ?? [];
        redirects.Add(new Uri(new Uri(settings.PublicStoreUrl), "/auth/signin").ToString());
        using var redirectResponse = await client.PutAsJsonAsync($"/admin/realms/{_options.Realm}/clients/{appClient["id"]!.GetValue<string>()}", new { redirectUris = redirects }, ct);
        redirectResponse.EnsureSuccessStatusCode();
    }
}
