using CookieCommerce.CustomerDomain.Application.Customers.Integration;
using CookieCommerce.CustomerDomain.Infrastructure.Integration;
using CookieCommerce.DomainBuildingBlocks;
using FluentAssertions;
using NSubstitute;

namespace CookieCommerce.CustomerDomain.Tests.IntegrationTests;

public class CustomerProfileEmailVerificationOutboxTests
{
    [Fact]
    public async Task PublishVerifiedEmailChangedAsync_PublishesEmailVerificationIntegrationEvent()
    {
        var publisher = Substitute.For<IIntegrationEventPublisher>();
        VerifiedCustomerEmailChangedIntegrationEvent? captured = null;
        publisher.PublishAsync(Arg.Do<VerifiedCustomerEmailChangedIntegrationEvent>(e => captured = e), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        var outbox = new CustomerProfileOutboxPublisher(publisher);
        var customerId = Guid.NewGuid();
        var occurredAt = DateTimeOffset.UtcNow;

        await outbox.PublishVerifiedEmailChangedAsync(
            customerId,
            "old@example.com",
            "new@example.com",
            occurredAt,
            "corr-outbox",
            CancellationToken.None);

        captured.Should().NotBeNull();
        captured!.CustomerId.Should().Be(customerId);
        captured.PreviousEmail.Should().Be("old@example.com");
        captured.VerifiedEmail.Should().Be("new@example.com");
        captured.OccurredAtUtc.Should().Be(occurredAt);
        captured.CorrelationId.Should().Be("corr-outbox");
        captured.EventId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task PublishCustomerForgottenAsync_PublishesPiiFreeForgottenIntegrationEvent()
    {
        var publisher = Substitute.For<IIntegrationEventPublisher>();
        CustomerForgottenIntegrationEvent? captured = null;
        publisher.PublishAsync(Arg.Do<CustomerForgottenIntegrationEvent>(e => captured = e), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        var outbox = new CustomerForgottenOutboxPublisher(publisher);
        var customerId = Guid.NewGuid();
        var occurredAt = DateTimeOffset.UtcNow;

        await outbox.PublishCustomerForgottenAsync(
            customerId,
            "subject-123",
            "CustomerForgotten",
            occurredAt,
            "corr-forgotten",
            CancellationToken.None);

        captured.Should().NotBeNull();
        captured!.CustomerId.Should().Be(customerId);
        captured.IdentitySubject.Should().Be("subject-123");
        captured.Reason.Should().Be("CustomerForgotten");
        captured.CorrelationId.Should().Be("corr-forgotten");
        captured.ToString().Should().NotContain("old@example.com");
        captured.ToString().Should().NotContain("Jamie");
    }
}
