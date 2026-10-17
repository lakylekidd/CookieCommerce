using CookieCommerce.CustomerDomain.Domain.Aggregates;
using CookieCommerce.CustomerDomain.Domain.Exceptions;
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
    public void Create_ShouldSetAllProperties_WhenAllFieldsProvided()
    {
        var agg = new CustomerAggregate();
        var id = Guid.NewGuid();
        var email = "encrypted_email";
        var firstName = "encrypted_first";
        var lastName = "encrypted_last";
        var gender = "encrypted_gender";
        var dateOfBirth = "encrypted_dob";

        agg.Create(id, email, firstName, lastName, gender, dateOfBirth);

        agg.Id.Should().Be(id);
        agg.EncryptedEmail.Should().Be(email);
        agg.EncryptedFirstName.Should().Be(firstName);
        agg.EncryptedLastName.Should().Be(lastName);
        agg.EncryptedGender.Should().Be(gender);
        agg.EncryptedDateOfBirth.Should().Be(dateOfBirth);
        agg.ForgottenDateUtc.Should().BeNull();
    }

    [Fact]
    public void Create_ShouldSetAllProperties_WhenDateOfBirthIsNull()
    {
        var agg = new CustomerAggregate();
        var id = Guid.NewGuid();
        var email = "encrypted_email";
        var firstName = "encrypted_first";
        var lastName = "encrypted_last";
        var gender = "encrypted_gender";

        agg.Create(id, email, firstName, lastName, gender, null);

        agg.Id.Should().Be(id);
        agg.EncryptedEmail.Should().Be(email);
        agg.EncryptedFirstName.Should().Be(firstName);
        agg.EncryptedLastName.Should().Be(lastName);
        agg.EncryptedGender.Should().Be(gender);
        agg.EncryptedDateOfBirth.Should().BeNull();
        agg.ForgottenDateUtc.Should().BeNull();
    }

    [Fact]
    public void Forget_ShouldSetForgottenDateUtc()
    {
        var agg = new CustomerAggregate();
        var id = Guid.NewGuid();
        agg.Create(id, "e", "f", "l", "g", null);

        var beforeForget = DateTime.UtcNow;
        agg.Forget();
        var afterForget = DateTime.UtcNow;

        agg.ForgottenDateUtc.Should().NotBeNull();
        agg.ForgottenDateUtc.Should().BeOnOrAfter(beforeForget);
        agg.ForgottenDateUtc.Should().BeOnOrBefore(afterForget);
    }

    [Fact]
    public void AddAddress_ShouldAddNonPrimaryAddress_WithoutAffectingOthers()
    {
        var agg = new CustomerAggregate();
        var id = Guid.NewGuid();
        agg.Create(id, "e", "f", "l", "g", null);

        var primaryAddr = new Domain.ValueObjects.Address("home", "street1", "city1", "post1", "GB", isPrimary: true);
        var nonPrimaryAddr = new Domain.ValueObjects.Address("work", "street2", "city2", "post2", "GB", isPrimary: false);

        agg.AddAddress(primaryAddr);
        agg.AddAddress(nonPrimaryAddr);

        agg.Addresses.Should().HaveCount(2);
        agg.Addresses.Should().Contain(a => a.Label == "home" && a.IsPrimary);
        agg.Addresses.Should().Contain(a => a.Label == "work" && !a.IsPrimary);
    }

    [Fact]
    public void AddAddress_ShouldAddPrimaryAddress_WhenNoPreviousPrimary()
    {
        var agg = new CustomerAggregate();
        var id = Guid.NewGuid();
        agg.Create(id, "e", "f", "l", "g", null);

        var primaryAddr = new Domain.ValueObjects.Address("home", "street1", "city1", "post1", "GB", isPrimary: true);

        agg.AddAddress(primaryAddr);

        agg.Addresses.Should().ContainSingle();
        agg.Addresses.First().Label.Should().Be("home");
        agg.Addresses.First().IsPrimary.Should().BeTrue();
    }

    [Fact]
    public void AddAddress_ShouldUnsetOldPrimary_WhenNewPrimaryAdded()
    {
        var agg = new CustomerAggregate();
        var id = Guid.NewGuid();
        agg.Create(id, "e", "f", "l", "g", null);

        var oldPrimaryAddr = new Domain.ValueObjects.Address("home", "street1", "city1", "post1", "GB", isPrimary: true);
        var newPrimaryAddr = new Domain.ValueObjects.Address("work", "street2", "city2", "post2", "GB", isPrimary: true);

        agg.AddAddress(oldPrimaryAddr);
        agg.AddAddress(newPrimaryAddr);

        agg.Addresses.Should().HaveCount(2);
        agg.Addresses.Should().Contain(a => a.Label == "home" && !a.IsPrimary);
        agg.Addresses.Should().Contain(a => a.Label == "work" && a.IsPrimary);
        agg.Addresses.Count(a => a.IsPrimary).Should().Be(1);
    }

    [Fact]
    public void AddAddress_ShouldUnsetAllPreviousPrimaries_WhenMultiplePrimaryAddressesAdded()
    {
        var agg = new CustomerAggregate();
        var id = Guid.NewGuid();
        agg.Create(id, "e", "f", "l", "g", null);

        var firstPrimary = new Domain.ValueObjects.Address("home", "street1", "city1", "post1", "GB", isPrimary: true);
        var secondPrimary = new Domain.ValueObjects.Address("work", "street2", "city2", "post2", "GB", isPrimary: true);
        var thirdPrimary = new Domain.ValueObjects.Address("vacation", "street3", "city3", "post3", "GB", isPrimary: true);

        agg.AddAddress(firstPrimary);
        agg.AddAddress(secondPrimary);
        agg.AddAddress(thirdPrimary);

        agg.Addresses.Should().HaveCount(3);
        agg.Addresses.Should().Contain(a => a.Label == "home" && !a.IsPrimary);
        agg.Addresses.Should().Contain(a => a.Label == "work" && !a.IsPrimary);
        agg.Addresses.Should().Contain(a => a.Label == "vacation" && a.IsPrimary);
        agg.Addresses.Count(a => a.IsPrimary).Should().Be(1, "only the last added primary address should remain primary");
    }

    [Fact]
    public void EventReplay_ShouldRebuildAggregateState_WithPrimaryAddressLogicApplied()
    {
        // Arrange - Create original aggregate with multiple operations
        var originalAgg = new CustomerAggregate();
        var id = Guid.NewGuid();
        originalAgg.Create(id, "email", "first", "last", "gender", "dob");

        var firstPrimary = new Domain.ValueObjects.Address("home", "street1", "city1", "post1", "GB", isPrimary: true);
        var secondPrimary = new Domain.ValueObjects.Address("work", "street2", "city2", "post2", "GB", isPrimary: true);
        var nonPrimary = new Domain.ValueObjects.Address("vacation", "street3", "city3", "post3", "GB", isPrimary: false);

        originalAgg.AddAddress(firstPrimary);
        originalAgg.AddAddress(secondPrimary);
        originalAgg.AddAddress(nonPrimary);
        originalAgg.RemoveAddress("home");
        originalAgg.Forget();

        // Capture all events
        var events = originalAgg.DomainEvents.ToList();

        // Act - Replay events into a new aggregate
        var replayedAgg = new CustomerAggregate();
        foreach (var evt in events)
        {
            // Simulate event replay by calling When directly (in real scenarios, this would be handled by event store)
            var whenMethod = typeof(CustomerAggregate).GetMethod("When",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            whenMethod?.Invoke(replayedAgg, new[] { evt });
        }

        // Assert - Verify replayed aggregate matches original state
        replayedAgg.Id.Should().Be(originalAgg.Id);
        replayedAgg.EncryptedEmail.Should().Be(originalAgg.EncryptedEmail);
        replayedAgg.EncryptedFirstName.Should().Be(originalAgg.EncryptedFirstName);
        replayedAgg.EncryptedLastName.Should().Be(originalAgg.EncryptedLastName);
        replayedAgg.EncryptedGender.Should().Be(originalAgg.EncryptedGender);
        replayedAgg.EncryptedDateOfBirth.Should().Be(originalAgg.EncryptedDateOfBirth);
        replayedAgg.ForgottenDateUtc.Should().NotBeNull();

        // Verify addresses state including primary logic
        replayedAgg.Addresses.Should().HaveCount(2);
        replayedAgg.Addresses.Should().NotContain(a => a.Label == "home", "it was removed");
        replayedAgg.Addresses.Should().Contain(a => a.Label == "work" && a.IsPrimary, "work should be primary");
        replayedAgg.Addresses.Should().Contain(a => a.Label == "vacation" && !a.IsPrimary, "vacation should not be primary");
        replayedAgg.Addresses.Count(a => a.IsPrimary).Should().Be(1, "only one address should be primary after replay");
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
        act.Should().Throw<DuplicateAddressLabelException>();

        agg.RemoveAddress("home");
        agg.Addresses.Should().BeEmpty();

        act = () => agg.RemoveAddress("home");
        act.Should().Throw<AddressNotFoundException>();
    }
}
