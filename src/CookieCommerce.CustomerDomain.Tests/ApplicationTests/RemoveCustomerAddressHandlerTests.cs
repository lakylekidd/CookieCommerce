using CookieCommerce.CustomerDomain.Application.Customers.ReadModels;
using CookieCommerce.CustomerDomain.Domain.DomainEvents;
using CookieCommerce.CustomerDomain.Infrastructure.Projections;
using CookieCommerce.CustomerDomain.Application.Security;
using FluentAssertions;
using NSubstitute;

namespace CookieCommerce.CustomerDomain.Tests.ApplicationTests;

public class RemoveCustomerAddressHandlerTests
{
    [Fact]
    public void ApplyRemovesAddressAndPromotesOldestRemainingDefault_GivenDefaultAddressRemoved()
    {
        var firstId = Guid.CreateVersion7();
        var secondId = Guid.CreateVersion7();
        var createdAt = DateTimeOffset.UtcNow;
        var current = new CustomerReadModel
        {
            Id = Guid.CreateVersion7(),
            Addresses =
            [
                BuildAddress(firstId, "Home", true, createdAt),
                BuildAddress(secondId, "Work", false, createdAt.AddMinutes(1))
            ]
        };
        var projection = new CustomerProjection(Substitute.For<IEncryptionService>());

        projection.Apply(new CustomerAddressRemoved
        {
            CustomerId = current.Id,
            AddressId = firstId,
            DeletedAtUtc = createdAt.AddMinutes(2)
        }, current);

        current.Addresses.Should().ContainSingle();
        current.Addresses.Single().AddressId.Should().Be(secondId);
        current.Addresses.Single().IsDefault.Should().BeTrue();
    }

    [Fact]
    public void ApplyLeavesReadModelUnchanged_GivenMissingAddressId()
    {
        var current = new CustomerReadModel
        {
            Id = Guid.CreateVersion7(),
            Addresses = [BuildAddress(Guid.CreateVersion7(), "Home", true, DateTimeOffset.UtcNow)]
        };
        var projection = new CustomerProjection(Substitute.For<IEncryptionService>());

        projection.Apply(new CustomerAddressRemoved
        {
            CustomerId = current.Id,
            AddressId = Guid.CreateVersion7(),
            DeletedAtUtc = DateTimeOffset.UtcNow
        }, current);

        current.Addresses.Should().ContainSingle();
        current.Addresses.Single().IsDefault.Should().BeTrue();
    }

    private static CustomerAddressReadModel BuildAddress(Guid id, string label, bool isDefault, DateTimeOffset createdAtUtc) => new()
    {
        AddressId = id,
        Index = 1,
        Icon = "home",
        Label = label,
        FirstName = "Jamie",
        LastName = "Baker",
        Street = "Cookie Lane",
        HouseNumber = "12",
        City = "Baker City",
        PostalCode = "1234 AB",
        Province = "North Brabant",
        Country = "Netherlands",
        Phone = "+31612345678",
        IsDefault = isDefault,
        CreatedAtUtc = createdAtUtc
    };
}
