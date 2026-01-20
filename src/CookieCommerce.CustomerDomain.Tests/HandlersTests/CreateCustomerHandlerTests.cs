using CookieCommerce.CustomerDomain.Application.Customers.Commands;
using CookieCommerce.CustomerDomain.Application.Customers.Handlers;
using CookieCommerce.CustomerDomain.Application.Security;
using CookieCommerce.CustomerDomain.Domain.Aggregates;
using CookieCommerce.CustomerDomain.Domain.DomainEvents;
using Marten;
using NSubstitute;

namespace CookieCommerce.CustomerDomain.Tests.HandlerTests;

public class CreateCustomerHandlerTests
{
    [Fact]
    public async Task Handle_ShouldEncryptAndAppendEvents()
    {
        // Arrange
        var session = Substitute.For<IDocumentSession>();
        var keyStore = Substitute.For<ICustomerKeyStore>();
        var encryption = Substitute.For<IEncryptionService>();

        keyStore.CreateKeyForCustomerAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new byte[] { 1, 2, 3 }));

        encryption.Encrypt("test@example.com", Arg.Any<byte[]>()).Returns("encrypted-email");
        encryption.Encrypt("John", Arg.Any<byte[]>()).Returns("encrypted-first-name");
        encryption.Encrypt("Doe", Arg.Any<byte[]>()).Returns("encrypted-last-name");
        encryption.Encrypt("Male", Arg.Any<byte[]>()).Returns("encrypted-gender");
        encryption.Encrypt("18/11/1986", Arg.Any<byte[]>()).Returns("encrypted-date-of-birth");

        var handler = new CreateCustomerHandler(session, keyStore, encryption);

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
        session.Received().Events.StartStream<CustomerAggregate>(
            id,
            Arg.Is<IEnumerable<object>>(e => e.OfType<CustomerCreated>().Any()));
    }
}
