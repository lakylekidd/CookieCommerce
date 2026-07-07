using CookieCommerce.CustomerDomain.Application.Customers.Handlers;
using CookieCommerce.CustomerDomain.Application.Customers.Integration;
using CookieCommerce.CustomerDomain.Application.Customers.Profile;
using CookieCommerce.CustomerDomain.Application.Customers.ReadModels;
using CookieCommerce.CustomerDomain.Domain.Exceptions;
using FluentAssertions;
using Marten;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace CookieCommerce.CustomerDomain.Tests.ApplicationTests;

public class EmailVerificationDeliveryHandoffTests
{
    private static ResendCustomerEmailVerificationCommandHandler BuildResendHandler(
        IQuerySession querySession,
        IEmailVerificationTokenStore tokenStore,
        IEmailVerificationDeliverySender deliverySender,
        EmailVerificationDeliverySettings? settings = null)
    {
        var options = Options.Create(settings ?? new EmailVerificationDeliverySettings
        {
            VerificationBaseUrl = "https://cookie.example",
            VerificationRoute = "/account/email-verification/confirm",
            TokenExpiry = TimeSpan.FromHours(24),
            MaxRetryAttempts = 3
        });

        return new ResendCustomerEmailVerificationCommandHandler(
            querySession,
            tokenStore,
            deliverySender,
            options,
            NullLogger<ResendCustomerEmailVerificationCommandHandler>.Instance);
    }

    [Fact]
    public async Task Resend_ShouldCallDeliverySender_WithCorrectPayloadFields()
    {
        var customerId = Guid.NewGuid();
        var pendingEmail = "jamie.new@example.com";

        var querySession = Substitute.For<IQuerySession>();
        var tokenStore = Substitute.For<IEmailVerificationTokenStore>();
        var deliverySender = Substitute.For<IEmailVerificationDeliverySender>();

        querySession.LoadAsync<CustomerReadModel>(customerId, Arg.Any<CancellationToken>())
            .Returns(new CustomerReadModel
            {
                Id = customerId,
                Email = "jamie@example.com",
                PendingEmail = pendingEmail,
                EmailVerificationStatus = "PendingVerification",
                FirstName = "Jamie",
                LastName = "Baker"
            });

        var expiresAt = DateTimeOffset.UtcNow.AddHours(24);
        var tokenRef = new EmailVerificationTokenReference(
            customerId, "evt_token-ref-001", "hashed-token", expiresAt, DateTimeOffset.UtcNow, null);

        tokenStore.CreateAsync(customerId, pendingEmail, Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns(tokenRef);

        EmailVerificationDeliveryPayload? capturedPayload = null;
        await deliverySender.SendAsync(Arg.Do<EmailVerificationDeliveryPayload>(p => capturedPayload = p), Arg.Any<CancellationToken>());

        var handler = BuildResendHandler(querySession, tokenStore, deliverySender);
        var command = new ResendCustomerEmailVerificationCommand(
            CustomerId: customerId,
            PendingEmail: pendingEmail,
            RequestedAtUtc: DateTimeOffset.UtcNow,
            CorrelationId: "corr-resend-001");

        var result = await handler.Handle(command, default);

        // Status
        result.Status.Should().Be(EmailVerificationStatus.PendingVerification);
        result.Message.Should().Contain("Verification email sent");
        result.IdempotencyKey.Should().Contain(customerId.ToString());
        result.IdempotencyKey.Should().Contain("corr-resend-001");

        // Payload fields
        capturedPayload.Should().NotBeNull();
        capturedPayload!.CustomerId.Should().Be(customerId);
        capturedPayload.PendingEmail.Should().Be(pendingEmail);
        capturedPayload.TokenReference.Should().Be("evt_token-ref-001");
        capturedPayload.Purpose.Should().Be("CustomerEmailChange");
        capturedPayload.CorrelationId.Should().Be("corr-resend-001");
        capturedPayload.IdempotencyKey.Should().Contain("corr-resend-001");
        capturedPayload.ExpiresAtUtc.Should().BeCloseTo(expiresAt, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task Resend_ShouldIncludeVerificationUrl_WhenBaseUrlConfigured()
    {
        var customerId = Guid.NewGuid();

        var querySession = Substitute.For<IQuerySession>();
        var tokenStore = Substitute.For<IEmailVerificationTokenStore>();
        var deliverySender = Substitute.For<IEmailVerificationDeliverySender>();

        querySession.LoadAsync<CustomerReadModel>(customerId, Arg.Any<CancellationToken>())
            .Returns(new CustomerReadModel
            {
                Id = customerId,
                Email = "jamie@example.com",
                PendingEmail = "new@example.com",
                EmailVerificationStatus = "PendingVerification",
                FirstName = "J",
                LastName = "B"
            });

        tokenStore.CreateAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns(new EmailVerificationTokenReference(
                customerId, "evt_ref-123", "hashed", DateTimeOffset.UtcNow.AddHours(24), DateTimeOffset.UtcNow, null));

        EmailVerificationDeliveryPayload? capturedPayload = null;
        await deliverySender.SendAsync(Arg.Do<EmailVerificationDeliveryPayload>(p => capturedPayload = p), Arg.Any<CancellationToken>());

        var handler = BuildResendHandler(querySession, tokenStore, deliverySender, new EmailVerificationDeliverySettings
        {
            VerificationBaseUrl = "https://cookie.example",
            VerificationRoute = "/account/email-verification/confirm",
            TokenExpiry = TimeSpan.FromHours(24)
        });

        await handler.Handle(new ResendCustomerEmailVerificationCommand(
            customerId, null, DateTimeOffset.UtcNow, "corr-url-001"), default);

        capturedPayload!.VerificationUrl.Should().NotBeNull();
        capturedPayload.VerificationUrl!.ToString().Should().Contain("/account/email-verification/confirm");
        capturedPayload.VerificationUrl.ToString().Should().Contain("evt_ref-123");
    }

    [Fact]
    public async Task Resend_ShouldThrow_WhenNoPendingEmailExists()
    {
        var customerId = Guid.NewGuid();
        var querySession = Substitute.For<IQuerySession>();

        querySession.LoadAsync<CustomerReadModel>(customerId, Arg.Any<CancellationToken>())
            .Returns(new CustomerReadModel
            {
                Id = customerId,
                Email = "jamie@example.com",
                PendingEmail = null,   // no pending email
                EmailVerificationStatus = "Verified",
                FirstName = "J",
                LastName = "B"
            });

        var handler = BuildResendHandler(querySession, Substitute.For<IEmailVerificationTokenStore>(), Substitute.For<IEmailVerificationDeliverySender>());

        Func<Task> act = () => handler.Handle(
            new ResendCustomerEmailVerificationCommand(customerId, null, DateTimeOffset.UtcNow, "c"), default);

        await act.Should().ThrowAsync<CustomerProfileValidationException>();
    }

    [Fact]
    public async Task Resend_IdempotencyKey_ShouldBeStableForSameCorrelation()
    {
        var customerId = Guid.NewGuid();
        var correlationId = "fixed-corr-id";

        var querySession = Substitute.For<IQuerySession>();
        var tokenStore = Substitute.For<IEmailVerificationTokenStore>();
        var deliverySender = Substitute.For<IEmailVerificationDeliverySender>();

        querySession.LoadAsync<CustomerReadModel>(customerId, Arg.Any<CancellationToken>())
            .Returns(new CustomerReadModel
            {
                Id = customerId,
                Email = "jamie@example.com",
                PendingEmail = "new@example.com",
                EmailVerificationStatus = "PendingVerification",
                FirstName = "J",
                LastName = "B"
            });

        tokenStore.CreateAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns(new EmailVerificationTokenReference(
                customerId, "ref", "hash", DateTimeOffset.UtcNow.AddHours(24), DateTimeOffset.UtcNow, null));

        var handler = BuildResendHandler(querySession, tokenStore, deliverySender);

        var result1 = await handler.Handle(
            new ResendCustomerEmailVerificationCommand(customerId, null, DateTimeOffset.UtcNow, correlationId), default);
        var result2 = await handler.Handle(
            new ResendCustomerEmailVerificationCommand(customerId, null, DateTimeOffset.UtcNow, correlationId), default);

        result1.IdempotencyKey.Should().Be(result2.IdempotencyKey);
    }
}
