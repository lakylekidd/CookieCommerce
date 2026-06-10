using CookieCommerce.CustomerDomain.Application.Customers.Commands;
using CookieCommerce.CustomerDomain.Application.Customers.Handlers;
using CookieCommerce.CustomerDomain.Application.Customers.Integration;
using CookieCommerce.CustomerDomain.Application.Security;
using CookieCommerce.CustomerDomain.Domain.Aggregates;
using CookieCommerce.CustomerDomain.Domain.DomainEvents;
using CookieCommerce.DomainBuildingBlocks;
using Marten;
using Marten.Events;
using NSubstitute;
using System.Globalization;

namespace CookieCommerce.CustomerDomain.Tests.HandlerTests;

public class CreateCustomerHandlerTests
{
    [Fact]
    public async Task Handle_ShouldEncryptAndAppendEvents()
    {
        // Arrange
        var session = Substitute.For<IDocumentSession>();
        var eventStore = Substitute.For<IEventStoreOperations>();
        var keyStore = Substitute.For<ICustomerKeyStore>();
        var encryption = Substitute.For<IEncryptionService>();
        var publisher = Substitute.For<IIntegrationEventPublisher>();

        session.Events.Returns(eventStore);

        keyStore.CreateKeyForCustomerAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new byte[] { 1, 2, 3 }));

        encryption.Encrypt("test@example.com", Arg.Any<byte[]>()).Returns("encrypted-email");
        encryption.Encrypt("John", Arg.Any<byte[]>()).Returns("encrypted-first-name");
        encryption.Encrypt("Doe", Arg.Any<byte[]>()).Returns("encrypted-last-name");
        encryption.Encrypt("Male", Arg.Any<byte[]>()).Returns("encrypted-gender");
        // Handler formats dates using the round-trip format specifier ("O")
        encryption.Encrypt(DateTime.ParseExact(
            "18/11/1986",
            "dd/MM/yyyy",
            CultureInfo.InvariantCulture).ToString("O"), Arg.Any<byte[]>()).Returns("encrypted-date-of-birth");

        var handler = new CreateCustomerHandler(session, keyStore, encryption, publisher);

        // Act
        var id = await handler.Handle(
            new CreateCustomer(
                "test@example.com", 
                "John", 
                "Doe", 
                Domain.ValueObjects.Gender.Male, 
                DateTime.Parse("18/11/1986")), 
            default);

        // Assert
        eventStore.Received().StartStream<CustomerAggregate>(
            id,
            Arg.Is<IEnumerable<object>>(e => e.OfType<CustomerCreated>().Any()));
        await session.Received().SaveChangesAsync(Arg.Any<CancellationToken>());
        await publisher.Received().PublishAsync(Arg.Any<CustomerSnapshotPublished>(), Arg.Any<CancellationToken>());
    }
}
