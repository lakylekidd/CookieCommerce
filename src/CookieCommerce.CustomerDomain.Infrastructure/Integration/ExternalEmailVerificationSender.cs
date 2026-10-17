using System.Net.Http.Json;
using CookieCommerce.CustomerDomain.Application.Customers.Integration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CookieCommerce.CustomerDomain.Infrastructure.Integration;

/// <summary>
/// Concrete adapter that hands off email verification delivery to an external dependency.
/// CustomerDomain remains authoritative for token validity; this adapter only delivers.
/// Raw tokens and unnecessary PII must not be logged.
/// </summary>
public sealed class ExternalEmailVerificationSender : IEmailVerificationDeliverySender
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly EmailVerificationDeliverySettings _options;
    private readonly ILogger<ExternalEmailVerificationSender> _logger;

    public ExternalEmailVerificationSender(
        IHttpClientFactory httpClientFactory,
        IOptions<EmailVerificationDeliverySettings> options,
        ILogger<ExternalEmailVerificationSender> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendAsync(EmailVerificationDeliveryPayload payload, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.ExternalDeliveryEndpoint))
        {
            // In development, log that the handoff was received without logging PII or token
            _logger.LogInformation(
                "Email verification delivery handoff received for customer {CustomerId} " +
                "[purpose={Purpose}, correlationId={CorrelationId}, idempotencyKey={IdempotencyKey}]. " +
                "ExternalDeliveryEndpoint is not configured; skipping external delivery.",
                payload.CustomerId,
                payload.Purpose,
                payload.CorrelationId,
                payload.IdempotencyKey);
            return;
        }

        _logger.LogInformation(
            "Sending email verification delivery handoff for customer {CustomerId} " +
            "[purpose={Purpose}, correlationId={CorrelationId}]",
            payload.CustomerId,
            payload.Purpose,
            payload.CorrelationId);

        // Build the external payload — exclude raw token; use tokenReference only
        var externalPayload = new
        {
            customerId = payload.CustomerId,
            pendingEmail = payload.PendingEmail,
            purpose = payload.Purpose,
            tokenReference = payload.TokenReference,  // reference only, not raw token
            expiresAtUtc = payload.ExpiresAtUtc,
            verificationUrl = payload.VerificationUrl?.ToString(),
            correlationId = payload.CorrelationId,
            idempotencyKey = payload.IdempotencyKey
            // PendingEmail (recipient) is kept out of logs but may be sent to delivery service
        };

        var client = _httpClientFactory.CreateClient("email-delivery");
        client.DefaultRequestHeaders.Add("X-Communications-Key", _options.ServiceKey);
        var response = await client.PostAsJsonAsync(_options.ExternalDeliveryEndpoint, externalPayload, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "External email verification delivery returned {StatusCode} for customer {CustomerId} [correlationId={CorrelationId}]",
                (int)response.StatusCode,
                payload.CustomerId,
                payload.CorrelationId);

            throw new InvalidOperationException(
                $"External email delivery dependency returned HTTP {(int)response.StatusCode}.");
        }
    }
}
