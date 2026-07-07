using CookieCommerce.CustomerDomain.Application.Customers.Handlers;
using CookieCommerce.CustomerDomain.Application.Customers.Integration;
using CookieCommerce.CustomerDomain.Application.Customers.Profile;
using CookieCommerce.CustomerDomain.Application.Customers.ReadModels;
using CookieCommerce.CustomerDomain.Application.Security;
using CookieCommerce.CustomerDomain.Domain.Aggregates;
using CookieCommerce.CustomerDomain.Domain.Exceptions;
using CookieCommerce.CustomerDomain.Domain.ValueObjects;
using FluentAssertions;
using Marten;
using Marten.Events;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace CookieCommerce.CustomerDomain.Tests.ApplicationTests;

public class CustomerProfileHandlerTests
{
    // ─── GetCurrentCustomerProfileQueryHandler ────────────────────────────────

    [Fact]
    public async Task GetCurrentCustomerProfile_ReturnsProfile_WhenFound()
    {
        var customerId = Guid.NewGuid();
        var readModel = new CustomerReadModel
        {
            Id = customerId,
            Email = "jamie@example.com",
            FirstName = "Jamie",
            LastName = "Baker",
            Gender = CustomerProfileGender.Female,
            DateOfBirth = new DateOnly(1990, 5, 12),
            EmailVerificationStatus = "Verified"
        };

        var querySession = Substitute.For<IQuerySession>();
        querySession.LoadAsync<CustomerReadModel>(customerId, Arg.Any<CancellationToken>()).Returns(readModel);

        var handler = new GetCurrentCustomerProfileQueryHandler(querySession);
        var result = await handler.Handle(new GetCurrentCustomerProfileQuery(customerId), default);

        result.Id.Should().Be(customerId);
        result.FirstName.Should().Be("Jamie");
        result.LastName.Should().Be("Baker");
        result.Email.Should().Be("jamie@example.com");
        result.Gender.Should().Be(CustomerProfileGender.Female);
        result.DateOfBirth.Should().Be(new DateOnly(1990, 5, 12));
        result.EmailVerificationStatus.Should().Be(EmailVerificationStatus.Verified);
    }

    [Fact]
    public async Task GetCurrentCustomerProfile_ThrowsMissingProfile_WhenNotFound()
    {
        var customerId = Guid.NewGuid();
        var querySession = Substitute.For<IQuerySession>();
        querySession.LoadAsync<CustomerReadModel>(customerId, Arg.Any<CancellationToken>())
            .Returns((CustomerReadModel?)null);

        var handler = new GetCurrentCustomerProfileQueryHandler(querySession);

        Func<Task> act = () => handler.Handle(new GetCurrentCustomerProfileQuery(customerId), default);

        await act.Should().ThrowAsync<MissingCustomerProfileException>();
    }

    [Fact]
    public async Task GetCurrentCustomerProfile_MapsPendingVerificationStatus()
    {
        var customerId = Guid.NewGuid();
        var readModel = new CustomerReadModel
        {
            Id = customerId,
            Email = "jamie@example.com",
            PendingEmail = "jamie.new@example.com",
            EmailVerificationStatus = "PendingVerification",
            FirstName = "Jamie",
            LastName = "Baker"
        };

        var querySession = Substitute.For<IQuerySession>();
        querySession.LoadAsync<CustomerReadModel>(customerId, Arg.Any<CancellationToken>()).Returns(readModel);

        var handler = new GetCurrentCustomerProfileQueryHandler(querySession);
        var result = await handler.Handle(new GetCurrentCustomerProfileQuery(customerId), default);

        result.EmailVerificationStatus.Should().Be(EmailVerificationStatus.PendingVerification);
        result.PendingEmail.Should().Be("jamie.new@example.com");
    }

    // ─── UpdateCustomerProfileCommandHandler ─────────────────────────────────

    private static UpdateCustomerProfileCommandHandler BuildUpdateHandler(
        IDocumentSession session,
        IQuerySession querySession,
        ICustomerKeyStore keyStore,
        CustomerProfileEncryption encryption,
        CustomerEmailUniquenessService uniquenessService,
        IEmailVerificationTokenStore tokenStore,
        IEmailVerificationDeliverySender deliverySender)
    {
        var profileOutboxPublisher = Substitute.For<ICustomerProfileOutboxPublisher>();
        var options = Options.Create(new EmailVerificationDeliverySettings
        {
            VerificationBaseUrl = "https://example.com",
            VerificationRoute = "/account/email-verification/confirm",
            TokenExpiry = TimeSpan.FromHours(24)
        });

        return new UpdateCustomerProfileCommandHandler(
            session,
            querySession,
            keyStore,
            encryption,
            new CustomerProfileValidator(),
            uniquenessService,
            tokenStore,
            deliverySender,
            profileOutboxPublisher,
            options,
            NullLogger<UpdateCustomerProfileCommandHandler>.Instance);
    }

    [Fact]
    public async Task UpdateProfile_WithNoEmailChange_ShouldNotCreatePendingEmail()
    {
        var customerId = Guid.NewGuid();
        var key = new byte[32];

        var session = Substitute.For<IDocumentSession>();
        var querySession = Substitute.For<IQuerySession>();
        var keyStore = Substitute.For<ICustomerKeyStore>();
        var encryptionService = Substitute.For<IEncryptionService>();
        var emailChecker = Substitute.For<ICustomerEmailUniquenessChecker>();
        var tokenStore = Substitute.For<IEmailVerificationTokenStore>();
        var deliverySender = Substitute.For<IEmailVerificationDeliverySender>();
        var eventStore = Substitute.For<IEventStoreOperations>();

        var readModel = new CustomerReadModel
        {
            Id = customerId,
            Email = "jamie@example.com",
            FirstName = "Jamie",
            LastName = "Baker",
            EmailVerificationStatus = "Verified"
        };

        session.Events.Returns(eventStore);
        querySession.LoadAsync<CustomerReadModel>(customerId, Arg.Any<CancellationToken>()).Returns(readModel);
        keyStore.GetKeyAsync(customerId, Arg.Any<CancellationToken>()).Returns(key);
        encryptionService.Encrypt(Arg.Any<string>(), Arg.Any<byte[]>()).Returns("enc");

        var aggregate = new CustomerAggregate();
        aggregate.Create(customerId, "enc-email", "enc-first", "enc-last", "enc-gender", null);
        eventStore.AggregateStreamAsync<CustomerAggregate>(customerId, token: Arg.Any<CancellationToken>())
            .Returns(aggregate);

        var uniquenessService = new CustomerEmailUniquenessService(emailChecker);
        var encryption = new CustomerProfileEncryption(encryptionService);

        var handler = BuildUpdateHandler(session, querySession, keyStore, encryption, uniquenessService, tokenStore, deliverySender);

        var command = new UpdateCustomerProfileCommand(
            CustomerId: customerId,
            FirstName: "Jamie",
            LastName: "Baker",
            Email: "jamie@example.com",  // same email — no change
            Gender: CustomerProfileGender.Female,
            DateOfBirth: new DateOnly(1990, 5, 12),
            RequestedAtUtc: DateTimeOffset.UtcNow,
            CorrelationId: "corr-001");

        await handler.Handle(command, default);

        // Token store must NOT have been called since email didn't change
        await tokenStore.DidNotReceive().CreateAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>());
        await deliverySender.DidNotReceive().SendAsync(Arg.Any<EmailVerificationDeliveryPayload>(), Arg.Any<CancellationToken>());
    }
}
