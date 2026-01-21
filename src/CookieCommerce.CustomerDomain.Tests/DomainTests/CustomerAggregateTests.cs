using CookieCommerce.CustomerDomain.Domain.Aggregates;
using CookieCommerce.CustomerDomain.Domain.DomainEvents;
using FluentAssertions;

namespace CookieCommerce.CustomerDomain.Tests.DomainTests;

public class CustomerAggregateTests
{
    [Fact]
    public void Create_ShouldThrow_WhenRequiredFieldsMissing()
    {
        var agg = new CustomerAggregate();

        Action act = () => agg.Create(Guid.NewGuid(), "", "first", "last", "gender", null);
        act.Should().Throw<Exception>();

        act = () => agg.Create(Guid.NewGuid(), "email", "", "last", "gender", null);
        act.Should().Throw<Exception>();

        act = () => agg.Create(Guid.NewGuid(), "email", "first", "", "gender", null);
        act.Should().Throw<Exception>();

        act = () => agg.Create(Guid.NewGuid(), "email", "first", "last", "", null);
        act.Should().Throw<Exception>();
    }

    [Fact]
    public void AddRemoveAddress_ShouldWork()
    {
        var agg = new CustomerAggregate();
        var id = Guid.NewGuid();
        agg.Create(id, "e", "f", "l", "g", null);

        var addr = new Domain.ValueObjects.Address("home", "line1", "town", "postcode", "GB");
        agg.AddAddress(addr);
        agg.Addresses.Should().ContainSingle(a => a.Label == "home");

        Action act = () => agg.AddAddress(addr);
        act.Should().Throw<InvalidOperationException>();

        agg.RemoveAddress("home");
        agg.Addresses.Should().BeEmpty();

        act = () => agg.RemoveAddress("home");
        act.Should().Throw<InvalidOperationException>();
    }
}
