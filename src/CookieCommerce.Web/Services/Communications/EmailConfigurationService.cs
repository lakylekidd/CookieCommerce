using System.Net.Mail;
using System.Text.Json;
using CookieCommerce.Web.Services.Commerce;
using Microsoft.AspNetCore.DataProtection;

namespace CookieCommerce.Web.Services.Communications;

public sealed class EmailConfigurationReader(ICommerceStore store, IConfiguration configuration, IDataProtectionProvider protection)
{
    private readonly IDataProtector protector = protection.CreateProtector("CookieCommerce.OutboundSmtp.v1");

    public async Task<SmtpConfiguration> ReadAsync()
    {
        var saved = await store.LoadAsync<StoredEmailConfiguration>("smtp");
        if (saved is not null) return new(saved.Settings, string.IsNullOrEmpty(saved.ProtectedPassword) ? "" : protector.Unprotect(saved.ProtectedPassword));
        return new(new EmailSettings
        {
            Enabled = !string.IsNullOrWhiteSpace(configuration["Smtp:Host"]), Host = configuration["Smtp:Host"] ?? "",
            Port = configuration.GetValue("Smtp:Port", 587), Security = configuration.GetValue("Smtp:EnableSsl", true) ? "StartTls" : "None",
            Username = configuration["Smtp:Username"] ?? "", From = configuration["Smtp:From"] ?? "info@tigervancookie.nl",
            PublicStoreUrl = configuration["PublicStoreUrl"] ?? "", HasPassword = !string.IsNullOrEmpty(configuration["Smtp:Password"])
        }, configuration["Smtp:Password"] ?? "");
    }

    public string Protect(string password) => protector.Protect(password);
}

public sealed class EmailConfigurationService(ICommerceStore store, EmailConfigurationReader reader,
    AccountAdministrationService accounts, CommerceAccess access, OutboundEmailService mail)
{
    public async Task<EmailSettings> GetAsync()
    {
        await access.RequireAdminAsync();
        var settings = JsonSerializer.Deserialize<EmailSettings>(JsonSerializer.Serialize((await reader.ReadAsync()).Settings))!;
        settings.Password = "";
        settings.ClearPassword = false;
        return settings;
    }

    public async Task SaveAsync(EmailSettings settings)
    {
        await access.RequireAdminAsync();
        Validate(settings);
        var current = await reader.ReadAsync();
        var secret = settings.ClearPassword ? "" : string.IsNullOrEmpty(settings.Password) ? current.Password : settings.Password;
        var values = JsonSerializer.Deserialize<EmailSettings>(JsonSerializer.Serialize(settings))!;
        values.Password = ""; values.ClearPassword = false; values.HasPassword = secret.Length != 0;
        values.AuthenticationSync = "Pending";
        await store.ExecuteAsync("smtp-settings", async tx =>
        {
            await tx.SaveAsync("smtp", new StoredEmailConfiguration { Settings = values, ProtectedPassword = secret.Length == 0 ? "" : reader.Protect(secret) });
            return true;
        });
        try
        {
            using var client = await accounts.AuthorizedClientAsync();
            using var response = await client.PutAsJsonAsync("api/admin/accounts/email-settings", new
            {
                Host = values.Enabled ? values.Host : "", values.Port, values.Security, values.Username, Password = secret,
                values.From, values.SenderName, values.ReplyTo, values.PublicStoreUrl
            });
            response.EnsureSuccessStatusCode();
            values.AuthenticationSync = "Synchronized";
        }
        catch (Exception ex) when (ex is HttpRequestException or UnauthorizedAccessException or TaskCanceledException)
        { values.AuthenticationSync = "Synchronization failed. Save again to retry."; }
        await store.ExecuteAsync("smtp-settings", async tx =>
        {
            var stored = await tx.LoadAsync<StoredEmailConfiguration>("smtp") ?? throw new InvalidOperationException("Email settings were not saved.");
            stored.Settings.AuthenticationSync = values.AuthenticationSync;
            await tx.SaveAsync("smtp", stored); return true;
        });
    }

    public async Task TestAsync(string recipient)
    {
        await access.RequireAdminAsync();
        await mail.SendNowAsync(new OutboundMessage
        {
            To = recipient,
            TemplateKey = "backoffice-test",
            Subject = "CookieCommerce test email",
            TextBody = "Your outbound email configuration works.",
            ExpiresAt = DateTimeOffset.MaxValue
        });
    }

    public static void Validate(EmailSettings settings)
    {
        if (settings.Port is < 1 or > 65535 || settings.Security is not ("None" or "Ssl" or "StartTls")
            || settings.Enabled && string.IsNullOrWhiteSpace(settings.Host) || !MailAddress.TryCreate(settings.From, out _)
            || settings.ReplyTo.Length > 0 && !MailAddress.TryCreate(settings.ReplyTo, out _)
            || !Uri.TryCreate(settings.PublicStoreUrl, UriKind.Absolute, out var url) || url.Scheme is not ("http" or "https")
            || !string.IsNullOrEmpty(url.UserInfo) || settings.Host.Any(char.IsWhiteSpace))
            throw new InvalidOperationException("Enter a valid SMTP host, port, security mode, sender and public store URL.");
    }
}
