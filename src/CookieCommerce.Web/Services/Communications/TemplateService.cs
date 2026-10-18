using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using CookieCommerce.Web.Services.Commerce;

namespace CookieCommerce.Web.Services.Communications;

public sealed class TemplateService(ICommerceStore store, CommerceAccess access)
{
    public static readonly string[] EmailKeys = ["password-reset", "account-invitation", "email-verification", "order-confirmation", "invoice-email"];
    private static readonly Regex Tokens = new("\\{\\{([a-z_]+)\\}\\}", RegexOptions.Compiled);
    private static readonly string[] ImageAlignments = ["left", "center", "right"];
    public static string[] Allowed(string key) => key == "invoice-pdf" ? ["store_name", "invoice_number", "order_reference", "customer_name"]
        : key is "password-reset" or "account-invitation" or "email-verification" ? ["customer_name", "store_name", "action_url", "expiry_minutes"]
        : ["customer_name", "store_name", "order_reference", "order_total", "invoice_number"];

    public static MessageTemplate Default(string key) => key switch
    {
        "invoice-pdf" => new() { Key = key, Name = "PDF invoice", Kind = "Pdf", Version = 1, AccentColor = "#D97706" },
        "password-reset" => Email(key, "Password reset", "Reset your password", "Reset password"),
        "account-invitation" => Email(key, "Account invitation", "Set up your password", "Set password"),
        "email-verification" => Email(key, "Email verification", "Verify your email address", "Verify email"),
        "order-confirmation" => new() { Key = key, Name = "Order confirmation", Version = 1, Subject = "Order {{order_reference}} confirmed", HtmlBody = "<p>Hello {{customer_name}},</p><p>We received your payment for order {{order_reference}} ({{order_total}}).</p><p>{{store_name}}</p>", TextBody = "Hello {{customer_name}}, your payment for order {{order_reference}} ({{order_total}}) is confirmed. {{store_name}}" },
        "invoice-email" => new() { Key = key, Name = "Invoice email", Version = 1, Subject = "Invoice {{invoice_number}} for order {{order_reference}}", HtmlBody = "<p>Hello {{customer_name}},</p><p>Your invoice {{invoice_number}} for order {{order_reference}} is attached.</p><p>{{store_name}}</p>", TextBody = "Hello {{customer_name}}, your invoice {{invoice_number}} for order {{order_reference}} is attached. {{store_name}}" },
        _ => throw new ArgumentException("Unknown template.")
    };

    private static MessageTemplate Email(string key, string name, string title, string label) => new()
    {
        Key = key, Name = name, Version = 1, Subject = title + " — {{store_name}}",
        HtmlBody = "<p>Hello {{customer_name}},</p><p><a href=\"{{action_url}}\">" + label + "</a></p><p>This link expires in {{expiry_minutes}} minutes. If you did not request it, you can ignore this email.</p><p>{{store_name}}</p>",
        TextBody = "Hello {{customer_name}}, " + title + ": {{action_url}}\nThis link expires in {{expiry_minutes}} minutes. If you did not request it, ignore this email.\n{{store_name}}"
    };

    public async Task<IReadOnlyList<MessageTemplate>> ListAsync()
    {
        await access.RequireAdminAsync();
        var result = new List<MessageTemplate>();
        foreach (var key in EmailKeys.Append("invoice-pdf")) result.Add(await DraftAsync(key));
        return result;
    }

    public async Task<MessageTemplate> DraftAsync(string key)
    {
        await access.RequireAdminAsync();
        return await store.LoadAsync<MessageTemplate>(key + ":draft") ?? await PublishedAsync(store, key);
    }

    public static async Task<MessageTemplate> PublishedAsync(ICommerceStore store, string key) =>
        await store.LoadAsync<MessageTemplate>(key + ":published") ?? Default(key);

    public async Task<EmailBranding> GetEmailBrandingAsync()
    {
        await access.RequireAdminAsync();
        var company = await store.LoadAsync<InvoiceSettings>("company") ?? new();
        var smtp = await store.LoadAsync<StoredEmailConfiguration>("smtp");
        return EmailBranding.From(company, smtp?.Settings.PublicStoreUrl);
    }

    public async Task SaveAsync(MessageTemplate template, bool publish)
    {
        await access.RequireAdminAsync();
        Validate(template);
        await store.ExecuteAsync("template:" + template.Key, async tx =>
        {
            var copy = JsonSerializer.Deserialize<MessageTemplate>(JsonSerializer.Serialize(template))!;
            if (publish)
            {
                copy.Version = (await tx.LoadAsync<MessageTemplate>(copy.Key + ":published"))?.Version + 1 ?? 2;
                copy.PublishedAt = DateTimeOffset.UtcNow;
                await tx.SaveAsync(copy.Key + ":v" + copy.Version, copy);
                await tx.SaveAsync(copy.Key + ":published", copy);
            }
            await tx.SaveAsync(copy.Key + ":draft", copy);
            return true;
        });
    }

    public static void Validate(MessageTemplate template)
    {
        var definition = Default(template.Key);
        if (definition.Kind != template.Kind) throw new InvalidOperationException("Template type cannot be changed.");
        var fields = template.Kind == "Pdf" ? new[] { template.Heading, template.Footer } : new[] { template.Subject, template.HtmlBody, template.TextBody };
        if (fields.Any(f => f.Length > 50000)) throw new InvalidOperationException("Template is too long.");
        foreach (var text in fields)
            if (Tokens.Matches(text).Any(m => !Allowed(template.Key).Contains(m.Groups[1].Value))
                || Tokens.Replace(text, "").Contains("{{") || Tokens.Replace(text, "").Contains("}}"))
                throw new InvalidOperationException("Use only the listed placeholders in the form {{placeholder}}.");
        if (template.Kind == "Email" && (string.IsNullOrWhiteSpace(template.Subject) || string.IsNullOrWhiteSpace(template.TextBody)
            || template.Subject.Contains('\r') || template.Subject.Contains('\n')))
            throw new InvalidOperationException("Enter a subject and plain-text email body.");
        if (template.Key is "password-reset" or "account-invitation" or "email-verification")
            if (!template.TextBody.Contains("{{action_url}}") || !template.HtmlBody.Contains("{{action_url}}"))
                throw new InvalidOperationException("Recovery and verification templates must contain the action URL in both bodies.");
        if (Regex.IsMatch(template.HtmlBody, @"<\s*(script|iframe|object|embed|form)|\bon\w+\s*=|javascript\s*:|data\s*:", RegexOptions.IgnoreCase))
            throw new InvalidOperationException("Email templates cannot contain scripts, forms or executable content.");
        if (!Regex.IsMatch(template.AccentColor, "^#[0-9a-fA-F]{6}$")) throw new InvalidOperationException("Enter a six-digit hex accent colour.");
    }

    public static string CreateImageHtml(string imageUrl, string altText, string alignment)
    {
        var trimmedUrl = imageUrl.Trim();
        if (!Uri.TryCreate(trimmedUrl, UriKind.Absolute, out var imageUri) || imageUri.Scheme != Uri.UriSchemeHttps || !string.IsNullOrEmpty(imageUri.UserInfo))
            throw new InvalidOperationException("Enter an absolute HTTPS image URL.");

        var normalizedAlignment = alignment.Trim().ToLowerInvariant();
        if (!ImageAlignments.Contains(normalizedAlignment, StringComparer.Ordinal))
            throw new InvalidOperationException("Choose left, centre, or right image alignment.");

        var trimmedAltText = altText.Trim();
        if (trimmedAltText.Length > 500) throw new InvalidOperationException("Image alternative text is too long.");

        return $"<p align=\"{normalizedAlignment}\" style=\"margin: 0; text-align: {normalizedAlignment};\"><img src=\"{WebUtility.HtmlEncode(imageUri.AbsoluteUri)}\" alt=\"{WebUtility.HtmlEncode(trimmedAltText)}\" style=\"display: inline-block; max-width: 100%; height: auto; border: 0;\" /></p>";
    }

    public static string Replace(string template, IReadOnlyDictionary<string, string> values, bool html) => Tokens.Replace(template,
        m => values.TryGetValue(m.Groups[1].Value, out var value) ? html ? WebUtility.HtmlEncode(value) : value : throw new InvalidOperationException("Missing template value: " + m.Groups[1].Value));

    public static string ApplyEmailBranding(string htmlBody, MessageTemplate template, EmailBranding? branding = null)
    {
        _ = template;
        var brand = branding ?? EmailBranding.Default;
        var storeName = WebUtility.HtmlEncode(brand.StoreName);
        var logo = string.IsNullOrWhiteSpace(brand.LogoUrl)
            ? ""
            : $"<img src=\"{WebUtility.HtmlEncode(brand.LogoUrl)}\" alt=\"{storeName}\" width=\"56\" style=\"display:block;width:56px;height:56px;object-fit:contain;margin:0 auto 12px;border:0;\" />";
        var content = Regex.Replace(htmlBody.Trim(),
            @"<a\b(?![^>]*\bstyle\s*=)([^>]*)>",
            $"<a$1 style=\"display:inline-block;background:{brand.PrimaryColor};color:#FFFFFF;text-decoration:none;font-weight:700;padding:12px 22px;border-radius:999px;\">",
            RegexOptions.IgnoreCase);
        return $"<!doctype html><html lang=\"en\"><head><meta charset=\"utf-8\"><meta name=\"viewport\" content=\"width=device-width, initial-scale=1\"><title>{storeName}</title><style>body{{margin:0!important;padding:0!important;background:{brand.BackgroundColor};}}a{{color:{brand.PrimaryColor};}}.email-shell{{width:100%;}}@media screen and (max-width:600px){{.email-card{{width:100%!important;}}.email-content{{padding:24px!important;}}}}</style></head><body style=\"margin:0;padding:0;background:{brand.BackgroundColor};font-family:{brand.BodyFont};color:{brand.TextColor};\"><table role=\"presentation\" class=\"email-shell\" width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:100%;background:{brand.BackgroundColor};margin:0;padding:0;\"><tr><td align=\"center\" style=\"padding:32px 16px;\"><table role=\"presentation\" class=\"email-card\" width=\"600\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:100%;max-width:600px;background:{brand.SurfaceColor};border-radius:20px;overflow:hidden;box-shadow:0 8px 30px rgba(69,26,3,.12);\"><tr><td align=\"center\" style=\"padding:28px 32px;background:{brand.HeaderColor};color:#FFFFFF;font-family:{brand.HeadingFont};font-size:24px;font-weight:700;\">{logo}<span>{storeName}</span></td></tr><tr><td class=\"email-content\" style=\"padding:36px 40px;background:{brand.SurfaceColor};font-family:{brand.BodyFont};font-size:16px;line-height:1.6;color:{brand.TextColor};\">{content}</td></tr><tr><td align=\"center\" style=\"padding:20px 32px;background:{brand.BackgroundColor};font-family:{brand.BodyFont};font-size:12px;line-height:1.5;color:{brand.MutedTextColor};\">{storeName}</td></tr></table></td></tr></table></body></html>";
    }

    public static RenderedEmail Render(MessageTemplate template, IReadOnlyDictionary<string, string> values, EmailBranding? branding = null)
    {
        Validate(template);
        var result = new RenderedEmail(Replace(template.Subject, values, false), Replace(template.TextBody, values, false),
            ApplyEmailBranding(Replace(template.HtmlBody, values, true), template, branding), template.Version);
        if (result.Subject.Contains('\r') || result.Subject.Contains('\n')) throw new InvalidOperationException("Email subject cannot contain line breaks.");
        return result;
    }
}
