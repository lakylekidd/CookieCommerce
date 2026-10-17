using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CookieCommerce.AuthenticationManagement.Infrastructure.Keycloak;

internal sealed partial class KeycloakBootstrapService : BackgroundService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly KeycloakOptions _options;
    private readonly ILogger<KeycloakBootstrapService> _logger;
    private readonly IHostEnvironment? _environment;

    public KeycloakBootstrapService(
        IHttpClientFactory httpClientFactory,
        IOptions<KeycloakOptions> options,
        ILogger<KeycloakBootstrapService> logger, IHostEnvironment? environment = null)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _logger = logger;
        _environment = environment;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Keycloak realm bootstrap starting [realm={Realm}]", _options.Realm);

        try
        {
            if (_environment is not null && !_environment.IsDevelopment() && _options.AdminClientSecret == "cookiecommerce-development-admin-secret")
                throw new InvalidOperationException("Configure a private Keycloak:AdminClientSecret outside Development.");
            // Use a plain client with a concrete base address so we're not affected
            // by any service-discovery rewriting on the named "keycloak" client.
            var http = _httpClientFactory.CreateClient();
            http.BaseAddress = new Uri(_options.BaseUrl.TrimEnd('/') + "/");

            var token = await GetAdminTokenWithRetryAsync(http, stoppingToken);
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            await EnsureRealmAsync(http, stoppingToken);
            await EnsureClientAsync(http, stoppingToken);
            await EnsureAccountConfigurationAsync(http, stoppingToken);
            await EnsureAdministrationClientAsync(http, stoppingToken);

            _logger.LogInformation("Keycloak realm/client bootstrap complete");
        }
        catch (Exception ex)
        {
            _logger.LogError("Keycloak bootstrap failed [failureType={FailureType}]", ex.GetType().Name);
        }
    }

    private async Task<string> GetAdminTokenWithRetryAsync(HttpClient http, CancellationToken ct)
    {
        for (var attempt = 1; attempt <= 15; attempt++)
        {
            try
            {
                var response = await http.PostAsync(
                    "/realms/master/protocol/openid-connect/token",
                    new FormUrlEncodedContent(new Dictionary<string, string>
                    {
                        ["grant_type"] = "password",
                        ["client_id"] = "admin-cli",
                        ["username"] = _options.AdminUser,
                        ["password"] = _options.AdminPassword,
                    }),
                    ct);

                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);
                _logger.LogInformation("Got Keycloak admin token on attempt {Attempt}", attempt);
                return json.GetProperty("access_token").GetString()!;
            }
            catch (Exception ex) when (attempt < 15)
            {
                _logger.LogWarning("Keycloak admin token request failed [attempt={Attempt}, failureType={FailureType}]; retrying", attempt, ex.GetType().Name);
                await Task.Delay(3_000, ct);
            }
        }

        throw new InvalidOperationException("Could not obtain Keycloak admin token after 15 attempts.");
    }

    private async Task EnsureRealmAsync(HttpClient http, CancellationToken ct)
    {
        var response = await http.GetAsync($"/admin/realms/{_options.Realm}", ct);
        if (response.StatusCode == HttpStatusCode.OK)
        {
            _logger.LogInformation("Realm '{Realm}' already exists", _options.Realm);
            return;
        }

        _logger.LogInformation("Creating realm '{Realm}'", _options.Realm);
        (await http.PostAsJsonAsync("/admin/realms", new
        {
            realm = _options.Realm,
            enabled = true,
            loginWithEmailAllowed = true,
            registrationAllowed = false,
        }, ct)).EnsureSuccessStatusCode();
        _logger.LogInformation("Realm '{Realm}' created", _options.Realm);
    }

    private async Task EnsureClientAsync(HttpClient http, CancellationToken ct)
    {
        var url = $"/admin/realms/{_options.Realm}/clients?clientId={Uri.EscapeDataString(_options.ClientId)}";
        var existing = await http.GetFromJsonAsync<JsonElement>(url, ct);

        if (existing.GetArrayLength() > 0)
        {
            _logger.LogInformation("Client '{ClientId}' already exists", _options.ClientId);
            return;
        }

        _logger.LogInformation("Creating client '{ClientId}'", _options.ClientId);
        (await http.PostAsJsonAsync($"/admin/realms/{_options.Realm}/clients", new
        {
            clientId = _options.ClientId,
            name = "CookieCommerce Backend",
            enabled = true,
            publicClient = false,
            secret = _options.ClientSecret,
            directAccessGrantsEnabled = true,
            serviceAccountsEnabled = true,
            standardFlowEnabled = true,
            implicitFlowEnabled = false,
        }, ct)).EnsureSuccessStatusCode();
        _logger.LogInformation("Client '{ClientId}' created", _options.ClientId);
    }

}
