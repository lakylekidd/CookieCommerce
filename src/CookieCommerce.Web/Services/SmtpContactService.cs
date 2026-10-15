using System.Net;
using System.Net.Mail;

namespace CookieCommerce.Web.Services;

public class SmtpContactService(IConfiguration configuration, ILogger<SmtpContactService> logger) : IContactService
{
    private const string RecipientEmail = "info@tigervancookie.nl";

    public async Task<bool> SendContactEmailAsync(string fromEmail, string subject, string message)
    {
        var host = configuration["Smtp:Host"];
        var port = configuration.GetValue<int>("Smtp:Port", 587);
        var username = configuration["Smtp:Username"];
        var password = configuration["Smtp:Password"];
        var enableSsl = configuration.GetValue<bool>("Smtp:EnableSsl", true);

        if (string.IsNullOrWhiteSpace(host))
        {
            logger.LogWarning("SMTP host is not configured. Contact email was not sent.");
            return false;
        }

        try
        {
            using var client = new SmtpClient(host, port);
            client.EnableSsl = enableSsl;

            if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
                client.Credentials = new NetworkCredential(username, password);

            using var mail = new MailMessage
            {
                From = new MailAddress(configuration["Smtp:From"] ?? (string.IsNullOrWhiteSpace(username) ? RecipientEmail : username), "Tiger van Cookie – Contact Form"),
                Subject = $"[Contact] {subject}",
                Body = $"From: {fromEmail}\n\n{message}",
                IsBodyHtml = false
            };

            mail.To.Add(RecipientEmail);
            mail.ReplyToList.Add(new MailAddress(fromEmail));

            await client.SendMailAsync(mail);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send contact email from {FromEmail}", fromEmail);
            return false;
        }
    }
}
