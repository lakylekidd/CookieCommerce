using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using CookieCommerce.Web.Services.Commerce;
using Microsoft.Extensions.Options;

namespace CookieCommerce.Web.Services.Payments;

public sealed class MolliePaymentGateway(IHttpClientFactory factory, IOptions<PaymentOptions> options) : IPaymentGateway
{
    public string Name => "Mollie";

    public async Task<bool> CancelAsync(string sessionId)
    {
        using var response = await Client().DeleteAsync($"https://api.mollie.com/v2/payments/{Uri.EscapeDataString(sessionId)}");
        return response.IsSuccessStatusCode;
    }

    private HttpClient Client()
    {
        if (string.IsNullOrWhiteSpace(options.Value.MollieApiKey)) throw new InvalidOperationException("Mollie is not configured.");
        var client = factory.CreateClient("payments");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", options.Value.MollieApiKey);
        return client;
    }

    public async Task<PaymentSession> CreateAsync(CommerceOrder order)
    {
        if (!Uri.TryCreate(options.Value.PublicBaseUrl, UriKind.Absolute, out var origin)) throw new InvalidOperationException("The public site URL is missing.");
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.mollie.com/v2/payments");
        request.Headers.Add("Idempotency-Key", order.Id.ToString());
        request.Content = JsonContent.Create(new
        {
            amount = new { currency = "EUR", value = order.Order.Total.ToString("F2", CultureInfo.InvariantCulture) },
            description = order.Order.OrderReference, metadata = new { order_id = order.Id },
            redirectUrl = new Uri(origin, "/shop/checkout/complete").ToString(),
            webhookUrl = new Uri(origin, "/api/payments/mollie/webhook").ToString()
        });
        using var response = await Client().SendAsync(request);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException("Unable to start payment. Please try again.");
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return new PaymentSession(json.RootElement.GetProperty("id").GetString()!, json.RootElement.GetProperty("_links").GetProperty("checkout").GetProperty("href").GetString()!);
    }

    public async Task<PaymentNotification> RetrieveAsync(string paymentId)
    {
        if (!System.Text.RegularExpressions.Regex.IsMatch(paymentId, "^tr_[A-Za-z0-9]+$")) throw new UnauthorizedAccessException("Invalid payment ID.");
        using var response = await Client().GetAsync($"https://api.mollie.com/v2/payments/{paymentId}");
        response.EnsureSuccessStatusCode();
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = json.RootElement;
        var status = root.GetProperty("status").GetString() switch { "paid" => "Paid", "failed" => "Failed", "expired" => "Expired", "canceled" => "Cancelled", _ => "Pending" };
        return new PaymentNotification(Guid.Parse(root.GetProperty("metadata").GetProperty("order_id").GetString()!), paymentId, status,
            decimal.Parse(root.GetProperty("amount").GetProperty("value").GetString()!, CultureInfo.InvariantCulture), root.GetProperty("amount").GetProperty("currency").GetString()!);
    }
}
