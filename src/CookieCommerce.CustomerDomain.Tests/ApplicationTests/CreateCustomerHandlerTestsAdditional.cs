using CookieCommerce.CustomerDomain.Application.Customers.Commands;
using CookieCommerce.CustomerDomain.Application.Customers.Handlers;
using CookieCommerce.CustomerDomain.Application.Security;
using CookieCommerce.CustomerDomain.Domain.Aggregates;
using NSubstitute;
using Marten;
using FluentAssertions;
using CookieCommerce.DomainBuildingBlocks;
using Marten.Events;

namespace CookieCommerce.CustomerDomain.Tests.ApplicationTests;

public class CreateCustomerHandlerTestsAdditional
{
    [Fact]
    public async Task Handle_ShouldReturnGuid_And_StartStream()
    {
        var session = Substitute.For<IDocumentSession>();
        var eventStore = Substitute.For<IEventStoreOperations>();
        var keyStore = Substitute.For<ICustomerKeyStore>();
        var encryption = Substitute.For<IEncryptionService>();
        var publisher = Substitute.For<IIntegrationEventPublisher>();

        session.Events.Returns(eventStore);

        keyStore.CreateKeyForCustomerAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(new byte[] { 1 }));
        encryption.Encrypt(Arg.Any<string>(), Arg.Any<byte[]>()).Returns("enc");

        var handler = new CreateCustomerHandler(session, keyStore, encryption, publisher);

        var id = await handler.Handle(new CreateCustomer("a@b.com", "f", "l", Domain.ValueObjects.Gender.Male, null), default);

        id.Should().NotBeEmpty();
        eventStore.Received().StartStream<CustomerAggregate>(Arg.Any<Guid>(), Arg.Any<IEnumerable<object>>());
    }
}
