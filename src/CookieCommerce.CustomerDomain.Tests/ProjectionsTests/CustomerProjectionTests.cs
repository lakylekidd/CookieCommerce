using CookieCommerce.CustomerDomain.Application.Customers.ReadModels;
using CookieCommerce.CustomerDomain.Application.Security;
using CookieCommerce.CustomerDomain.Domain.DomainEvents;
using CookieCommerce.CustomerDomain.Infrastructure.Projections;
using JasperFx.Events;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace CookieCommerce.CustomerDomain.Tests.ProjectionsTests;

public class CustomerProjectionTests
{
    [Fact]
    public async Task ApplyAsync_ShouldCreateReadModel()
    {
        // Arrange
        var ops = Substitute.For<IDocumentOperations>();
        var keyStore = Substitute.For<ICustomerKeyStore>();
        var encryption = Substitute.For<IEncryptionService>();
        
        var customerGuid = Guid.Parse("26f10d1c-3d05-42fd-af7f-a1ac8d953350");

        keyStore.GetKeyAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(new byte[] { 1, 2, 3 });

        encryption.Decrypt("encrypted-email", Arg.Any<byte[]>()).Returns("test@example.com");
        encryption.Decrypt("encrypted-first-name", Arg.Any<byte[]>()).Returns("John");
        encryption.Decrypt("encrypted-last-name", Arg.Any<byte[]>()).Returns("Doe");
        encryption.Decrypt("encrypted-gender", Arg.Any<byte[]>()).Returns("Male");
        // Use ISO date format to avoid culture-specific parsing issues in tests
        encryption.Decrypt("encrypted-date-of-birth", Arg.Any<byte[]>()).Returns("1986-11-18");


        var scope = Substitute.For<IServiceScope>();
        var provider = Substitute.For<IServiceProvider>();

        scope.ServiceProvider.Returns(provider);
        
        provider.GetService(typeof(ICustomerKeyStore)).Returns(keyStore);
        provider.GetService(typeof(IEncryptionService)).Returns(encryption);

        var projection = new CustomerProjection(provider);

        var e = new Event<CustomerCreated>(new CustomerCreated
        {
            CustomerId = customerGuid,
            EncryptedEmail = "encrypted-email",
            EncryptedFirstName = "encrypted-first-name",
            EncryptedLastName = "encrypted-last-name",
            EncryptedGender = "encrypted-gender",
            EncryptedDateOfBirth = "encrypted-date-of-birth"
        });

        // Act
        await projection.ApplyAsync(ops, new JasperFx.Events.IEvent[] { e }, default);

        // Assert
        ops.Received().Store(Arg.Is<CustomerReadModel>(m =>
            m.Id == customerGuid &&
            m.Email == "test@example.com" &&
            m.FirstName == "John" &&
            m.LastName == "Doe" &&
            m.Gender == Domain.ValueObjects.Gender.Male &&
            m.DateOfBirth == new DateTime(1986, 11, 18)
        ));
    }
}
