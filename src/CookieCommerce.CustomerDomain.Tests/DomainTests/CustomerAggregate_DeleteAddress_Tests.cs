using CookieCommerce.CustomerDomain.Domain.Aggregates;
using CookieCommerce.CustomerDomain.Domain.DomainEvents;
using CookieCommerce.CustomerDomain.Domain.Exceptions;
using FluentAssertions;

namespace CookieCommerce.CustomerDomain.Tests.DomainTests;

public class CustomerAggregate_DeleteAddress_Tests
{
    [Fact]
    public void RemoveAddressPreservesDefault_GivenRemovingNonDefaultAddress()
    {
        var customer = CreateCustomerWithAddresses(out var defaultId, out var otherId);

        customer.RemoveAddress(otherId, DateTimeOffset.UtcNow);

        customer.Addresses.Should().ContainSingle();
        customer.Addresses.Single().AddressId.Should().Be(defaultId);
        customer.Addresses.Single().IsDefault.Should().BeTrue();
    }

    [Fact]
    public void RemoveAddressPromotesOldestRemainingAddress_GivenRemovingDefaultAddress()
    {
        var customer = CreateCustomerWithAddresses(out var defaultId, out var otherId);

        customer.RemoveAddress(defaultId, DateTimeOffset.UtcNow);

        customer.Addresses.Should().ContainSingle();
        customer.Addresses.Single().AddressId.Should().Be(otherId);
        customer.Addresses.Single().IsDefault.Should().BeTrue();
    }

    [Fact]
    public void RemoveAddressLeavesZeroDefaults_GivenRemovingOnlyAddress()
    {
        var customer = CreateCustomer();
        var address = BuildAddressAdded(customer.Id, "Home", true, DateTimeOffset.UtcNow);
        customer.AddAddress(address);

        customer.RemoveAddress(address.AddressId, DateTimeOffset.UtcNow);

        customer.Addresses.Should().BeEmpty();
    }

    [Fact]
    public void RemoveAddressRejectsMissingAddressId_GivenAddressIsAlreadyRemoved()
    {
        var customer = CreateCustomer();
        var address = BuildAddressAdded(customer.Id, "Home", true, DateTimeOffset.UtcNow);
        customer.AddAddress(address);
        customer.RemoveAddress(address.AddressId, DateTimeOffset.UtcNow);

        var act = () => customer.RemoveAddress(address.AddressId, DateTimeOffset.UtcNow);

        act.Should().Throw<CustomerAddressNotFoundException>();
        customer.Addresses.Should().BeEmpty();
    }

    private static CustomerAggregate CreateCustomerWithAddresses(out Guid defaultId, out Guid otherId)
    {
        var customer = CreateCustomer();
        var createdAt = DateTimeOffset.UtcNow;
        var defaultAddress = BuildAddressAdded(customer.Id, "Home", true, createdAt);
        var otherAddress = BuildAddressAdded(customer.Id, "Work", false, createdAt.AddMinutes(1));
        customer.AddAddress(defaultAddress);
        customer.AddAddress(otherAddress);
        defaultId = defaultAddress.AddressId;
        otherId = otherAddress.AddressId;
        return customer;
    }

    private static CustomerAggregate CreateCustomer()
    {
        var customer = new CustomerAggregate();
        customer.Create(Guid.CreateVersion7(), "email", "first", "last", "gender", null);
        return customer;
    }

    private static CustomerAddressAdded BuildAddressAdded(Guid customerId, string label, bool isDefault, DateTimeOffset createdAtUtc) => new()
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
        CreatedAtUtc = createdAtUtc
    };
}
