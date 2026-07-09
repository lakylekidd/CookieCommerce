using CookieCommerce.CustomerDomain.Domain.Aggregates;
using CookieCommerce.CustomerDomain.Domain.DomainEvents;
using CookieCommerce.CustomerDomain.Domain.Exceptions;
using FluentAssertions;

namespace CookieCommerce.CustomerDomain.Tests.DomainTests;

public class CustomerAggregate_AddAddress_Tests
{
    [Fact]
    public void AddAddressMakesFirstAddressDefault_GivenRequestedDefaultIsFalse()
    {
        var customer = CreateCustomer();
        var address = BuildAddressAdded(customer.Id, "Home", false);

        customer.AddAddress(address);

        customer.Addresses.Should().ContainSingle();
        customer.Addresses.Single().IsDefault.Should().BeTrue();
    }

    [Fact]
    public void AddAddressReplacesPreviousDefault_GivenNewAddressRequestsDefault()
    {
        var customer = CreateCustomer();
        customer.AddAddress(BuildAddressAdded(customer.Id, "Home", true, DateTimeOffset.UtcNow));

        customer.AddAddress(BuildAddressAdded(customer.Id, "Work", true, DateTimeOffset.UtcNow.AddMinutes(1)));

        customer.Addresses.Should().HaveCount(2);
        customer.Addresses.Single(a => a.Label == "Home").IsDefault.Should().BeFalse();
        customer.Addresses.Single(a => a.Label == "Work").IsDefault.Should().BeTrue();
        customer.Addresses.Count(a => a.IsDefault).Should().Be(1);
    }

    [Fact]
    public void AddAddressRejectsDuplicateLabel_GivenActiveAddressAlreadyUsesLabel()
    {
        var customer = CreateCustomer();
        customer.AddAddress(BuildAddressAdded(customer.Id, "Home", true));

        var act = () => customer.AddAddress(BuildAddressAdded(customer.Id, "home", false));

        act.Should().Throw<DuplicateCustomerAddressLabelException>();
        customer.Addresses.Should().ContainSingle();
    }

    [Fact]
    public void AddAddressPreservesExistingDefault_GivenNewAddressDoesNotRequestDefault()
    {
        var customer = CreateCustomer();
        customer.AddAddress(BuildAddressAdded(customer.Id, "Home", true, DateTimeOffset.UtcNow));

        customer.AddAddress(BuildAddressAdded(customer.Id, "Work", false, DateTimeOffset.UtcNow.AddMinutes(1)));

        customer.Addresses.Single(a => a.Label == "Home").IsDefault.Should().BeTrue();
        customer.Addresses.Single(a => a.Label == "Work").IsDefault.Should().BeFalse();
    }

    private static CustomerAggregate CreateCustomer()
    {
        var customer = new CustomerAggregate();
        var customerId = Guid.CreateVersion7();
        customer.Create(customerId, "email", "first", "last", "gender", null);
        return customer;
    }

    private static CustomerAddressAdded BuildAddressAdded(Guid customerId, string label, bool isDefault, DateTimeOffset? createdAtUtc = null) => new()
    {
        CustomerId = customerId,
        AddressId = Guid.CreateVersion7(),
        Icon = "home",
        Label = label,
        EncryptedFirstName = "first",
        EncryptedLastName = "last",
        EncryptedStreet = "street",
        EncryptedHouseNumber = "12",
        EncryptedCity = "city",
        EncryptedPostalCode = "1234 AB",
        EncryptedProvince = "province",
        EncryptedCountry = "country",
        EncryptedPhone = "phone",
        IsDefault = isDefault,
        CreatedAtUtc = createdAtUtc ?? DateTimeOffset.UtcNow
    };
}
