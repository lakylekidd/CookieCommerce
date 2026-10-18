namespace CookieCommerce.Web.Services.Communications;

public sealed class EmailSettings
{
    public bool Enabled { get; set; }
    public string Host { get; set; } = "";
    public int Port { get; set; } = 587;
    public string Security { get; set; } = "StartTls";
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public bool ClearPassword { get; set; }
    public bool HasPassword { get; set; }
    public string From { get; set; } = "info@tigervancookie.nl";
    public string SenderName { get; set; } = "Tiger van Cookie";
    public string ReplyTo { get; set; } = "";
    public string PublicStoreUrl { get; set; } = "";
    public string AuthenticationSync { get; set; } = "Not configured";
}

public sealed class StoredEmailConfiguration
{
    public EmailSettings Settings { get; set; } = new();
    public string ProtectedPassword { get; set; } = "";
}

public sealed record SmtpConfiguration(EmailSettings Settings, string Password);

public sealed record EmailBranding(
    string StoreName,
    string LogoUrl,
    string PrimaryColor = "#D97706",
    string HeaderColor = "#451A03",
    string BackgroundColor = "#FFFBEB",
    string SurfaceColor = "#FFFFFF",
    string TextColor = "#451A03",
    string MutedTextColor = "#92400E",
    string BodyFont = "Nunito, Arial, Helvetica, sans-serif",
    string HeadingFont = "Fredoka, Arial, Helvetica, sans-serif")
{
    public static EmailBranding Default { get; } = new("Tiger van Cookie", "");

    public static EmailBranding From(InvoiceSettings company, string? publicStoreUrl)
    {
        var storeName = string.IsNullOrWhiteSpace(company.CompanyName) ? Default.StoreName : company.CompanyName.Trim();
        var logoUrl = "";
        if (Uri.TryCreate(publicStoreUrl, UriKind.Absolute, out var baseUri) && baseUri.Scheme is "http" or "https"
            && string.IsNullOrEmpty(baseUri.UserInfo) && company.LogoPath.StartsWith("/images/", StringComparison.Ordinal)
            && !company.LogoPath.Contains("..", StringComparison.Ordinal))
            logoUrl = new Uri(baseUri, company.LogoPath).ToString();
        return new(storeName, logoUrl);
    }
}

public sealed class OutboundMessage
{
    public string Id { get; set; } = "";
    public string To { get; set; } = "";
    public string Subject { get; set; } = "";
    public string HtmlBody { get; set; } = "";
    public string TextBody { get; set; } = "";
    public string ReplyTo { get; set; } = "";
    public string TemplateKey { get; set; } = "";
    public int TemplateVersion { get; set; }
    public Guid? InvoiceId { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? SentAt { get; set; }
    public DateTimeOffset RetryAfter { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
    public int Attempts { get; set; }
    public string State { get; set; } = "Queued";
    public string? LastFailure { get; set; }
}

public sealed class MessageTemplate
{
    public string Key { get; set; } = "";
    public string Name { get; set; } = "";
    public string Kind { get; set; } = "Email";
    public int Version { get; set; }
    public string Subject { get; set; } = "";
    public string HtmlBody { get; set; } = "";
    public string TextBody { get; set; } = "";
    public string Heading { get; set; } = "Invoice {{invoice_number}}";
    public string Footer { get; set; } = "Thank you for your order.";
    public string AccentColor { get; set; } = "#F97316";
    public DateTimeOffset? PublishedAt { get; set; }
}

public sealed record RenderedEmail(string Subject, string TextBody, string HtmlBody, int Version);

public sealed class InvoiceSettings
{
    public string CompanyName { get; set; } = "Tiger van Cookie";
    public string CompanyAddress { get; set; } = "";
    public string CompanyRegistration { get; set; } = "";
    public string VatNumber { get; set; } = "";
    public string Prefix { get; set; } = "INV";
    public bool EmailInvoices { get; set; }
    public string LogoPath { get; set; } = "/images/logo.png";
}
