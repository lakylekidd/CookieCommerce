using CookieCommerce.CustomerDomain.Application.Customers.Handlers;
using CookieCommerce.CustomerDomain.Application.Customers.Integration;
using CookieCommerce.CustomerDomain.Application.Customers.Profile;
using CookieCommerce.CustomerDomain.Application.Customers.ReadModels;
using CookieCommerce.CustomerDomain.Application.Security;
using CookieCommerce.CustomerDomain.Domain.Aggregates;
using CookieCommerce.CustomerDomain.Infrastructure.Integration;
using FluentAssertions;
using Marten;
using Marten.Events;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace CookieCommerce.CustomerDomain.Tests.IntegrationTests;

public class CustomerProfileOperationalLoggingTests
{
    [Fact]
    public async Task ExternalEmailVerificationSender_DoesNotLogPiiOrRawToken_GivenDeliveryHandoff()
    {
        var logger = new CapturingLogger<ExternalEmailVerificationSender>();
        var sender = new ExternalEmailVerificationSender(
            Substitute.For<IHttpClientFactory>(),
            Options.Create(new EmailVerificationDeliverySettings { ExternalDeliveryEndpoint = string.Empty }),
            logger);
        var payload = new EmailVerificationDeliveryPayload(
            Guid.NewGuid(),
            "jamie.new@example.com",
            "token-ref-not-raw-token",
            DateTimeOffset.UtcNow.AddHours(1),
            new Uri("https://web.example/account/email-verification/confirm?token=raw-secret-token"),
            "/account/email-verification/confirm",
            new Dictionary<string, string> { ["token"] = "raw-secret-token" },
            "CustomerEmailChange",
            "corr-logging",
            "idempotency-key");

        await sender.SendAsync(payload, CancellationToken.None);

        logger.Messages.Should().ContainSingle();
        AssertNoSensitiveValues(logger.Messages);
    }

    [Fact]
    public async Task VerifyCustomerEmailChangeHandler_DoesNotLogPiiOrRawToken_GivenSuccessfulVerification()
    {
        var customerId = Guid.NewGuid();
        var logger = new CapturingLogger<VerifyCustomerEmailChangeCommandHandler>();
        var session = Substitute.For<IDocumentSession>();
        var eventStore = Substitute.For<IEventStoreOperations>();
        var querySession = Substitute.For<IQuerySession>();
        var keyStore = Substitute.For<ICustomerKeyStore>();
        var tokenStore = Substitute.For<IEmailVerificationTokenStore>();
        var outboxPublisher = Substitute.For<ICustomerProfileOutboxPublisher>();
        var aggregate = new CustomerAggregate();
        aggregate.Create(customerId, "enc-email", "enc-first", "enc-last", "enc-gender", "enc-dob");
        aggregate.ClearUncommitedEvents();
        aggregate.RequestEmailChange("enc-new-email", "token-ref", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddHours(1));
        aggregate.ClearUncommitedEvents();
        session.Events.Returns(eventStore);
        eventStore.AggregateStreamAsync<CustomerAggregate>(customerId, token: Arg.Any<CancellationToken>()).Returns(aggregate);
        querySession.LoadAsync<CustomerReadModel>(customerId, Arg.Any<CancellationToken>()).Returns(new CustomerReadModel
        {
            Id = customerId,
            Email = "jamie@example.com",
            PendingEmail = "jamie.new@example.com",
            FirstName = "Jamie",
            LastName = "Baker",
            EmailVerificationStatus = "PendingVerification",
            Addresses = []
        });
        keyStore.GetKeyAsync(customerId, Arg.Any<CancellationToken>()).Returns(new byte[] { 1, 2, 3 });
        tokenStore.FindAsync(customerId, "raw-secret-token", Arg.Any<CancellationToken>())
            .Returns(new EmailVerificationTokenReference(customerId, "token-ref", "hashed", DateTimeOffset.UtcNow.AddHours(1), DateTimeOffset.UtcNow, null));
        var handler = new VerifyCustomerEmailChangeCommandHandler(session, querySession, keyStore, tokenStore, outboxPublisher, logger);

        await handler.Handle(new VerifyCustomerEmailChangeCommand(customerId, "raw-secret-token", DateTimeOffset.UtcNow, "corr-logging"), CancellationToken.None);

        logger.Messages.Should().ContainSingle();
        AssertNoSensitiveValues(logger.Messages);
    }

    private static void AssertNoSensitiveValues(IEnumerable<string> messages)
    {
        var combined = string.Join(Environment.NewLine, messages);
        combined.Should().NotContain("Jamie");
        combined.Should().NotContain("Baker");
        combined.Should().NotContain("jamie@example.com");
        combined.Should().NotContain("jamie.new@example.com");
        combined.Should().NotContain("1990-05-12");
        combined.Should().NotContain("Female");
        combined.Should().NotContain("raw-secret-token");
    }

    private sealed class CapturingLogger<T> : ILogger<T>
    {
        public List<string> Messages { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Messages.Add(formatter(state, exception));
        }
    }
}
