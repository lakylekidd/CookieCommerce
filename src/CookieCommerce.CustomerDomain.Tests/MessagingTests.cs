using CookieCommerce.CustomerDomain.Application.Customers.Commands;
using CookieCommerce.CustomerDomain.Application.Customers.Handlers;
using CookieCommerce.CustomerDomain.Application.Customers.Integration;
using CookieCommerce.CustomerDomain.Application.Security;
using CookieCommerce.CustomerDomain.Domain.Aggregates;
using CookieCommerce.DomainBuildingBlocks;
using Marten;
using NSubstitute;

namespace CookieCommerce.CustomerDomain.Tests;

public class MessagingTests
{
    [Fact]
    public async Task ForgetCustomer_ShouldPublishIntegrationEvent()
    {
        // Arrange
        var publisher = Substitute.For<IIntegrationEventPublisher>();
        var session = Substitute.For<IDocumentSession>();
        var keyStore = Substitute.For<ICustomerKeyStore>();

        var handler = new ForgetCustomerHandler(session, keyStore, publisher);

        keyStore.CreateKeyForCustomerAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new byte[] { 1, 2, 3 }));

        session.Events
            .AggregateStreamAsync<CustomerAggregate>(Arg.Any<Guid>(), token: Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new CustomerAggregate()));

        // Act
        await handler.Handle(new ForgetCustomer(Guid.NewGuid()), default);

        // Assert
        await publisher.Received().PublishAsync(
            Arg.Any<CustomerDeletedIntegrationEvent>(),
            Arg.Any<CancellationToken>());
    }
}
