using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Hosting;

namespace CookieCommerce.AuthenticationManagement.Infrastructure.Keycloak;

internal sealed partial class KeycloakBootstrapService
{
    private async Task EnsureAdministrationClientAsync(HttpClient http, CancellationToken ct)
    {
        if (_environment is not null && !_environment.IsDevelopment() && _options.AdminClientSecret == "cookiecommerce-development-admin-secret")
            throw new InvalidOperationException("Configure a private Keycloak:AdminClientSecret outside Development.");
        var clients = await http.GetFromJsonAsync<List<JsonObject>>($"/admin/realms/{_options.Realm}/clients?clientId={Uri.EscapeDataString(_options.AdminClientId)}", ct) ?? [];
        if (clients.Count == 0)
        {
            using var create = await http.PostAsJsonAsync($"/admin/realms/{_options.Realm}/clients", new
            {
                clientId = _options.AdminClientId, enabled = true, publicClient = false, secret = _options.AdminClientSecret,
                serviceAccountsEnabled = true, standardFlowEnabled = false, directAccessGrantsEnabled = false
            }, ct);
            create.EnsureSuccessStatusCode();
            clients = (await http.GetFromJsonAsync<List<JsonObject>>($"/admin/realms/{_options.Realm}/clients?clientId={Uri.EscapeDataString(_options.AdminClientId)}", ct))!;
        }
        var serviceUser = await http.GetFromJsonAsync<JsonObject>($"/admin/realms/{_options.Realm}/clients/{clients.Single()["id"]!.GetValue<string>()}/service-account-user", ct);
        var management = await http.GetFromJsonAsync<List<JsonObject>>($"/admin/realms/{_options.Realm}/clients?clientId=realm-management", ct);
        var managementId = management!.Single()["id"]!.GetValue<string>();
        var roles = new List<JsonObject>();
        foreach (var role in new[] { "manage-users", "view-users", "view-realm", "manage-realm", "manage-clients", "view-clients" })
            roles.Add((await http.GetFromJsonAsync<JsonObject>($"/admin/realms/{_options.Realm}/clients/{managementId}/roles/{role}", ct))!);
        using var mapping = await http.PostAsJsonAsync($"/admin/realms/{_options.Realm}/users/{serviceUser!["id"]!.GetValue<string>()}/role-mappings/clients/{managementId}", roles, ct);
        mapping.EnsureSuccessStatusCode();
    }

    private async Task EnsureAccountConfigurationAsync(HttpClient http, CancellationToken ct)
    {
        var realm = await http.GetFromJsonAsync<JsonObject>($"/admin/realms/{_options.Realm}", ct) ?? new();
        var attributes = realm["attributes"]?.DeepClone().AsObject() ?? new JsonObject();
        if (attributes["cookiecommerce.security.v1"] is null)
        {
            attributes["cookiecommerce.security.v1"] = "true";
            using var response = await http.PutAsJsonAsync($"/admin/realms/{_options.Realm}", new
            {
                bruteForceProtected = true, permanentLockout = false, failureFactor = 5, loginWithEmailAllowed = true,
                waitIncrementSeconds = 60, maxFailureWaitSeconds = 900, resetPasswordAllowed = true, attributes
            }, ct);
            response.EnsureSuccessStatusCode();
        }
        var currentPolicy = realm["passwordPolicy"]?.GetValue<string>() ?? "";
        var passwordPolicy = KeycloakPasswordPolicy.Resolve(_environment?.IsDevelopment() == true, currentPolicy);
        if (passwordPolicy != currentPolicy)
        {
            using var response = await http.PutAsJsonAsync($"/admin/realms/{_options.Realm}", new { passwordPolicy }, ct);
            response.EnsureSuccessStatusCode();
        }
        var profilePath = $"/admin/realms/{_options.Realm}/users/profile";
        var profile = await http.GetFromJsonAsync<JsonObject>(profilePath, ct) ?? new();
        var profileAttributes = profile["attributes"]?.AsArray() ?? new JsonArray();
        if (profile["attributes"] is null) profile["attributes"] = profileAttributes;
        var customerLink = profileAttributes.OfType<JsonObject>().FirstOrDefault(a => a["name"]?.GetValue<string>() == "cookiecommerce_customer_id");
        if (customerLink is null)
        {
            customerLink = new JsonObject { ["name"] = "cookiecommerce_customer_id", ["displayName"] = "Customer account ID" };
            profileAttributes.Add(customerLink);
        }
        // This claim determines order/profile ownership. Customers must never be able to edit it.
        customerLink["permissions"] = new JsonObject { ["view"] = new JsonArray("admin"), ["edit"] = new JsonArray("admin") };
        using var profileUpdate = await http.PutAsJsonAsync(profilePath, profile, ct);
        profileUpdate.EnsureSuccessStatusCode();
        var clients = await http.GetFromJsonAsync<List<JsonObject>>($"/admin/realms/{_options.Realm}/clients?clientId={Uri.EscapeDataString(_options.ClientId)}", ct);
        var clientId = clients!.Single()["id"]!.GetValue<string>();
        var path = $"/admin/realms/{_options.Realm}/clients/{clientId}/protocol-mappers/models";
        var mappers = await http.GetFromJsonAsync<List<JsonObject>>(path, ct) ?? [];
        foreach (var mapper in new[]
        {
            new { name = "cookiecommerce-customer-id", protocol = "openid-connect", protocolMapper = "oidc-usermodel-attribute-mapper",
                config = new Dictionary<string, string> { ["user.attribute"] = "cookiecommerce_customer_id", ["claim.name"] = "customer_id", ["jsonType.label"] = "String", ["access.token.claim"] = "true", ["id.token.claim"] = "true", ["userinfo.token.claim"] = "true" } },
            new { name = "cookiecommerce-api-audience", protocol = "openid-connect", protocolMapper = "oidc-audience-mapper",
                config = new Dictionary<string, string> { ["included.client.audience"] = _options.ClientId, ["access.token.claim"] = "true", ["id.token.claim"] = "false" } }
        })
        {
            if (mappers.Any(m => m["name"]?.GetValue<string>() == mapper.name)) continue;
            using var response = await http.PostAsJsonAsync(path, mapper, ct);
            response.EnsureSuccessStatusCode();
        }
    }
}
