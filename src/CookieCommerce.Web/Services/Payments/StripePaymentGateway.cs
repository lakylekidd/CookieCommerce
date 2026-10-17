using System.Globalization;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CookieCommerce.Web.Services.Commerce;
using Microsoft.Extensions.Options;

namespace CookieCommerce.Web.Services.Payments;

public sealed class StripePaymentGateway(IHttpClientFactory factory, IOptions<PaymentOptions> options) : IPaymentGateway
{
    public string Name => "Stripe";

    public async Task<bool> CancelAsync(string sessionId)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"https://api.stripe.com/v1/checkout/sessions/{Uri.EscapeDataString(sessionId)}/expire");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", options.Value.StripeSecretKey);
        using var response = await factory.CreateClient("payments").SendAsync(request);
        return response.IsSuccessStatusCode;
    }

    public async Task<PaymentSession> CreateAsync(CommerceOrder order)
    {
        var config = options.Value;
        if (string.IsNullOrWhiteSpace(config.StripeSecretKey) || !Uri.TryCreate(config.PublicBaseUrl, UriKind.Absolute, out var origin))
            throw new InvalidOperationException("Online payments are not configured yet. Please contact the store.");
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.stripe.com/v1/checkout/sessions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", config.StripeSecretKey);
        request.Headers.Add("Idempotency-Key", order.Id.ToString());
        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["mode"] = "payment", ["customer_email"] = order.Email,
            ["client_reference_id"] = order.Id.ToString(), ["metadata[order_id]"] = order.Id.ToString(),
            ["success_url"] = new Uri(origin, "/shop/checkout/complete").ToString(),
            ["cancel_url"] = new Uri(origin, "/shop/checkout?cancelled=true").ToString(),
            ["line_items[0][price_data][currency]"] = "eur",
            ["line_items[0][price_data][unit_amount]"] = checked((long)decimal.Round(order.Order.Total * 100)).ToString(CultureInfo.InvariantCulture),
            ["line_items[0][price_data][product_data][name]"] = $"Cookie order {order.Order.OrderReference}",
            ["line_items[0][quantity]"] = "1"
        });
        using var response = await factory.CreateClient("payments").SendAsync(request);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException("Unable to start payment. Please try again.");
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return new PaymentSession(json.RootElement.GetProperty("id").GetString()!, json.RootElement.GetProperty("url").GetString()!);
    }

    public PaymentNotification? VerifyWebhook(string payload, string signature, DateTimeOffset now)
    {
        var secret = options.Value.StripeWebhookSecret;
        if (string.IsNullOrWhiteSpace(secret)) throw new InvalidOperationException("Stripe webhook secret is missing.");
        var fields = signature.Split(',').Select(p => p.Split('=', 2)).Where(p => p.Length == 2).ToList();
        var timestamp = fields.FirstOrDefault(p => p[0] == "t")?[1];
        if (!long.TryParse(timestamp, out var seconds) || Math.Abs((decimal)now.ToUnixTimeSeconds() - seconds) > 300)
            throw new UnauthorizedAccessException("Invalid webhook timestamp.");
        var expected = HMACSHA256.HashData(Encoding.UTF8.GetBytes(secret), Encoding.UTF8.GetBytes($"{timestamp}.{payload}"));
        var valid = fields.Where(p => p[0] == "v1").Any(p => ValidSignature(p[1], expected));
        if (!valid) throw new UnauthorizedAccessException("Invalid webhook signature.");
        using var json = JsonDocument.Parse(payload);
        var type = json.RootElement.GetProperty("type").GetString();
        if (type is not ("checkout.session.completed" or "checkout.session.async_payment_succeeded" or "checkout.session.async_payment_failed" or "checkout.session.expired")) return null;
        var session = json.RootElement.GetProperty("data").GetProperty("object");
        if (!session.TryGetProperty("metadata", out var metadata) || !metadata.TryGetProperty("order_id", out var orderId) || !Guid.TryParse(orderId.GetString(), out var parsedOrderId)) return null;
        var status = type switch
        {
            "checkout.session.expired" => "Expired",
            "checkout.session.async_payment_failed" => "Failed",
            _ => session.GetProperty("payment_status").GetString() == "paid" ? "Paid" : "Pending"
        };
        return new PaymentNotification(parsedOrderId,
            session.GetProperty("id").GetString()!, status,
            session.GetProperty("amount_total").GetInt64() / 100m, session.GetProperty("currency").GetString()!);
    }

    private static bool ValidSignature(string value, byte[] expected)
    {
        try { return CryptographicOperations.FixedTimeEquals(Convert.FromHexString(value), expected); }
        catch (FormatException) { return false; }
    }
}
