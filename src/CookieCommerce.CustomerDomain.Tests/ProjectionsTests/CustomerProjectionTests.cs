using CookieCommerce.CustomerDomain.Application.Customers.ReadModels;
using CookieCommerce.CustomerDomain.Application.Security;
using CookieCommerce.CustomerDomain.Domain.DomainEvents;
using CookieCommerce.CustomerDomain.Domain.ValueObjects;
using CookieCommerce.CustomerDomain.Infrastructure.Projections;
using CookieCommerce.CustomerDomain.Infrastructure.Security;
using FluentAssertions;
using Marten;
using NSubstitute;

namespace CookieCommerce.CustomerDomain.Tests.ProjectionsTests;

public class CustomerProjectionTests
{
    [Fact]
    public async Task Create_CustomerCreated_ReturnsDecryptedReadModel()
    {
        // Arrange
        var encryption = Substitute.For<IEncryptionService>();
        var querySession = Substitute.For<IQuerySession>();

        var id = Guid.NewGuid();
        var key = new byte[] { 1, 2, 3 };
        var keyRecord = new CustomerKeyRecord
        {
            Id = id,
            Key = key,
            CreatedAt = DateTime.UtcNow
        };

        querySession.LoadAsync<CustomerKeyRecord>(id, Arg.Any<CancellationToken>()).Returns(keyRecord);

        encryption.Decrypt("encrypted-email", key).Returns("test@example.com");
        encryption.Decrypt("encrypted-first", key).Returns("John");
        encryption.Decrypt("encrypted-last", key).Returns("Doe");
        encryption.Decrypt("encrypted-gender", key).Returns("Male");
        var dob = DateTime.Parse("1986-11-18");
        encryption.Decrypt("encrypted-dob", key).Returns(dob.ToString("O"));

        var projection = new CustomerProjection(encryption);

        var created = new CustomerCreated
        {
            CustomerId = id,
            EncryptedEmail = "encrypted-email",
            EncryptedFirstName = "encrypted-first",
            EncryptedLastName = "encrypted-last",
            EncryptedGender = "encrypted-gender",
            EncryptedDateOfBirth = "encrypted-dob"
        };

        // Act
        var result = await projection.Create(created, querySession);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(id);
        result.Email.Should().Be("test@example.com");
        result.FirstName.Should().Be("John");
        result.LastName.Should().Be("Doe");
        result.Gender.Should().Be(Gender.Male);
        result.DateOfBirth.Should().NotBeNull();
        result.DateOfBirth!.Value.Date.Should().Be(dob.Date);
    }

    [Fact]
    public async Task Create_CustomerCreated_KeyMissing_ReturnsNull()
    {
        // Arrange
        var encryption = Substitute.For<IEncryptionService>();
        var querySession = Substitute.For<IQuerySession>();

        var id = Guid.NewGuid();

        // No key -> returns null
        querySession.LoadAsync<CustomerKeyRecord>(id, Arg.Any<CancellationToken>()).Returns((CustomerKeyRecord?)null);

        var projection = new CustomerProjection(encryption);

        var created = new CustomerCreated
        {
            CustomerId = id,
            EncryptedEmail = "encrypted-email",
            EncryptedFirstName = "encrypted-first",
            EncryptedLastName = "encrypted-last",
            EncryptedGender = "encrypted-gender",
            EncryptedDateOfBirth = null
        };

        // Act
        var result = await projection.Create(created, querySession);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Apply_CustomerAddressAdded_AddsAddressToReadModel()
    {
        // Arrange
        var encryption = Substitute.For<IEncryptionService>();
        var projection = new CustomerProjection(encryption);

        var customerId = Guid.NewGuid();
        var current = new CustomerReadModel
        {
            Id = customerId,
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe",
            Gender = Gender.Male,
            Addresses = new List<CustomerAddressReadModel>()
        };

        var addressAdded = new CustomerAddressAdded(
            customerId,
            new Address("Home", "123 Main St", "Springfield", "62701", "US")
        );

        // Act
        var result = projection.Apply(addressAdded, current);

        // Assert
        result.Addresses.Should().HaveCount(1);
        result.Addresses[0].Label.Should().Be("Home");
        result.Addresses[0].Street.Should().Be("123 Main St");
        result.Addresses[0].City.Should().Be("Springfield");
        result.Addresses[0].PostalCode.Should().Be("62701");
        result.Addresses[0].Country.Should().Be("US");
    }

    [Fact]
    public void Apply_CustomerAddressRemoved_RemovesAddressFromReadModel()
    {
        // Arrange
        var encryption = Substitute.For<IEncryptionService>();
        var projection = new CustomerProjection(encryption);

        var customerId = Guid.NewGuid();
        var current = new CustomerReadModel
        {
            Id = customerId,
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe",
            Gender = Gender.Male,
            Addresses = new List<CustomerAddressReadModel>
            {
                new CustomerAddressReadModel
                {
                    Label = "Home",
                    Street = "123 Main St",
                    City = "Springfield",
                    State = "IL",
                    PostalCode = "62701",
                    Country = "US"
                },
                new CustomerAddressReadModel
                {
                    Label = "Work",
                    Street = "456 Office Blvd",
                    City = "Chicago",
                    State = "IL",
                    PostalCode = "60601",
                    Country = "US"
                }
            }
        };

        var addressRemoved = new CustomerAddressRemoved(customerId, "Home");

        // Act
        var result = projection.Apply(addressRemoved, current);

        // Assert
        result.Addresses.Should().HaveCount(1);
        result.Addresses[0].Label.Should().Be("Work");
    }
}

