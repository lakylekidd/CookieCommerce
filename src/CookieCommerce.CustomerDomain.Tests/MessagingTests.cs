using CookieCommerce.CustomerDomain.Application.Customers.Commands;
using CookieCommerce.CustomerDomain.Application.Customers.Handlers;
using CookieCommerce.CustomerDomain.Application.Customers.Integration;
using CookieCommerce.CustomerDomain.Application.Security;
using CookieCommerce.CustomerDomain.Domain.Aggregates;
using FluentAssertions;
using Marten;
using Marten.Events;
using NSubstitute;

namespace CookieCommerce.CustomerDomain.Tests;

public class MessagingTests
{
    [Fact]
    public async Task ForgetCustomer_ShouldPublishIntegrationEvent()
    {
        // Arrange
        var forgottenOutboxPublisher = Substitute.For<ICustomerForgottenOutboxPublisher>();
        var session = Substitute.For<IDocumentSession>();
        var eventStore = Substitute.For<IEventStoreOperations>();
        var keyStore = Substitute.For<ICustomerKeyStore>();
        var customerId = Guid.NewGuid();
        var aggregate = new CustomerAggregate();
        aggregate.Create(customerId, "enc-email", "enc-first", "enc-last", "enc-gender", null);
        aggregate.ClearUncommitedEvents();
        session.Events.Returns(eventStore);
        eventStore.AggregateStreamAsync<CustomerAggregate>(customerId, token: Arg.Any<CancellationToken>())
            .Returns(aggregate);
        var handler = new ForgetCustomerHandler(session, keyStore, forgottenOutboxPublisher);

        // Act
        await handler.Handle(new ForgetCustomer(customerId), default);

        // Assert
        aggregate.ForgottenDateUtc.Should().NotBeNull();
        await forgottenOutboxPublisher.Received().PublishCustomerForgottenAsync(
            customerId,
            Arg.Any<string?>(),
            "CustomerForgotten",
            Arg.Any<DateTimeOffset>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
    }
}
