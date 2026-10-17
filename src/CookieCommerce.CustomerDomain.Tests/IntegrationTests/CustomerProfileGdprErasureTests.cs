using CookieCommerce.CustomerDomain.Application.Customers.Commands;
using CookieCommerce.CustomerDomain.Application.Customers.Handlers;
using CookieCommerce.CustomerDomain.Application.Customers.Integration;
using CookieCommerce.CustomerDomain.Application.Customers.ReadModels;
using CookieCommerce.CustomerDomain.Application.Security;
using CookieCommerce.CustomerDomain.Domain.Aggregates;
using CookieCommerce.CustomerDomain.Domain.DomainEvents;
using FluentAssertions;
using Marten;
using Marten.Events;
using NSubstitute;

namespace CookieCommerce.CustomerDomain.Tests.IntegrationTests;

public class CustomerProfileGdprErasureTests
{
    [Fact]
    public void ProfileEvents_StoreEncryptedPii_GivenUpdatedProfileAndPendingEmail()
    {
        var customerId = Guid.NewGuid();
        var aggregate = new CustomerAggregate();
        aggregate.Create(customerId, "enc-email", "enc-first", "enc-last", "enc-gender", "enc-dob");
        aggregate.ClearUncommitedEvents();

        aggregate.UpdateProfile("enc-first-updated", "enc-last-updated", "enc-gender-updated", "enc-dob-updated");
        aggregate.RequestEmailChange("enc-pending-email", "token-ref", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddHours(1));

        aggregate.DomainEvents.OfType<CustomerProfileUpdated>().Single().Should().Match<CustomerProfileUpdated>(e =>
            e.EncryptedFirstName == "enc-first-updated" &&
            e.EncryptedLastName == "enc-last-updated" &&
            e.EncryptedGender == "enc-gender-updated" &&
            e.EncryptedDateOfBirth == "enc-dob-updated");
        aggregate.DomainEvents.OfType<CustomerEmailChangeRequested>().Single().EncryptedPendingEmail.Should().Be("enc-pending-email");
        var eventText = string.Join(Environment.NewLine, aggregate.DomainEvents.Select(e => e.ToString()));
        eventText.Should().NotContain("Jamie");
        eventText.Should().NotContain("Baker");
        eventText.Should().NotContain("jamie@example.com");
        eventText.Should().NotContain("1990-05-12");
        eventText.Should().NotContain("Female");
    }

    [Fact]
    public async Task ForgetCustomer_DeletesKeyAndReadModelAndPublishesPiiFreeEvent()
    {
        var customerId = Guid.NewGuid();
        var session = Substitute.For<IDocumentSession>();
        var eventStore = Substitute.For<IEventStoreOperations>();
        var keyStore = Substitute.For<ICustomerKeyStore>();
        var forgottenOutbox = Substitute.For<ICustomerForgottenOutboxPublisher>();
        var aggregate = new CustomerAggregate();
        aggregate.Create(customerId, "enc-email", "enc-first", "enc-last", "enc-gender", "enc-dob");
        aggregate.ClearUncommitedEvents();
        session.Events.Returns(eventStore);
        eventStore.AggregateStreamAsync<CustomerAggregate>(customerId, token: Arg.Any<CancellationToken>()).Returns(aggregate);
        var handler = new ForgetCustomerHandler(session, keyStore, forgottenOutbox);

        await handler.Handle(new ForgetCustomer(customerId), CancellationToken.None);

        await keyStore.Received(1).DeleteKeyAsync(customerId, Arg.Any<CancellationToken>());
        session.Received(1).DeleteWhere(Arg.Any<System.Linq.Expressions.Expression<Func<CustomerReadModel, bool>>>());
        await forgottenOutbox.Received(1).PublishCustomerForgottenAsync(
            customerId,
            Arg.Any<string?>(),
            "CustomerForgotten",
            Arg.Any<DateTimeOffset>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
    }
}
