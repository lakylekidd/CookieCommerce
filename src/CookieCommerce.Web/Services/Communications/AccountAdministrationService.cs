using System.Net.Http.Headers;
using CookieCommerce.Web.Services.Commerce;

namespace CookieCommerce.Web.Services.Communications;

public sealed record AccountAccessStatus(string? IdentityId, string State, int FailedAttempts, string? LastRecoveryState = null);

public sealed class AccountAdministrationService(IHttpClientFactory clients, IAuthService auth, CommerceAccess access)
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
        using var client = await AuthorizedClientAsync();
        using var response = await client.PostAsync($"api/admin/accounts/{customerId}/{action}", null);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException("The account action failed. Check email settings or sign in again and retry.");
    }
}
