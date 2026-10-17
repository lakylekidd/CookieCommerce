using CookieCommerce.AuthenticationManagement.Application.Auth.Commands;
using CookieCommerce.AuthenticationManagement.Application.Auth.Handlers;
using CookieCommerce.AuthenticationManagement.Application.Keycloak;
using CookieCommerce.AuthenticationManagement.Domain.Aggregates;
using FluentAssertions;
using Marten;
using Marten.Events;
using Moq;

namespace CookieCommerce.AuthenticationManagement.Tests.ConsumerTests;

public class CustomerForgottenConsumerTests
{
    [Fact]
    public async Task Handle_DisablesLoginAndReleasesActiveLookup_GivenCustomerForgottenEvent()
    {
        var customerId = Guid.NewGuid();
        var authUser = CreateActiveUser(customerId, "keycloak-123", "deleted@example.com");
        var fixture = BuildFixture(customerId, authUser);

        await fixture.Handler.Handle(new ConsumeCustomerForgotten(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            customerId,
            "subject-123",
            "CustomerForgotten"), CancellationToken.None);

        authUser.IsActive.Should().BeFalse();
        fixture.Keycloak.Verify(x => x.DisableUserAsync("keycloak-123", It.IsAny<CancellationToken>()), Times.Once);
        fixture.Session.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DoesNothing_GivenDuplicateCustomerForgottenEvent()
    {
        var customerId = Guid.NewGuid();
        var authUser = CreateActiveUser(customerId, "keycloak-123", "deleted@example.com");
        authUser.Forget(DateTimeOffset.UtcNow.AddMinutes(-5), "CustomerForgotten");
        authUser.ClearUncommitedEvents();
        var fixture = BuildFixture(customerId, authUser);

        await fixture.Handler.Handle(new ConsumeCustomerForgotten(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            customerId,
            "subject-123",
            "CustomerForgotten"), CancellationToken.None);

        authUser.IsActive.Should().BeFalse();
        fixture.Keycloak.Verify(x => x.DisableUserAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        fixture.Session.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_LeavesEmailReusableInAuthenticationBoundary_GivenForgottenAccountIsInactive()
    {
        var customerId = Guid.NewGuid();
        var authUser = CreateActiveUser(customerId, "keycloak-123", "deleted@example.com");
        var fixture = BuildFixture(customerId, authUser);

        await fixture.Handler.Handle(new ConsumeCustomerForgotten(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            customerId,
            "subject-123",
            "CustomerForgotten"), CancellationToken.None);

        authUser.Email.Should().Be("deleted@example.com");
        authUser.IsActive.Should().BeFalse();
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
        var handler = new ConsumeCustomerForgottenCommandHandler(session.Object, keycloak.Object);
        return new HandlerFixture(handler, session, keycloak);
    }

    private sealed record HandlerFixture(
        ConsumeCustomerForgottenCommandHandler Handler,
        Mock<IDocumentSession> Session,
        Mock<IKeycloakAdminService> Keycloak);
}
