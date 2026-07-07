using CookieCommerce.AuthenticationManagement.Application.Auth.Commands;
using CookieCommerce.AuthenticationManagement.Application.Auth.Handlers;
using CookieCommerce.AuthenticationManagement.Application.Keycloak;
using CookieCommerce.AuthenticationManagement.Domain.Aggregates;
using FluentAssertions;
using Marten;
using Marten.Events;
using Moq;

namespace CookieCommerce.AuthenticationManagement.Tests.ConsumerTests;

public class EmailVerificationVerifiedCustomerEmailChangedConsumerTests
{
    [Fact]
    public async Task Handle_UpdatesLoginEmailOnce_GivenEmailVerificationVerifiedCustomerEmailChangedEvent()
    {
        var customerId = Guid.NewGuid();
        var authUser = CreateActiveUser(customerId, "keycloak-123", "old@example.com");
        var fixture = BuildFixture(customerId, authUser);

        await fixture.Handler.Handle(new ConsumeVerifiedCustomerEmailChanged(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            customerId,
            "old@example.com",
            "new@example.com"), CancellationToken.None);

        authUser.Email.Should().Be("new@example.com");
        fixture.Keycloak.Verify(x => x.UpdateUserEmailAsync("keycloak-123", "new@example.com", It.IsAny<CancellationToken>()), Times.Once);
        fixture.Session.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DoesNotUpdateLoginAgain_GivenDuplicateEmailVerificationVerifiedEmailEvent()
    {
        var customerId = Guid.NewGuid();
        var authUser = CreateActiveUser(customerId, "keycloak-123", "new@example.com");
        var fixture = BuildFixture(customerId, authUser);

        await fixture.Handler.Handle(new ConsumeVerifiedCustomerEmailChanged(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            customerId,
            "old@example.com",
            "new@example.com"), CancellationToken.None);

        authUser.Email.Should().Be("new@example.com");
        fixture.Keycloak.Verify(x => x.UpdateUserEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        fixture.Session.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
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
        var handler = new ConsumeVerifiedCustomerEmailChangedCommandHandler(session.Object, keycloak.Object);
        return new HandlerFixture(handler, session, keycloak);
    }

    private sealed record HandlerFixture(
        ConsumeVerifiedCustomerEmailChangedCommandHandler Handler,
        Mock<IDocumentSession> Session,
        Mock<IKeycloakAdminService> Keycloak);
}
