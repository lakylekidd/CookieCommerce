using CookieCommerce.CustomerDomain.Application.Customers.Handlers;
using CookieCommerce.CustomerDomain.Application.Customers.Integration;
using CookieCommerce.CustomerDomain.Application.Customers.Profile;
using CookieCommerce.CustomerDomain.Application.Customers.ReadModels;
using CookieCommerce.CustomerDomain.Application.Security;
using CookieCommerce.CustomerDomain.Domain.Aggregates;
using CookieCommerce.CustomerDomain.Domain.Exceptions;
using FluentAssertions;
using Marten;
using Marten.Events;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace CookieCommerce.CustomerDomain.Tests.ApplicationTests;

public class VerifyCustomerEmailChangeTests
{
    [Fact]
    public async Task Handle_PromotesPendingEmailAndPublishesOutboxEvent_GivenValidToken()
    {
        var customerId = Guid.NewGuid();
        var verifiedAt = DateTimeOffset.UtcNow;
        var fixture = BuildFixture(customerId, verifiedAt.AddHours(1), tokenReference: "token-ref");

        var result = await fixture.Handler.Handle(
            new VerifyCustomerEmailChangeCommand(customerId, "raw-token", verifiedAt, "corr-verify"),
            CancellationToken.None);

        result.Email.Should().Be("jamie.new@example.com");
        result.PendingEmail.Should().BeNull();
        result.EmailVerificationStatus.Should().Be(EmailVerificationStatus.Verified);
        fixture.Aggregate.EncryptedEmail.Should().Be("enc-new-email");
        fixture.Aggregate.EncryptedPendingEmail.Should().BeNull();
        fixture.Aggregate.VerificationTokenReference.Should().BeNull();
        await fixture.TokenStore.Received(1).MarkConsumedAsync(customerId, "token-ref", verifiedAt, Arg.Any<CancellationToken>());
        await fixture.OutboxPublisher.Received(1).PublishVerifiedEmailChangedAsync(
            customerId,
            "jamie@example.com",
            "jamie.new@example.com",
            verifiedAt,
            "corr-verify",
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ThrowsInvalidToken_GivenUnknownToken()
    {
        var customerId = Guid.NewGuid();
        var fixture = BuildFixture(customerId, DateTimeOffset.UtcNow.AddHours(1), tokenReference: "token-ref");
        fixture.TokenStore.FindAsync(customerId, "bad-token", Arg.Any<CancellationToken>())
            .Returns((EmailVerificationTokenReference?)null);

        Func<Task> act = () => fixture.Handler.Handle(
            new VerifyCustomerEmailChangeCommand(customerId, "bad-token", DateTimeOffset.UtcNow, "corr"),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidEmailVerificationTokenException>();
        await fixture.OutboxPublisher.DidNotReceive().PublishVerifiedEmailChangedAsync(
            Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<DateTimeOffset>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ThrowsExpiredToken_GivenExpiredToken()
    {
        var customerId = Guid.NewGuid();
        var fixture = BuildFixture(customerId, DateTimeOffset.UtcNow.AddMinutes(-1), tokenReference: "token-ref");

        Func<Task> act = () => fixture.Handler.Handle(
            new VerifyCustomerEmailChangeCommand(customerId, "raw-token", DateTimeOffset.UtcNow, "corr"),
            CancellationToken.None);

        await act.Should().ThrowAsync<ExpiredEmailVerificationTokenException>();
    }

    [Fact]
    public async Task Handle_ThrowsInvalidToken_GivenMismatchedAggregateTokenReference()
    {
        var customerId = Guid.NewGuid();
        var fixture = BuildFixture(customerId, DateTimeOffset.UtcNow.AddHours(1), tokenReference: "stored-token-ref");
        fixture.TokenStore.FindAsync(customerId, "raw-token", Arg.Any<CancellationToken>())
            .Returns(new EmailVerificationTokenReference(customerId, "different-token-ref", "hashed", DateTimeOffset.UtcNow.AddHours(1), DateTimeOffset.UtcNow, null));

        Func<Task> act = () => fixture.Handler.Handle(
            new VerifyCustomerEmailChangeCommand(customerId, "raw-token", DateTimeOffset.UtcNow, "corr"),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidEmailVerificationTokenException>();
    }

    private static VerificationFixture BuildFixture(Guid customerId, DateTimeOffset tokenExpiresAt, string tokenReference)
    {
        var session = Substitute.For<IDocumentSession>();
        var eventStore = Substitute.For<IEventStoreOperations>();
        var querySession = Substitute.For<IQuerySession>();
        var keyStore = Substitute.For<ICustomerKeyStore>();
        var tokenStore = Substitute.For<IEmailVerificationTokenStore>();
        var outboxPublisher = Substitute.For<ICustomerProfileOutboxPublisher>();
        var key = new byte[] { 1, 2, 3 };
        var aggregate = new CustomerAggregate();
        aggregate.Create(customerId, "enc-old-email", "enc-first", "enc-last", "enc-gender", null);
        aggregate.ClearUncommitedEvents();
        aggregate.RequestEmailChange("enc-new-email", tokenReference, DateTimeOffset.UtcNow.AddMinutes(-5), DateTimeOffset.UtcNow.AddHours(1));
        aggregate.ClearUncommitedEvents();

        session.Events.Returns(eventStore);
        eventStore.AggregateStreamAsync<CustomerAggregate>(customerId, token: Arg.Any<CancellationToken>()).Returns(aggregate);
        querySession.LoadAsync<CustomerReadModel>(customerId, Arg.Any<CancellationToken>()).Returns(new CustomerReadModel
        {
            Id = customerId,
            Email = "jamie@example.com",
            PendingEmail = "jamie.new@example.com",
            EmailVerificationStatus = "PendingVerification",
            FirstName = "Jamie",
            LastName = "Baker",
            Addresses = []
        });
        keyStore.GetKeyAsync(customerId, Arg.Any<CancellationToken>()).Returns(key);
        tokenStore.FindAsync(customerId, "raw-token", Arg.Any<CancellationToken>())
            .Returns(new EmailVerificationTokenReference(customerId, tokenReference, "hashed", tokenExpiresAt, DateTimeOffset.UtcNow, null));

        var handler = new VerifyCustomerEmailChangeCommandHandler(
            session,
            querySession,
            keyStore,
            tokenStore,
            outboxPublisher,
            NullLogger<VerifyCustomerEmailChangeCommandHandler>.Instance);

        return new VerificationFixture(handler, aggregate, tokenStore, outboxPublisher);
    }

    private sealed record VerificationFixture(
        VerifyCustomerEmailChangeCommandHandler Handler,
        CustomerAggregate Aggregate,
        IEmailVerificationTokenStore TokenStore,
        ICustomerProfileOutboxPublisher OutboxPublisher);
}
