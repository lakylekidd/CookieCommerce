using System.Net.Http.Headers;
using CookieCommerce.Web.Services.Commerce;

namespace CookieCommerce.Web.Services.Communications;

public sealed record AccountAccessStatus(
    string? IdentityId,
    string State,
    int FailedAttempts,
    string? LastRecoveryState = null,
    bool IsAdmin = false,
    bool IsCurrentUser = false);

public sealed record PasswordSetupInvitation(
    string Token,
    string Email,
    string FirstName,
    string LastName,
    DateTimeOffset ExpiresAt);

public sealed class AccountAdministrationService(
    IHttpClientFactory clients,
    IAuthService auth,
    CommerceAccess access,
    ICommerceStore store,
    EmailConfigurationReader emailConfiguration)
{
    public async Task<HttpClient> AuthorizedClientAsync()
    {
        await access.RequireAdminAsync();
        var token = await auth.GetAccessTokenAsync() ?? throw new UnauthorizedAccessException("Sign in again to administer accounts.");
        var client = clients.CreateClient("authentication-management-api");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    public async Task<AccountAccessStatus> GetAsync(Guid customerId)
    {
        using var client = await AuthorizedClientAsync();
        return await client.GetFromJsonAsync<AccountAccessStatus>("api/admin/accounts/" + customerId)
            ?? throw new InvalidOperationException("Account status is unavailable.");
    }

    public async Task RunAsync(Guid customerId, string action)
    {
        if (action is not ("unblock" or "password-reset" or "invite")) throw new ArgumentException("Unknown account action.");
        if (action == "invite") { await SendInvitationAsync(customerId); return; }
        using var client = await AuthorizedClientAsync();
        using var response = await client.PostAsync($"api/admin/accounts/{customerId}/{action}", null);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException("The account action failed. Check email settings or sign in again and retry.");
    }

    public async Task ProvisionAsync(Guid customerId)
    {
        using var client = await AuthorizedClientAsync();
        using var response = await client.PostAsync($"api/admin/accounts/{customerId}/provision", null);
        response.EnsureSuccessStatusCode();
    }

    public async Task SendInvitationAsync(Guid customerId)
    {
        using var client = await AuthorizedClientAsync();
        using var response = await client.PostAsync($"api/admin/accounts/{customerId}/invite", null);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException("The password setup invitation could not be created.");
        var invitation = await response.Content.ReadFromJsonAsync<PasswordSetupInvitation>()
            ?? throw new InvalidOperationException("The password setup invitation was empty.");
        var settings = (await emailConfiguration.ReadAsync()).Settings;
        if (!Uri.TryCreate(settings.PublicStoreUrl, UriKind.Absolute, out var storeUrl))
            throw new InvalidOperationException("Configure the public store URL before sending invitations.");
        var actionUrl = new Uri(storeUrl, $"/auth/password/setup/{Uri.EscapeDataString(invitation.Token)}").ToString();
        await store.ExecuteAsync("account-invitation:" + invitation.Token, async tx =>
        {
            await OutboundEmailService.QueueTemplateAsync(tx, "account-invitation:" + invitation.Token,
                invitation.Email, "account-invitation", new Dictionary<string, string>
                {
                    ["customer_name"] = $"{invitation.FirstName} {invitation.LastName}".Trim(),
                    ["store_name"] = string.IsNullOrWhiteSpace(settings.SenderName) ? "Tiger van Cookie" : settings.SenderName,
                    ["action_url"] = actionUrl,
                    ["expiry_minutes"] = Math.Max(1, (int)Math.Ceiling((invitation.ExpiresAt - DateTimeOffset.UtcNow).TotalMinutes)).ToString()
                }, expiry: invitation.ExpiresAt);
            return true;
        });
    }

    public async Task SetAdminAsync(Guid customerId, bool isAdmin)
    {
        using var client = await AuthorizedClientAsync();
        using var request = new HttpRequestMessage(isAdmin ? HttpMethod.Put : HttpMethod.Delete,
            $"api/admin/accounts/{customerId}/role/admin");
        using var response = await client.SendAsync(request);
        if (response.IsSuccessStatusCode) return;
        var problem = await response.Content.ReadFromJsonAsync<Microsoft.AspNetCore.Mvc.ProblemDetails>();
        throw new InvalidOperationException(problem?.Title ?? "The account role could not be changed.");
    }
}
