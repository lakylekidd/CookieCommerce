using System.Reflection;
using CookieCommerce.CustomerDomain.Domain.Aggregates;
using CookieCommerce.CustomerDomain.Domain.DomainEvents;
using FluentAssertions;

namespace CookieCommerce.CustomerDomain.Tests.DomainTests;

public class CustomerAggregate_AddressView_Tests
{
    [Fact]
    public void ReplayPreservesAddressIdsAndActiveDefault_GivenAddressAddedEvents()
    {
        var customerId = Guid.NewGuid();
        var firstId = Guid.CreateVersion7();
        var secondId = Guid.CreateVersion7();
        var createdAt = DateTimeOffset.UtcNow;
        var events = new object[]
        {
            new CustomerCreated
            {
                CustomerId = customerId,
                EncryptedEmail = "email",
                EncryptedFirstName = "first",
                EncryptedLastName = "last",
                EncryptedGender = "gender"
            },
            BuildAddressAdded(customerId, firstId, "Home", false, createdAt),
            BuildAddressAdded(customerId, secondId, "Work", true, createdAt.AddMinutes(1))
        };

        var aggregate = Replay(events);

        aggregate.Addresses.Should().HaveCount(2);
        aggregate.Addresses.Should().Contain(address => address.AddressId == firstId && address.Label == "Home" && !address.IsDefault);
        aggregate.Addresses.Should().Contain(address => address.AddressId == secondId && address.Label == "Work" && address.IsDefault);
        aggregate.Addresses.Count(address => address.IsDefault).Should().Be(1);
    }

    [Fact]
    public void ReplayPromotesRemainingAddress_GivenDefaultAddressRemoved()
    {
        var customerId = Guid.NewGuid();
        var firstId = Guid.CreateVersion7();
        var secondId = Guid.CreateVersion7();
        var createdAt = DateTimeOffset.UtcNow;
        var events = new object[]
        {
            new CustomerCreated
            {
                CustomerId = customerId,
                EncryptedEmail = "email",
                EncryptedFirstName = "first",
                EncryptedLastName = "last",
                EncryptedGender = "gender"
            },
            BuildAddressAdded(customerId, firstId, "Home", true, createdAt),
            BuildAddressAdded(customerId, secondId, "Work", false, createdAt.AddMinutes(1)),
            new CustomerAddressRemoved
            {
                CustomerId = customerId,
                AddressId = firstId,
                DeletedAtUtc = createdAt.AddMinutes(2)
            }
        };

        var aggregate = Replay(events);

        aggregate.Addresses.Should().ContainSingle();
        aggregate.Addresses.Single().AddressId.Should().Be(secondId);
        aggregate.Addresses.Single().IsDefault.Should().BeTrue();
    }

    private static CustomerAggregate Replay(IEnumerable<object> events)
    {
        var aggregate = new CustomerAggregate();
        var when = typeof(CustomerAggregate).GetMethod("When", BindingFlags.Instance | BindingFlags.NonPublic)!;

        foreach (var @event in events)
        {
            when.Invoke(aggregate, [@event]);
        }

        return aggregate;
    }

    private static CustomerAddressAdded BuildAddressAdded(
        Guid customerId,
        Guid addressId,
        string label,
        bool isDefault,
        DateTimeOffset createdAtUtc) =>
        new()
        {
            CustomerId = customerId,
            AddressId = addressId,
            Icon = "home",
            Label = label,
            EncryptedFirstName = $"first-{label}",
            EncryptedLastName = $"last-{label}",
            EncryptedStreet = $"street-{label}",
            EncryptedHouseNumber = "12",
            EncryptedCity = "Baker City",
            EncryptedPostalCode = "1234 AB",
            EncryptedProvince = "North Brabant",
            EncryptedCountry = "Netherlands",
            EncryptedPhone = "+31612345678",
            IsDefault = isDefault,
            CreatedAtUtc = createdAtUtc
        };
}
