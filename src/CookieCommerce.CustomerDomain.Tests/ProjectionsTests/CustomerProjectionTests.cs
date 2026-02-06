using System;
using System.Threading;
using System.Threading.Tasks;
using CookieCommerce.CustomerDomain.Application.Customers.ReadModels;
using CookieCommerce.CustomerDomain.Application.Security;
using CookieCommerce.CustomerDomain.Domain.DomainEvents;
using CookieCommerce.CustomerDomain.Domain.ValueObjects;
using CookieCommerce.CustomerDomain.Infrastructure.Projections;
using JasperFx.Events;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace CookieCommerce.CustomerDomain.Tests.ProjectionsTests;

public class CustomerProjectionTests
{
    [Fact]
    public async Task ApplyAsync_CustomerCreated_StoresDecryptedReadModel()
    {
        // Arrange
        var services = new ServiceCollection();
        var keyStore = Substitute.For<ICustomerKeyStore>();
        var encryption = Substitute.For<IEncryptionService>();

        var id = Guid.NewGuid();

        keyStore.GetKeyAsync(id, Arg.Any<CancellationToken>()).Returns(Task.FromResult(new byte[] { 1, 2, 3 }));

        // projection will call Decrypt with the encrypted values provided below
        encryption.Decrypt("encrypted-email", Arg.Any<byte[]>()).Returns("test@example.com");
        encryption.Decrypt("encrypted-first", Arg.Any<byte[]>()).Returns("John");
        encryption.Decrypt("encrypted-last", Arg.Any<byte[]>()).Returns("Doe");
        encryption.Decrypt("encrypted-gender", Arg.Any<byte[]>()).Returns("Male");
        var dob = DateTime.Parse("1986-11-18");
        encryption.Decrypt("encrypted-dob", Arg.Any<byte[]>()).Returns(dob.ToString("O"));

        services.AddSingleton(keyStore);
        services.AddSingleton(encryption);

        var projection = new CustomerProjection(services);

        var ops = Substitute.For<IDocumentOperations>();

        var created = new CustomerCreated
        {
            CustomerId = id,
            EncryptedEmail = "encrypted-email",
            EncryptedFirstName = "encrypted-first",
            EncryptedLastName = "encrypted-last",
            EncryptedGender = "encrypted-gender",
            EncryptedDateOfBirth = "encrypted-dob"
        };

        var @event = Substitute.For<IEvent>();
        @event.Data.Returns(created);

        // Act
        await projection.ApplyAsync(ops, @event, CancellationToken.None);

        // Assert
        ops.Received().Store(Arg.Is<CustomerReadModel>(c =>
            c.Id == id &&
            c.Email == "test@example.com" &&
            c.FirstName == "John" &&
            c.LastName == "Doe" &&
            c.Gender == Gender.Male &&
            c.DateOfBirth.HasValue && c.DateOfBirth.Value.Date == dob.Date
        ));
    }

    [Fact]
    public async Task ApplyAsync_CustomerCreated_KeyMissing_DoesNotStore()
    {
        // Arrange
        var services = new ServiceCollection();
        var keyStore = Substitute.For<ICustomerKeyStore>();
        var encryption = Substitute.For<IEncryptionService>();

        var id = Guid.NewGuid();

        // No key -> returns null
        keyStore.GetKeyAsync(id, Arg.Any<CancellationToken>()).Returns(Task.FromResult<byte[]?>(null));

        services.AddSingleton(keyStore);
        services.AddSingleton(encryption);

        var projection = new CustomerProjection(services);

        var ops = Substitute.For<IDocumentOperations>();

        var created = new CustomerCreated
        {
            CustomerId = id,
            EncryptedEmail = "encrypted-email",
            EncryptedFirstName = "encrypted-first",
            EncryptedLastName = "encrypted-last",
            EncryptedGender = "encrypted-gender",
            EncryptedDateOfBirth = null
        };

        var @event = Substitute.For<IEvent>();
        @event.Data.Returns(created);

        // Act
        await projection.ApplyAsync(ops, @event, CancellationToken.None);

        // Assert
        ops.DidNotReceive().Store(Arg.Any<CustomerReadModel>());
    }

    [Fact]
    public async Task ApplyAsync_CustomerDeleted_DeletesReadModel()
    {
        // Arrange
        var services = new ServiceCollection();
        var projection = new CustomerProjection(services);

        var ops = Substitute.For<IDocumentOperations>();

        var deleted = new CustomerDeleted(Guid.NewGuid());
        var @event = Substitute.For<IEvent>();
        @event.Data.Returns(deleted);

        // Act
        await projection.ApplyAsync(ops, @event, CancellationToken.None);

        // Assert - ensure DeleteWhere was called for CustomerReadModel
        ops.Received().DeleteWhere<CustomerReadModel>(Arg.Any<System.Linq.Expressions.Expression<Func<CustomerReadModel, bool>>>());
    }
}
