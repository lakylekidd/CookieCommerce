using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using CookieCommerce.AuthDomain.Application.Auth.Commands;
using CookieCommerce.AuthDomain.Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CookieCommerce.AuthDomain.Infrastructure.Keycloak;

internal sealed class KeycloakBootstrapService : BackgroundService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly KeycloakOptions _options;
    private readonly ILogger<KeycloakBootstrapService> _logger;

    public KeycloakBootstrapService(
        IHttpClientFactory httpClientFactory,
        IServiceScopeFactory scopeFactory,
        IOptions<KeycloakOptions> options,
        ILogger<KeycloakBootstrapService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("KeycloakBootstrapService starting — BaseUrl={BaseUrl} Realm={Realm} AdminUser={AdminUser}",
            _options.BaseUrl, _options.Realm, _options.AdminUser);

        try
        {
            // Use a plain client with a concrete base address so we're not affected
            // by any service-discovery rewriting on the named "keycloak" client.
            var http = _httpClientFactory.CreateClient();
            http.BaseAddress = new Uri(_options.BaseUrl.TrimEnd('/') + "/");

            var token = await GetAdminTokenWithRetryAsync(http, stoppingToken);
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            await EnsureRealmAsync(http, stoppingToken);
            await EnsureClientAsync(http, stoppingToken);
            await EnsureUserAsync("admin@tiger.com", "Tiger", "Baker", "Admin123!", stoppingToken);
            await EnsureUserAsync("customer@tiger.com", "John", "Doe", "Customer123!", stoppingToken);

            _logger.LogInformation("Keycloak bootstrap complete");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Keycloak bootstrap failed");
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
                _logger.LogWarning("Attempt {Attempt}/15 failed ({Message}). Retrying in 3s...", attempt, ex.Message);
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

    private async Task EnsureUserAsync(
        string email, string firstName, string lastName, string password,
        CancellationToken ct)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            await mediator.Send(new RegisterUser(email, password, firstName, lastName), ct);
            _logger.LogInformation("Seed user '{Email}' created", email);
        }
        catch (EmailAlreadyTakenException)
        {
            _logger.LogInformation("Seed user '{Email}' already exists — skipping", email);
        }
    }
}
