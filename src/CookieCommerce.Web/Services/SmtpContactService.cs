using System.Net;
using System.Net.Mail;

namespace CookieCommerce.Web.Services;

public class SmtpContactService(CookieCommerce.Web.Services.Communications.OutboundEmailService sender, ILogger<SmtpContactService> logger) : IContactService
{
    private const string RecipientEmail = "info@tigervancookie.nl";

    public async Task<bool> SendContactEmailAsync(string fromEmail, string subject, string message)
    {
        try
        {
            if (!MailAddress.TryCreate(fromEmail, out _) || subject.Length > 200 || subject.Any(c => c is '\r' or '\n') || message.Length > 10000) return false;
            await sender.SendNowAsync(new CookieCommerce.Web.Services.Communications.OutboundMessage
            {
                To = RecipientEmail, ReplyTo = fromEmail,
                Subject = $"[Contact] {subject}",
                TextBody = $"From: {fromEmail}\n\n{message}"
            });
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send contact email from {FromEmail}", fromEmail);
            return false;
        }
    }
}
