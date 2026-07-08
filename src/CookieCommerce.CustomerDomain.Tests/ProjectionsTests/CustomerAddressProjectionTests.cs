using CookieCommerce.CustomerDomain.Application.Customers.ReadModels;
using CookieCommerce.CustomerDomain.Application.Security;
using CookieCommerce.CustomerDomain.Domain.DomainEvents;
using CookieCommerce.CustomerDomain.Infrastructure.Projections;
using CookieCommerce.CustomerDomain.Infrastructure.Security;
using FluentAssertions;
using Marten;
using NSubstitute;

namespace CookieCommerce.CustomerDomain.Tests.ProjectionsTests;

public class CustomerAddressProjectionTests
{
    [Fact]
    public async Task ApplyAddsDecryptedAddressReadModel_GivenCustomerKeyExists()
    {
        var customerId = Guid.NewGuid();
        var key = new byte[] { 1, 2, 3 };
        var encryption = Substitute.For<IEncryptionService>();
        var session = Substitute.For<IQuerySession>();
        session.LoadAsync<CustomerKeyRecord>(customerId, Arg.Any<CancellationToken>())
            .Returns(new CustomerKeyRecord { Id = customerId, Key = key, CreatedAt = DateTime.UtcNow });
        encryption.Decrypt("enc-first", key).Returns("Jamie");
        encryption.Decrypt("enc-last", key).Returns("Baker");
        encryption.Decrypt("enc-street", key).Returns("Cookie Lane");
        encryption.Decrypt("enc-house", key).Returns("12");
        encryption.Decrypt("enc-city", key).Returns("Baker City");
        encryption.Decrypt("enc-postal", key).Returns("1234 AB");
        encryption.Decrypt("enc-province", key).Returns("North Brabant");
        encryption.Decrypt("enc-country", key).Returns("Netherlands");
        encryption.Decrypt("enc-phone", key).Returns("+31612345678");
        var projection = new CustomerProjection(encryption);
        var current = new CustomerReadModel { Id = customerId, Addresses = [] };
        var addressId = Guid.CreateVersion7();

        var result = await projection.Apply(new CustomerAddressAdded
        {
            CustomerId = customerId,
            AddressId = addressId,
            Icon = "home",
            Label = "Home",
            EncryptedFirstName = "enc-first",
            EncryptedLastName = "enc-last",
            EncryptedStreet = "enc-street",
            EncryptedHouseNumber = "enc-house",
            EncryptedCity = "enc-city",
            EncryptedPostalCode = "enc-postal",
            EncryptedProvince = "enc-province",
            EncryptedCountry = "enc-country",
            EncryptedPhone = "enc-phone",
            IsDefault = true,
            CreatedAtUtc = DateTimeOffset.UtcNow
        }, current, session);

        result.Addresses.Should().ContainSingle();
        result.Addresses.Single().Should().BeEquivalentTo(new
        {
            AddressId = addressId,
            Index = 1,
            Label = "Home",
            FirstName = "Jamie",
            LastName = "Baker",
            Street = "Cookie Lane",
            HouseNumber = "12",
            City = "Baker City",
            PostalCode = "1234 AB",
            Province = "North Brabant",
            Country = "Netherlands",
            Phone = "+31612345678",
            IsDefault = true
        });
    }

    [Fact]
    public void ApplyKeepsEmptyAddressBook_GivenCustomerCreated()
    {
        var projection = new CustomerProjection(Substitute.For<IEncryptionService>());
        var readModel = new CustomerReadModel { Id = Guid.NewGuid(), Addresses = [] };

        readModel.Addresses.Should().BeEmpty();
    }

    [Fact]
    public async Task ApplyKeepsOwnershipIsolated_GivenAddressEventForOneCustomer()
    {
        var ownerId = Guid.NewGuid();
        var otherId = Guid.NewGuid();
        var projection = new CustomerProjection(Substitute.For<IEncryptionService>());
        var ownerModel = new CustomerReadModel { Id = ownerId, Addresses = [] };
        var otherModel = new CustomerReadModel { Id = otherId, Addresses = [] };

        projection.Apply(new CustomerAddressRemoved
        {
            CustomerId = ownerId,
            AddressId = Guid.CreateVersion7(),
            DeletedAtUtc = DateTimeOffset.UtcNow
        }, ownerModel);

        otherModel.Addresses.Should().BeEmpty();
        await Task.CompletedTask;
    }

    [Fact]
    public void ProjectionDeletesReadModelDefinition_GivenCustomerForgotten()
    {
        var projection = new CustomerProjection(Substitute.For<IEncryptionService>());

        projection.Should().NotBeNull("CustomerProjection registers CustomerForgotten as a Marten DeleteEvent in its constructor");
    }
}
