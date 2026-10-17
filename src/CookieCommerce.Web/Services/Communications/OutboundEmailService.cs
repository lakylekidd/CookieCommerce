using CookieCommerce.Web.Services.Commerce;
using System.Net.Sockets;
using System.Security.Cryptography;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace CookieCommerce.Web.Services.Communications;

public sealed class OutboundEmailService(
    ICommerceStore store,
    EmailConfigurationReader settings,
    ILogger<OutboundEmailService> logger)
{
    public async Task SendNowAsync(OutboundMessage message, CancellationToken ct = default)
    {
        var deliveryKind = string.IsNullOrWhiteSpace(message.TemplateKey) ? "ad-hoc" : message.TemplateKey;
        var smtpHost = "unavailable";
        var smtpPort = 0;
        var smtpSecurity = "unavailable";

        try
        {
            var config = await settings.ReadAsync();
            smtpHost = config.Settings.Host;
            smtpPort = config.Settings.Port;
            smtpSecurity = config.Settings.Security;
            if (!config.Settings.Enabled || string.IsNullOrWhiteSpace(smtpHost)) throw new InvalidOperationException("Outbound email is disabled or not configured.");
            if (message.ExpiresAt <= DateTimeOffset.UtcNow) throw new InvalidOperationException("This email's action link has expired.");

            var mail = await CreateMessage(message, config.Settings);
            using var client = new SmtpClient { Timeout = 15000 };
            var security = smtpSecurity switch { "Ssl" => SecureSocketOptions.SslOnConnect, "StartTls" => SecureSocketOptions.StartTls, _ => SecureSocketOptions.None };
            var host = OperatingSystem.IsWindows() && smtpHost.Equals("host.docker.internal", StringComparison.OrdinalIgnoreCase)
                ? "localhost" : smtpHost;
            await client.ConnectAsync(host, smtpPort, security, ct);
            if (!string.IsNullOrWhiteSpace(config.Settings.Username)) await client.AuthenticateAsync(config.Settings.Username, config.Password, ct);
            await client.SendAsync(mail, ct);
            await client.DisconnectAsync(true, ct);

            logger.LogInformation(
                "Outbound email was accepted by the SMTP server [deliveryKind={DeliveryKind}, smtpHost={SmtpHost}, smtpPort={SmtpPort}, security={SmtpSecurity}]",
                deliveryKind, smtpHost, smtpPort, smtpSecurity);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(
                "Outbound email delivery failed [deliveryKind={DeliveryKind}, failureReason={FailureReason}, smtpHost={SmtpHost}, smtpPort={SmtpPort}, security={SmtpSecurity}]",
                deliveryKind, GetFailureReason(exception), smtpHost, smtpPort, smtpSecurity);
            throw;
        }
    }

    private async Task<MimeMessage> CreateMessage(OutboundMessage message, EmailSettings settings)
    {
        var mail = new MimeMessage();
        mail.From.Add(new MailboxAddress(settings.SenderName, settings.From));
        mail.To.Add(MailboxAddress.Parse(message.To));
        mail.Subject = message.Subject;
        var replyTo = string.IsNullOrEmpty(message.ReplyTo) ? settings.ReplyTo : message.ReplyTo;
        if (!string.IsNullOrEmpty(replyTo)) mail.ReplyTo.Add(MailboxAddress.Parse(replyTo));
        var body = new BodyBuilder { TextBody = message.TextBody, HtmlBody = string.IsNullOrWhiteSpace(message.HtmlBody) ? null : message.HtmlBody };
        if (message.InvoiceId is { } id)
        {
            var invoice = await store.LoadAsync<InvoiceRecord>(id.ToString()) ?? throw new InvalidOperationException("Invoice was not found.");
            var pdf = await store.LoadAsync<InvoicePdf>(id.ToString()) ?? throw new InvalidOperationException("Invoice PDF is still being generated.");
            body.Attachments.Add(invoice.Number + ".pdf", pdf.Bytes, new ContentType("application", "pdf"));
        }
        mail.Body = body.ToMessageBody();
        return mail;
    }

    private static string GetFailureReason(Exception exception) => exception switch
    {
        CryptographicException => "configuration-decryption",
        AuthenticationException => "smtp-authentication",
        SslHandshakeException => "smtp-tls",
        SmtpCommandException => "smtp-command",
        SmtpProtocolException => "smtp-protocol",
        SocketException => "smtp-network",
        TimeoutException or TaskCanceledException => "smtp-timeout",
        InvalidOperationException => "invalid-operation",
        _ => "unexpected"
    };

    public static async Task QueueTemplateAsync(ICommerceTransaction tx, string key, string recipient, string templateKey,
        IReadOnlyDictionary<string, string> values, Guid? invoiceId = null, DateTimeOffset? expiry = null)
    {
        if (!System.Net.Mail.MailAddress.TryCreate(recipient, out _)) throw new InvalidOperationException("Email recipient is invalid.");
        if (await tx.LoadAsync<OutboundMessage>(key) is not null) return;
        var template = await tx.LoadAsync<MessageTemplate>(templateKey + ":published") ?? TemplateService.Default(templateKey);
        var rendered = TemplateService.Render(template, values);
        await tx.SaveAsync(key, new OutboundMessage
        {
            Id = key, To = recipient, Subject = rendered.Subject, HtmlBody = rendered.HtmlBody, TextBody = rendered.TextBody,
            TemplateKey = templateKey, TemplateVersion = template.Version, InvoiceId = invoiceId, ExpiresAt = expiry
        });
    }
}

public sealed class OutboundEmailWorker(ICommerceStore store, OutboundEmailService sender, EmailConfigurationReader settings, ILogger<OutboundEmailWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(10));
        while (await timer.WaitForNextTickAsync(ct))
        {
            try { await DeliverAsync(ct); }
            catch (Exception ex) when (!ct.IsCancellationRequested) { logger.LogWarning("Email queue will retry ({FailureType})", ex.GetType().Name); }
        }
    }

    public async Task DeliverAsync(CancellationToken ct = default)
    {
        if (!(await settings.ReadAsync()).Settings.Enabled) return;
        var messages = (await store.ListAsync<OutboundMessage>()).Where(m => m.State is "Queued" or "Retrying" && m.RetryAfter <= DateTimeOffset.UtcNow).Take(20);
        foreach (var pending in messages)
            await store.ExecuteAsync("outbound:" + pending.Id, async tx =>
            {
                var message = await tx.LoadAsync<OutboundMessage>(pending.Id);
                if (message is null || message.State is not ("Queued" or "Retrying") || message.RetryAfter > DateTimeOffset.UtcNow) return false;
                if (message.ExpiresAt <= DateTimeOffset.UtcNow) message.State = "Expired";
                else
                {
                    try
                    {
                        await sender.SendNowAsync(message, ct);
                        message.SentAt = DateTimeOffset.UtcNow; message.State = "Sent"; message.LastFailure = null;
                    }
                    catch (Exception ex) when (!ct.IsCancellationRequested)
                    {
                        message.Attempts++; message.LastFailure = ex.GetType().Name;
                        message.State = message.Attempts >= 5 ? "Failed" : "Retrying";
                        message.RetryAfter = DateTimeOffset.UtcNow.AddMinutes(Math.Min(30, Math.Pow(2, message.Attempts)));
                    }
                }
                await tx.SaveAsync(message.Id, message); return true;
            });
    }
}
