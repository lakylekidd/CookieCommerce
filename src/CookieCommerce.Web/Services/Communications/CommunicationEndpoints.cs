using System.Security.Cryptography;
using System.Text;
using CookieCommerce.Web.Services.Commerce;

namespace CookieCommerce.Web.Services.Communications;

public static class CommunicationEndpoints
{
    public static bool Authorized(HttpRequest request, IConfiguration configuration)
    {
        var expected = configuration["Communications:ServiceKey"];
        var supplied = request.Headers["X-Communications-Key"].ToString();
        return !string.IsNullOrWhiteSpace(expected) && supplied.Length <= 1024
            && CryptographicOperations.FixedTimeEquals(SHA256.HashData(Encoding.UTF8.GetBytes(expected)), SHA256.HashData(Encoding.UTF8.GetBytes(supplied)));
    }

    public static void MapCommunicationEndpoints(this WebApplication app)
    {
        app.MapPost("/api/internal/email/render", async (HttpRequest request, IConfiguration configuration, ICommerceStore store, AuthenticationEmail input) =>
        {
            if (!Authorized(request, configuration)) return Results.Unauthorized();
            if (input.TemplateKey is not ("password-reset" or "account-invitation" or "email-verification")
                || input.ExpiryMinutes is < 1 or > 1440 || !Uri.TryCreate(input.ActionUrl, UriKind.Absolute, out var url) || url.Scheme is not ("http" or "https")) return Results.BadRequest();
            var template = await TemplateService.PublishedAsync(store, input.TemplateKey);
            var company = await store.LoadAsync<InvoiceSettings>("company") ?? new();
            var rendered = TemplateService.Render(template, new Dictionary<string, string>
            {
                ["customer_name"] = input.CustomerName, ["store_name"] = company.CompanyName,
                ["action_url"] = input.ActionUrl, ["expiry_minutes"] = input.ExpiryMinutes.ToString()
            });
            return Results.Ok(rendered);
        }).DisableAntiforgery().WithMetadata(new Microsoft.AspNetCore.Mvc.RequestSizeLimitAttribute(65536));

        app.MapPost("/api/internal/email/verification", async (HttpRequest request, IConfiguration configuration, ICommerceStore store, VerificationEmail input) =>
        {
            if (!Authorized(request, configuration)) return Results.Unauthorized();
            if (input.ExpiresAtUtc <= DateTimeOffset.UtcNow || !Uri.TryCreate(input.VerificationUrl, UriKind.Absolute, out var url)
                || url.Scheme is not ("http" or "https") || !System.Net.Mail.MailAddress.TryCreate(input.PendingEmail, out _)) return Results.BadRequest();
            await store.ExecuteAsync("verification:" + input.IdempotencyKey, async tx =>
            {
                var company = await tx.LoadAsync<InvoiceSettings>("company") ?? new();
                await OutboundEmailService.QueueTemplateAsync(tx, "verification:" + input.IdempotencyKey, input.PendingEmail, "email-verification", new Dictionary<string, string>
                {
                    ["customer_name"] = "customer", ["store_name"] = company.CompanyName, ["action_url"] = input.VerificationUrl,
                    ["expiry_minutes"] = Math.Max(1, (int)(input.ExpiresAtUtc - DateTimeOffset.UtcNow).TotalMinutes).ToString()
                }, expiry: input.ExpiresAtUtc); return true;
            });
            return Results.Accepted();
        }).DisableAntiforgery().WithMetadata(new Microsoft.AspNetCore.Mvc.RequestSizeLimitAttribute(65536));
    }

    public sealed record AuthenticationEmail(string TemplateKey, string CustomerName, string ActionUrl, long ExpiryMinutes);
    public sealed record VerificationEmail(string PendingEmail, string VerificationUrl, DateTimeOffset ExpiresAtUtc, string IdempotencyKey);
}
