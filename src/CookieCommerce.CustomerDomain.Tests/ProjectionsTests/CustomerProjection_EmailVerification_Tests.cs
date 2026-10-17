using CookieCommerce.CustomerDomain.Application.Customers.ReadModels;
using CookieCommerce.CustomerDomain.Application.Security;
using CookieCommerce.CustomerDomain.Domain.DomainEvents;
using CookieCommerce.CustomerDomain.Infrastructure.Projections;
using CookieCommerce.CustomerDomain.Infrastructure.Security;
using FluentAssertions;
using Marten;
using NSubstitute;

namespace CookieCommerce.CustomerDomain.Tests.ProjectionsTests;

public class CustomerProjection_EmailVerification_Tests
{
    [Fact]
    public async Task Apply_VerifiedEmailChanged_PromotesVerifiedEmailAndClearsPendingEmailState()
    {
        var customerId = Guid.NewGuid();
        var key = new byte[] { 4, 5, 6 };
        var encryption = Substitute.For<IEncryptionService>();
        encryption.Decrypt("enc-new-email", key).Returns("jamie.new@example.com");
        var session = Substitute.For<IQuerySession>();
        session.LoadAsync<CustomerKeyRecord>(customerId, Arg.Any<CancellationToken>())
            .Returns(new CustomerKeyRecord { Id = customerId, Key = key, CreatedAt = DateTime.UtcNow });
        var projection = new CustomerProjection(encryption);
        var current = new CustomerReadModel
        {
            Id = customerId,
            Email = "jamie@example.com",
            PendingEmail = "jamie.new@example.com",
            EmailVerificationStatus = "PendingVerification",
            FirstName = "Jamie",
            LastName = "Baker",
            Addresses = []
        };

        var result = await projection.Apply(new CustomerVerifiedEmailChanged
        {
            CustomerId = customerId,
            EncryptedPreviousEmail = "enc-old-email",
            EncryptedVerifiedEmail = "enc-new-email",
            VerifiedAtUtc = DateTimeOffset.UtcNow
        }, current, session);

        result.Email.Should().Be("jamie.new@example.com");
        result.PendingEmail.Should().BeNull();
        result.EmailVerificationStatus.Should().Be("Verified");
    }

    [Fact]
    public void VerifyPendingEmailChange_ClearsTokenReferenceOnAggregate()
    {
        var customerId = Guid.NewGuid();
        var aggregate = new Domain.Aggregates.CustomerAggregate();
        aggregate.Create(customerId, "enc-old-email", "enc-first", "enc-last", "enc-gender", null);
        aggregate.ClearUncommitedEvents();
        aggregate.RequestEmailChange("enc-new-email", "token-ref", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddHours(1));
        aggregate.ClearUncommitedEvents();

        aggregate.VerifyPendingEmailChange(DateTimeOffset.UtcNow);

        aggregate.EncryptedEmail.Should().Be("enc-new-email");
        aggregate.EncryptedPendingEmail.Should().BeNull();
        aggregate.VerificationTokenReference.Should().BeNull();
        aggregate.VerificationExpiresAtUtc.Should().BeNull();
    }
}
