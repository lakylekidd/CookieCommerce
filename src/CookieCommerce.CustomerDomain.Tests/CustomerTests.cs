using CookieCommerce.CustomerDomain.Domain.Aggregates;
using CookieCommerce.CustomerDomain.Domain.DomainEvents;
using FluentAssertions;

namespace CookieCommerce.CustomerDomain.Tests;

public class CustomerTests
{
    [Fact]
    public void Create_ShouldEmitCustomerCreatedEvent()
    {
        // Arrange
        var customer = new CustomerAggregate();
        var id = Guid.NewGuid();

        // Act
        customer.Create(
            id,
            "encryptedEmail",
            "encryptedFirstName",
            "encryptedLastName",
            "encryptedGender",
            "encryptedDateOfBirth"
        );

        // Assert
        var events = customer.DomainEvents;
        var created = Assert.Single(events) as CustomerCreated;

        created.Should().NotBeNull();
        created.CustomerId.Should().Be(id);
        created.EncryptedEmail.Should().Be("encryptedEmail");
        created.EncryptedFirstName.Should().Be("encryptedFirstName");
        created.EncryptedLastName.Should().Be("encryptedLastName");
        created.EncryptedGender.Should().Be("encryptedGender");
        created.EncryptedDateOfBirth.Should().Be("encryptedDateOfBirth");
    }
}
