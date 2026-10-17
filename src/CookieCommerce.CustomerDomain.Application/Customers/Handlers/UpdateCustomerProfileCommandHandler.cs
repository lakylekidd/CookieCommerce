using CookieCommerce.CustomerDomain.Application.Customers.Integration;
using CookieCommerce.CustomerDomain.Application.Customers.Profile;
using CookieCommerce.CustomerDomain.Application.Customers.ReadModels;
using CookieCommerce.CustomerDomain.Application.Security;
using CookieCommerce.CustomerDomain.Domain.Aggregates;
using CookieCommerce.CustomerDomain.Domain.Exceptions;
using Marten;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CookieCommerce.CustomerDomain.Application.Customers.Handlers;

public class UpdateCustomerProfileCommandHandler(
    IDocumentSession session,
    IQuerySession querySession,
    ICustomerKeyStore keyStore,
    CustomerProfileEncryption profileEncryption,
    CustomerProfileValidator profileValidator,
    CustomerEmailUniquenessService emailUniquenessService,
    IEmailVerificationTokenStore tokenStore,
    IEmailVerificationDeliverySender deliverySender,
    ICustomerProfileOutboxPublisher profileOutboxPublisher,
    IOptions<EmailVerificationDeliverySettings> deliveryOptions,
    ILogger<UpdateCustomerProfileCommandHandler> logger)
    : IRequestHandler<UpdateCustomerProfileCommand, CustomerProfileResult>
{
    public async Task<CustomerProfileResult> Handle(
        UpdateCustomerProfileCommand request,
        CancellationToken cancellationToken)
    {
        // ── Validate plaintext fields before any aggregate/persistence work ──
        var validationResult = profileValidator.Validate(
            request.FirstName,
            request.LastName,
            request.Email,
            request.Gender,
            request.DateOfBirth);

        if (!validationResult.IsValid)
            throw new CustomerProfileValidationException(validationResult.Errors);

        var readModel = await querySession.LoadAsync<CustomerReadModel>(request.CustomerId, cancellationToken);
        if (readModel is null)
            throw new MissingCustomerProfileException(request.CustomerId);

        var key = await keyStore.GetKeyAsync(request.CustomerId, cancellationToken);
        if (key is null)
            throw new MissingCustomerProfileException(request.CustomerId);

        var customer = await session.Events.AggregateStreamAsync<CustomerAggregate>(
            request.CustomerId, token: cancellationToken);
        if (customer is null)
            throw new MissingCustomerProfileException(request.CustomerId);

        customer.ClearUncommitedEvents();

        // Encrypt non-email profile fields
        var encrypted = profileEncryption.Encrypt(
            key,
            request.FirstName,
            request.LastName,
            readModel.Email,          // current verified email unchanged
            readModel.PendingEmail,   // pending email unchanged by this path
            request.Gender,
            request.DateOfBirth);

        // Apply profile update event (name, gender, DOB)
        customer.UpdateProfile(
            encrypted.EncryptedFirstName,
            encrypted.EncryptedLastName,
            encrypted.EncryptedGender,
            encrypted.EncryptedDateOfBirth);

        var normalizedRequestedEmail = request.Email.Trim().ToLowerInvariant();
        var normalizedCurrentEmail = readModel.Email.Trim().ToLowerInvariant();
        var emailChanged = normalizedRequestedEmail != normalizedCurrentEmail;

        if (emailChanged)
        {
            // Check uniqueness before creating any pending state (Foundation/US1 contract)
            await emailUniquenessService.EnsureEmailAvailableForPendingChangeAsync(
                request.Email, request.CustomerId, cancellationToken);

            // Encrypt the requested (pending) email
            var encryptedPendingEmail = profileEncryption.Encrypt(
                key,
                request.FirstName,
                request.LastName,
                readModel.Email,
                request.Email,  // pending
                request.Gender,
                request.DateOfBirth).EncryptedPendingEmail!;

            var expiresAt = DateTimeOffset.UtcNow.Add(deliveryOptions.Value.TokenExpiry);
            var tokenRef = await tokenStore.CreateAsync(
                request.CustomerId, request.Email, expiresAt, cancellationToken);

            customer.RequestEmailChange(
                encryptedPendingEmail,
                tokenRef.TokenReference,
                request.RequestedAtUtc,
                expiresAt);

            // Handoff to external delivery — no raw token or PII in logs
            var idempotencyKey = $"customer-email-change:{request.CustomerId}:{request.CorrelationId}";
            var baseUrl = deliveryOptions.Value.VerificationBaseUrl;
            var route = deliveryOptions.Value.VerificationRoute;
            var verificationUrl = string.IsNullOrWhiteSpace(baseUrl)
                ? null
                : new Uri($"{baseUrl.TrimEnd('/')}{route}?tokenReference={Uri.EscapeDataString(tokenRef.TokenReference)}");

            var payload = new EmailVerificationDeliveryPayload(
                CustomerId: request.CustomerId,
                PendingEmail: request.Email,
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
                // Log without PII — delivery failure must not roll back profile update
                logger.LogWarning(ex,
                    "Email verification delivery handoff failed for customer {CustomerId} [correlationId={CorrelationId}]",
                    request.CustomerId,
                    request.CorrelationId);
            }
        }

        session.Events.Append(request.CustomerId, customer.DomainEvents);
        await session.SaveChangesAsync(cancellationToken);

        await profileOutboxPublisher.PublishProfileUpdatedAsync(
            request.CustomerId,
            request.FirstName,
            request.LastName,
            request.Gender?.ToString(),
            request.DateOfBirth,
            request.RequestedAtUtc,
            request.CorrelationId,
            cancellationToken);

        // Reload fresh read model after projection update
        var updatedModel = await querySession.LoadAsync<CustomerReadModel>(request.CustomerId, cancellationToken)
                           ?? readModel;

        return GetCurrentCustomerProfileQueryHandler.ToResult(updatedModel);
    }
}
