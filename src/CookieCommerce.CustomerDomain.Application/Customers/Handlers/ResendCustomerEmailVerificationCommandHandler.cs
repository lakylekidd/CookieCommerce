using CookieCommerce.CustomerDomain.Application.Customers.Integration;
using CookieCommerce.CustomerDomain.Application.Customers.Profile;
using CookieCommerce.CustomerDomain.Application.Customers.ReadModels;
using CookieCommerce.CustomerDomain.Domain.Exceptions;
using Marten;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CookieCommerce.CustomerDomain.Application.Customers.Handlers;

public class ResendCustomerEmailVerificationCommandHandler(
    IQuerySession querySession,
    IEmailVerificationTokenStore tokenStore,
    IEmailVerificationDeliverySender deliverySender,
    IOptions<EmailVerificationDeliverySettings> deliveryOptions,
    ILogger<ResendCustomerEmailVerificationCommandHandler> logger)
    : IRequestHandler<ResendCustomerEmailVerificationCommand, EmailVerificationDeliveryResult>
{
    public async Task<EmailVerificationDeliveryResult> Handle(
        ResendCustomerEmailVerificationCommand request,
        CancellationToken cancellationToken)
    {
        var readModel = await querySession.LoadAsync<CustomerReadModel>(request.CustomerId, cancellationToken);
        if (readModel is null)
            throw new MissingCustomerProfileException(request.CustomerId);

        if (string.IsNullOrWhiteSpace(readModel.PendingEmail))
            throw new CustomerProfileValidationException(
                "No pending email change exists for this customer.");

        // If a specific pending email was supplied, verify it matches
        if (!string.IsNullOrWhiteSpace(request.PendingEmail))
        {
            var normalizedRequested = request.PendingEmail.Trim().ToLowerInvariant();
            var normalizedPending = readModel.PendingEmail.Trim().ToLowerInvariant();
            if (normalizedRequested != normalizedPending)
                throw new CustomerProfileValidationException(
                    "The supplied pending email does not match the current pending email.");
        }

        var options = deliveryOptions.Value;
        var expiresAt = DateTimeOffset.UtcNow.Add(options.TokenExpiry);
        var tokenRef = await tokenStore.CreateAsync(
            request.CustomerId, readModel.PendingEmail, expiresAt, cancellationToken);

        var idempotencyKey = $"customer-email-change:{request.CustomerId}:{request.CorrelationId}";
        var baseUrl = options.VerificationBaseUrl;
        var route = options.VerificationRoute;
        var verificationUrl = string.IsNullOrWhiteSpace(baseUrl)
            ? null
            : new Uri($"{baseUrl.TrimEnd('/')}{route}?tokenReference={Uri.EscapeDataString(tokenRef.TokenReference)}");

        var payload = new EmailVerificationDeliveryPayload(
            CustomerId: request.CustomerId,
            PendingEmail: readModel.PendingEmail,
            TokenReference: tokenRef.TokenReference,
            ExpiresAtUtc: expiresAt,
            VerificationUrl: verificationUrl,
            VerificationRoute: route,
            RouteValues: new Dictionary<string, string>
            {
                ["tokenReference"] = tokenRef.TokenReference
            },
            Purpose: "CustomerEmailChange",
            CorrelationId: request.CorrelationId,
            IdempotencyKey: idempotencyKey);

        try
        {
            await deliverySender.SendAsync(payload, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                "Email verification resend delivery failed for customer {CustomerId} [correlationId={CorrelationId}, failureType={FailureType}]",
                request.CustomerId,
                request.CorrelationId,
                ex.GetType().Name);

            // Delivery failure is non-fatal for resend; callers can retry
            return new EmailVerificationDeliveryResult(
                EmailVerificationStatus.PendingVerification,
                "We could not send the verification email at this time. Please try again shortly.",
                idempotencyKey);
        }

        return new EmailVerificationDeliveryResult(
            EmailVerificationStatus.PendingVerification,
            "Verification email sent. Continue signing in with your current email until verification is complete.",
            idempotencyKey);
    }
}
