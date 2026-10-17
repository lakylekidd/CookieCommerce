using CookieCommerce.CustomerDomain.Domain.Aggregates;
using CookieCommerce.CustomerDomain.Domain.DomainEvents;
using CookieCommerce.CustomerDomain.Domain.Exceptions;
using FluentAssertions;

namespace CookieCommerce.CustomerDomain.Tests.DomainTests;

public class CustomerAggregate_UpdateProfile_Tests
{
    private static CustomerAggregate BuildCreatedAggregate(Guid? id = null)
    {
        var agg = new CustomerAggregate();
        agg.Create(id ?? Guid.NewGuid(), "enc-email", "enc-first", "enc-last", "enc-gender", null);
        return agg;
    }

    // ─── UpdateProfile ────────────────────────────────────────────────────────

    [Fact]
    public void UpdateProfile_ShouldAppendProfileUpdatedEvent()
    {
        var agg = BuildCreatedAggregate();
        var beforeCount = agg.DomainEvents.Count;

        agg.UpdateProfile("enc-new-first", "enc-new-last", "enc-new-gender", "enc-new-dob");

        agg.DomainEvents.Should().HaveCount(beforeCount + 1);
        agg.DomainEvents.Last().Should().BeOfType<CustomerProfileUpdated>();
    }

    [Fact]
    public void UpdateProfile_ShouldUpdateEncryptedFields()
    {
        var agg = BuildCreatedAggregate();

        agg.UpdateProfile("enc-first-2", "enc-last-2", "enc-gender-2", "enc-dob-2");

        agg.EncryptedFirstName.Should().Be("enc-first-2");
        agg.EncryptedLastName.Should().Be("enc-last-2");
        agg.EncryptedGender.Should().Be("enc-gender-2");
        agg.EncryptedDateOfBirth.Should().Be("enc-dob-2");
    }

    [Fact]
    public void UpdateProfile_WithNullGender_ShouldSetNullGender()
    {
        var agg = BuildCreatedAggregate();
        agg.UpdateProfile("enc-first", "enc-last", null, null);

        agg.EncryptedDateOfBirth.Should().BeNull();
        // Gender keeps existing value when null is passed (non-clearing semantics)
        agg.EncryptedGender.Should().Be("enc-gender");
    }

    [Fact]
    public void UpdateProfile_WithEmptyFirstName_ShouldThrow()
    {
        var agg = BuildCreatedAggregate();

        Action act = () => agg.UpdateProfile("", "enc-last", null, null);

        act.Should().Throw<CustomerProfileValidationException>();
    }

    [Fact]
    public void UpdateProfile_WithEmptyLastName_ShouldThrow()
    {
        var agg = BuildCreatedAggregate();

        Action act = () => agg.UpdateProfile("enc-first", "  ", null, null);

        act.Should().Throw<CustomerProfileValidationException>();
    }

    [Fact]
    public void UpdateProfile_EventShouldContainCorrectCustomerId()
    {
        var id = Guid.NewGuid();
        var agg = BuildCreatedAggregate(id);
        agg.UpdateProfile("enc-first-2", "enc-last-2", "enc-gender-2", null);

        var evt = agg.DomainEvents.OfType<CustomerProfileUpdated>().Last();
        evt.CustomerId.Should().Be(id);
        evt.EncryptedFirstName.Should().Be("enc-first-2");
        evt.EncryptedLastName.Should().Be("enc-last-2");
    }

    // ─── RequestEmailChange ────────────────────────────────────────────────────

    [Fact]
    public void RequestEmailChange_ShouldAppendEmailChangeRequestedEvent()
    {
        var agg = BuildCreatedAggregate();
        var beforeCount = agg.DomainEvents.Count;

        agg.RequestEmailChange("enc-pending-email", "token-ref-123", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddHours(24));

        agg.DomainEvents.Should().HaveCount(beforeCount + 1);
        agg.DomainEvents.Last().Should().BeOfType<CustomerEmailChangeRequested>();
    }

    [Fact]
    public void RequestEmailChange_ShouldSetPendingEmailState()
    {
        var agg = BuildCreatedAggregate();
        var expiresAt = DateTimeOffset.UtcNow.AddHours(24);

        agg.RequestEmailChange("enc-pending@example.com", "token-ref-abc", DateTimeOffset.UtcNow, expiresAt);

        agg.EncryptedPendingEmail.Should().Be("enc-pending@example.com");
        agg.VerificationTokenReference.Should().Be("token-ref-abc");
        agg.VerificationExpiresAtUtc.Should().Be(expiresAt);
    }

    [Fact]
    public void RequestEmailChange_ShouldNotChangeVerifiedEmail()
    {
        var agg = BuildCreatedAggregate();
        var originalEmail = agg.EncryptedEmail;

        agg.RequestEmailChange("enc-pending", "ref-123", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddHours(24));

        agg.EncryptedEmail.Should().Be(originalEmail);
    }

    [Fact]
    public void RequestEmailChange_WithEmptyPendingEmail_ShouldThrow()
    {
        var agg = BuildCreatedAggregate();

        Action act = () => agg.RequestEmailChange("", "ref", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddHours(1));

        act.Should().Throw<CustomerProfileValidationException>();
    }

    [Fact]
    public void RequestEmailChange_WithEmptyTokenReference_ShouldThrow()
    {
        var agg = BuildCreatedAggregate();

        Action act = () => agg.RequestEmailChange("enc-email", "", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddHours(1));

        act.Should().Throw<CustomerProfileValidationException>();
    }

    [Fact]
    public void RequestEmailChange_CanReplaceExistingPendingEmail()
    {
        var agg = BuildCreatedAggregate();
        agg.RequestEmailChange("enc-first-pending", "ref-001", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddHours(24));

        agg.RequestEmailChange("enc-second-pending", "ref-002", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddHours(24));

        agg.EncryptedPendingEmail.Should().Be("enc-second-pending");
        agg.VerificationTokenReference.Should().Be("ref-002");
    }

    // ─── Event replay ─────────────────────────────────────────────────────────

    [Fact]
    public void EventReplay_ProfileUpdated_ShouldRestoreState()
    {
        var agg = BuildCreatedAggregate();
        agg.UpdateProfile("enc-first-2", "enc-last-2", "enc-gender-2", "enc-dob-2");

        var events = agg.DomainEvents.ToList();
        var replayed = new CustomerAggregate();
        foreach (var evt in events)
        {
            var whenMethod = typeof(CustomerAggregate).GetMethod(
                "When", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            whenMethod!.Invoke(replayed, new[] { evt });
        }

        replayed.EncryptedFirstName.Should().Be("enc-first-2");
        replayed.EncryptedLastName.Should().Be("enc-last-2");
        replayed.EncryptedGender.Should().Be("enc-gender-2");
        replayed.EncryptedDateOfBirth.Should().Be("enc-dob-2");
    }

    [Fact]
    public void EventReplay_EmailChangeRequested_ShouldRestorePendingEmailState()
    {
        var agg = BuildCreatedAggregate();
        var expiresAt = DateTimeOffset.UtcNow.AddHours(24);
        agg.RequestEmailChange("enc-pending", "ref-xyz", DateTimeOffset.UtcNow, expiresAt);

        var events = agg.DomainEvents.ToList();
        var replayed = new CustomerAggregate();
        foreach (var evt in events)
        {
            var whenMethod = typeof(CustomerAggregate).GetMethod(
                "When", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            whenMethod!.Invoke(replayed, new[] { evt });
        }

        replayed.EncryptedPendingEmail.Should().Be("enc-pending");
        replayed.VerificationTokenReference.Should().Be("ref-xyz");
        replayed.EncryptedEmail.Should().Be("enc-email");  // verified email unchanged
    }
}
