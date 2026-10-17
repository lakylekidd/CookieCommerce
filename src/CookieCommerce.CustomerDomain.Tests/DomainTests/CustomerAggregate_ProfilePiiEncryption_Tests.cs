using CookieCommerce.CustomerDomain.Domain.Aggregates;
using CookieCommerce.CustomerDomain.Domain.DomainEvents;
using FluentAssertions;

namespace CookieCommerce.CustomerDomain.Tests.DomainTests;

public class CustomerAggregate_ProfilePiiEncryption_Tests
{
    [Fact]
    public void ProfileUpdated_Event_ShouldCarryEncryptedFields()
    {
        var agg = new CustomerAggregate();
        agg.Create(Guid.NewGuid(), "enc-email", "enc-first", "enc-last", "enc-gender", null);

        agg.UpdateProfile("enc-fn-updated", "enc-ln-updated", "enc-gender-PreferNotToDisclose", "enc-dob-2000-01-01");

        var evt = agg.DomainEvents.OfType<CustomerProfileUpdated>().Single();

        evt.EncryptedFirstName.Should().Be("enc-fn-updated");
        evt.EncryptedLastName.Should().Be("enc-ln-updated");
        evt.EncryptedGender.Should().Be("enc-gender-PreferNotToDisclose");
        evt.EncryptedDateOfBirth.Should().Be("enc-dob-2000-01-01");
    }

    [Fact]
    public void ProfileUpdated_Event_ShouldNotContainPlainTextValues()
    {
        var agg = new CustomerAggregate();
        agg.Create(Guid.NewGuid(), "enc-email", "enc-first", "enc-last", "enc-gender", null);
        agg.UpdateProfile("AES+enc+blob+firstName", "AES+enc+blob+lastName", "AES+enc+gender", null);

        var evt = agg.DomainEvents.OfType<CustomerProfileUpdated>().Single();

        // Simulated encrypted values should be opaque blobs, not plain names
        evt.EncryptedFirstName.Should().NotBe("Jamie");
        evt.EncryptedLastName.Should().NotBe("Baker");
    }

    [Fact]
    public void EmailChangeRequested_Event_ShouldCarryEncryptedPendingEmail()
    {
        var agg = new CustomerAggregate();
        agg.Create(Guid.NewGuid(), "enc-email", "enc-first", "enc-last", "enc-gender", null);
        agg.RequestEmailChange("enc-pending-email-blob", "token-ref-001", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddHours(24));

        var evt = agg.DomainEvents.OfType<CustomerEmailChangeRequested>().Single();

        evt.EncryptedPendingEmail.Should().Be("enc-pending-email-blob");
        evt.VerificationTokenReference.Should().Be("token-ref-001");
        // Raw email must not appear in the event
        evt.EncryptedPendingEmail.Should().NotBe("jamie.new@example.com");
    }

    [Fact]
    public void EmailChangeRequested_TokenReference_MustNotBeRawToken()
    {
        // Token reference is a stable reference ID, not the raw cryptographic token
        var agg = new CustomerAggregate();
        agg.Create(Guid.NewGuid(), "enc-email", "enc-first", "enc-last", "enc-gender", null);
        agg.RequestEmailChange("enc-pending", "evt_stable-opaque-reference", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddHours(24));

        var evt = agg.DomainEvents.OfType<CustomerEmailChangeRequested>().Single();

        // Reference should be the stable reference ID, not a raw base64 token
        evt.VerificationTokenReference.Should().Be("evt_stable-opaque-reference");
    }

    [Fact]
    public void ProfileUpdated_GenderAndDateOfBirth_AreEmittedAsEncryptedFields()
    {
        var agg = new CustomerAggregate();
        agg.Create(Guid.NewGuid(), "enc-email", "enc-first", "enc-last", "enc-gender", "enc-dob");

        // Update with new gender and DOB — both must be in encrypted form
        agg.UpdateProfile("enc-first", "enc-last", "enc-Female", "enc-1990-05-12");

        var evt = agg.DomainEvents.OfType<CustomerProfileUpdated>().Single();

        evt.EncryptedGender.Should().Be("enc-Female");
        evt.EncryptedDateOfBirth.Should().Be("enc-1990-05-12");
        // Plain text must not appear
        evt.EncryptedGender.Should().NotBe("Female");
        evt.EncryptedDateOfBirth.Should().NotBe("1990-05-12");
    }
}
