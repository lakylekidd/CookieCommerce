using CookieCommerce.CustomerDomain.Application.Customers.Integration;
using CookieCommerce.CustomerDomain.Application.Customers.Profile;
using CookieCommerce.CustomerDomain.Application.Customers.ReadModels;
using CookieCommerce.CustomerDomain.Application.Security;
using CookieCommerce.CustomerDomain.Domain.Aggregates;
using CookieCommerce.CustomerDomain.Domain.Exceptions;
using Marten;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CookieCommerce.CustomerDomain.Application.Customers.Handlers;

public sealed class VerifyCustomerEmailChangeCommandHandler(
    IDocumentSession session,
    IQuerySession querySession,
    ICustomerKeyStore keyStore,
    IEmailVerificationTokenStore tokenStore,
    ICustomerProfileOutboxPublisher outboxPublisher,
    ILogger<VerifyCustomerEmailChangeCommandHandler> logger)
    : IRequestHandler<VerifyCustomerEmailChangeCommand, CustomerProfileResult>
{
    public async Task<CustomerProfileResult> Handle(
        VerifyCustomerEmailChangeCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
            throw new InvalidEmailVerificationTokenException();

        var readModel = await querySession.LoadAsync<CustomerReadModel>(request.CustomerId, cancellationToken);
        if (readModel is null)
            throw new MissingCustomerProfileException(request.CustomerId);

        if (string.IsNullOrWhiteSpace(readModel.PendingEmail))
            throw new CustomerProfileValidationException("No pending email change exists for this customer.");

        var token = await tokenStore.FindAsync(request.CustomerId, request.Token, cancellationToken);
        if (token is null)
            throw new InvalidEmailVerificationTokenException();

        if (token.ExpiresAtUtc <= request.VerifiedAtUtc)
            throw new ExpiredEmailVerificationTokenException();

        var key = await keyStore.GetKeyAsync(request.CustomerId, cancellationToken);
        if (key is null)
            throw new MissingCustomerProfileException(request.CustomerId);

        var customer = await session.Events.AggregateStreamAsync<CustomerAggregate>(
            request.CustomerId,
            token: cancellationToken);
        if (customer is null)
            throw new MissingCustomerProfileException(request.CustomerId);

        if (customer.VerificationTokenReference is not null
            && !string.Equals(customer.VerificationTokenReference, token.TokenReference, StringComparison.Ordinal))
        {
            throw new InvalidEmailVerificationTokenException();
        }

        var previousEmail = readModel.Email;
        var verifiedEmail = readModel.PendingEmail;

        customer.VerifyPendingEmailChange(request.VerifiedAtUtc);
        session.Events.Append(request.CustomerId, customer.DomainEvents);

        await tokenStore.MarkConsumedAsync(
            request.CustomerId,
            token.TokenReference,
            request.VerifiedAtUtc,
            cancellationToken);

        await session.SaveChangesAsync(cancellationToken);

        await outboxPublisher.PublishVerifiedEmailChangedAsync(
            request.CustomerId,
            previousEmail,
            verifiedEmail,
            request.VerifiedAtUtc,
            request.CorrelationId,
            cancellationToken);

        logger.LogInformation(
            "Verified customer email change for customer {CustomerId} [correlationId={CorrelationId}]",
            request.CustomerId,
            request.CorrelationId);

        var result = new CustomerProfileResult(
            readModel.Id,
            readModel.FirstName,
            readModel.LastName,
            verifiedEmail,
            PendingEmail: null,
            EmailVerificationStatus.Verified,
            readModel.Gender,
            readModel.DateOfBirth);

        return result;
    }
}
