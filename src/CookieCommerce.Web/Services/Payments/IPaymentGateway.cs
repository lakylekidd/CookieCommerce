using CookieCommerce.Web.Services.Commerce;

namespace CookieCommerce.Web.Services.Payments;

public sealed record PaymentSession(string Id, string Url);
public sealed record PaymentNotification(Guid OrderId, string SessionId, string Status, decimal Amount, string Currency);

public interface IPaymentGateway
{
    string Name { get; }
    Task<PaymentSession> CreateAsync(CommerceOrder order);
    Task<bool> CancelAsync(string sessionId);
}

public sealed class PaymentOptions
{
    public string Provider { get; set; } = "Stripe";
    public string PublicBaseUrl { get; set; } = "";
    public string StripeSecretKey { get; set; } = "";
    public string StripeWebhookSecret { get; set; } = "";
    public string MollieApiKey { get; set; } = "";
}
