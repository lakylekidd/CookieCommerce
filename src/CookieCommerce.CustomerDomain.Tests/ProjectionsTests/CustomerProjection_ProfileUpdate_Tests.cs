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

public class CustomerProjection_ProfileUpdate_Tests
{
    private static (CustomerProjection Projection, IQuerySession Session, CustomerKeyRecord KeyRecord) Build(
        Guid customerId,
        byte[] key,
        Action<IEncryptionService>? configureEncryption = null)
    {
        var encryption = Substitute.For<IEncryptionService>();
        configureEncryption?.Invoke(encryption);

        var session = Substitute.For<IQuerySession>();
        var keyRecord = new CustomerKeyRecord { Id = customerId, Key = key, CreatedAt = DateTime.UtcNow };
        session.LoadAsync<CustomerKeyRecord>(customerId, Arg.Any<CancellationToken>()).Returns(keyRecord);

        return (new CustomerProjection(encryption), session, keyRecord);
    }

    [Fact]
    public async Task Apply_ProfileUpdated_ShouldDecryptAndUpdateReadModel()
    {
        var id = Guid.NewGuid();
        var key = new byte[] { 1, 2, 3 };
        var (projection, session, keyRecord) = Build(id, key, enc =>
        {
            enc.Decrypt("enc-first-2", key).Returns("Jamie");
            enc.Decrypt("enc-last-2", key).Returns("Baker");
            enc.Decrypt("enc-Female", key).Returns("Female");
            enc.Decrypt("enc-dob", key).Returns("1990-05-12");
        });

        var current = new CustomerReadModel
        {
            Id = id,
            Email = "jamie@example.com",
            FirstName = "Old",
            LastName = "Name",
            Gender = CustomerProfileGender.Male,
            Addresses = []
        };

        var evt = new CustomerProfileUpdated
        {
            CustomerId = id,
            EncryptedFirstName = "enc-first-2",
            EncryptedLastName = "enc-last-2",
            EncryptedGender = "enc-Female",
            EncryptedDateOfBirth = "enc-dob",
            UpdatedAtUtc = DateTimeOffset.UtcNow
        };

        var result = await projection.Apply(evt, current, session);

        result.FirstName.Should().Be("Jamie");
        result.LastName.Should().Be("Baker");
        result.Gender.Should().Be(CustomerProfileGender.Female);
        result.DateOfBirth.Should().Be(new DateOnly(1990, 5, 12));
        result.Email.Should().Be("jamie@example.com");  // email unchanged
    }

    [Fact]
    public async Task Apply_ProfileUpdated_NullGender_ShouldSetGenderToNull()
    {
        var id = Guid.NewGuid();
        var key = new byte[] { 1, 2, 3 };
        var (projection, session, _) = Build(id, key, enc =>
        {
            enc.Decrypt(Arg.Any<string>(), key).Returns("Jamie");
        });

        var current = new CustomerReadModel
        {
            Id = id,
            Email = "jamie@example.com",
            FirstName = "Old",
            LastName = "Name",
            Gender = CustomerProfileGender.Male,
            Addresses = []
        };

        var evt = new CustomerProfileUpdated
        {
            CustomerId = id,
            EncryptedFirstName = "enc-first",
            EncryptedLastName = "enc-last",
            EncryptedGender = null,   // cleared
            EncryptedDateOfBirth = null,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        };

        var result = await projection.Apply(evt, current, session);

        result.Gender.Should().BeNull();
        result.DateOfBirth.Should().BeNull();
    }

    [Fact]
    public async Task Apply_ProfileUpdated_MissingKey_ShouldLeaveExistingValuesIntact()
    {
        var id = Guid.NewGuid();
        var encryption = Substitute.For<IEncryptionService>();
        var session = Substitute.For<IQuerySession>();

        // No key available (shredded)
        session.LoadAsync<CustomerKeyRecord>(id, Arg.Any<CancellationToken>()).Returns((CustomerKeyRecord?)null);

        var projection = new CustomerProjection(encryption);
        var current = new CustomerReadModel
        {
            Id = id,
            Email = "existing@example.com",
            FirstName = "Existing",
            LastName = "User",
            Addresses = []
        };

        var evt = new CustomerProfileUpdated
        {
            CustomerId = id,
            EncryptedFirstName = "enc",
            EncryptedLastName = "enc",
            UpdatedAtUtc = DateTimeOffset.UtcNow
        };

        var result = await projection.Apply(evt, current, session);

        // Existing values must be preserved when key is missing
        result.FirstName.Should().Be("Existing");
        result.LastName.Should().Be("User");
    }

    [Fact]
    public async Task Apply_EmailChangeRequested_ShouldSetPendingEmailAndStatus()
    {
        var id = Guid.NewGuid();
        var key = new byte[] { 1, 2, 3 };
        var (projection, session, _) = Build(id, key, enc =>
        {
            enc.Decrypt("enc-pending", key).Returns("jamie.new@example.com");
        });

        var current = new CustomerReadModel
        {
            Id = id,
            Email = "jamie@example.com",
            EmailVerificationStatus = "Verified",
            Addresses = [],
            FirstName = "Jamie",
            LastName = "Baker"
        };

        var evt = new CustomerEmailChangeRequested
        {
            CustomerId = id,
            EncryptedPendingEmail = "enc-pending",
            VerificationTokenReference = "token-ref-abc",
            RequestedAtUtc = DateTimeOffset.UtcNow,
            ExpiresAtUtc = DateTimeOffset.UtcNow.AddHours(24)
        };

        var result = await projection.Apply(evt, current, session);

        result.PendingEmail.Should().Be("jamie.new@example.com");
        result.EmailVerificationStatus.Should().Be("PendingVerification");
        result.Email.Should().Be("jamie@example.com");  // verified email unchanged
    }

    [Fact]
    public async Task Apply_VerifiedEmailChanged_ShouldPromoteEmailAndClearPending()
    {
        var id = Guid.NewGuid();
        var key = new byte[] { 1, 2, 3 };
        var (projection, session, _) = Build(id, key, enc =>
        {
            enc.Decrypt("enc-new-email", key).Returns("jamie.new@example.com");
        });

        var current = new CustomerReadModel
        {
            Id = id,
            Email = "jamie@example.com",
            PendingEmail = "jamie.new@example.com",
            EmailVerificationStatus = "PendingVerification",
            Addresses = [],
            FirstName = "Jamie",
            LastName = "Baker"
        };

        var evt = new CustomerVerifiedEmailChanged
        {
            CustomerId = id,
            EncryptedPreviousEmail = "enc-old-email",
            EncryptedVerifiedEmail = "enc-new-email",
            VerifiedAtUtc = DateTimeOffset.UtcNow
        };

        var result = await projection.Apply(evt, current, session);

        result.Email.Should().Be("jamie.new@example.com");
        result.PendingEmail.Should().BeNull();
        result.EmailVerificationStatus.Should().Be("Verified");
    }
}
