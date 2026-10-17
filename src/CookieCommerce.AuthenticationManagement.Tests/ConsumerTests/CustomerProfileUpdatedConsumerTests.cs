using CookieCommerce.AuthenticationManagement.Application.Auth.Commands;
using CookieCommerce.AuthenticationManagement.Application.Auth.Handlers;
using CookieCommerce.AuthenticationManagement.Application.Keycloak;
using CookieCommerce.AuthenticationManagement.Domain.Aggregates;
using FluentAssertions;
using Marten;
using Marten.Events;
using Moq;

namespace CookieCommerce.AuthenticationManagement.Tests.ConsumerTests;

public class CustomerProfileUpdatedConsumerTests
{
    [Fact]
    public async Task Handle_UpdatesKeycloakProfile_GivenCustomerProfileUpdatedEvent()
    {
        var customerId = Guid.NewGuid();
        var authUser = CreateActiveUser(customerId, "keycloak-123", "old@example.com");
        var fixture = BuildFixture(customerId, authUser);
        var dob = new DateOnly(1990, 5, 12);

        await fixture.Handler.Handle(new ConsumeCustomerProfileUpdated(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            customerId,
            "NewFirst",
            "NewLast",
            "Female",
            dob), CancellationToken.None);

        authUser.FirstName.Should().Be("NewFirst");
        authUser.LastName.Should().Be("NewLast");
        fixture.Keycloak.Verify(x => x.UpdateUserProfileAsync(
            "keycloak-123",
            "NewFirst",
            "NewLast",
            "Female",
            dob,
            It.IsAny<CancellationToken>()), Times.Once);
        fixture.Session.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private static AuthUserAggregate CreateActiveUser(Guid customerId, string keycloakId, string email)
    {
        var authUser = new AuthUserAggregate();
        authUser.Register(customerId, keycloakId, email, "Jamie", "Baker");
        authUser.ClearUncommitedEvents();
        return authUser;
    }

    private static HandlerFixture BuildFixture(Guid customerId, AuthUserAggregate authUser)
    {
        var session = new Mock<IDocumentSession>();
        var events = new Mock<IEventStoreOperations>();
        var keycloak = new Mock<IKeycloakAdminService>();
        session.SetupGet(x => x.Events).Returns(events.Object);
        events.Setup(x => x.AggregateStreamAsync<AuthUserAggregate>(
                customerId,
                It.IsAny<long>(),
                It.IsAny<DateTimeOffset?>(),
                It.IsAny<AuthUserAggregate?>(),
                It.IsAny<long>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(authUser);
        var handler = new ConsumeCustomerProfileUpdatedCommandHandler(session.Object, keycloak.Object);
        return new HandlerFixture(handler, session, keycloak);
    }

    private sealed record HandlerFixture(
        ConsumeCustomerProfileUpdatedCommandHandler Handler,
        Mock<IDocumentSession> Session,
        Mock<IKeycloakAdminService> Keycloak);
}
