using CookieCommerce.AuthenticationManagement.Application.Auth.Commands;
using CookieCommerce.AuthenticationManagement.Application.Auth.Integration;
using CookieCommerce.AuthenticationManagement.Application.Keycloak;
using CookieCommerce.AuthenticationManagement.Domain.Aggregates;
using CookieCommerce.AuthenticationManagement.Domain.Exceptions;
using CookieCommerce.DomainBuildingBlocks;
using Marten;
using MediatR;

namespace CookieCommerce.AuthenticationManagement.Application.Auth.Handlers;

public class RegisterUserHandler(
    IDocumentSession session,
    IKeycloakAdminService keycloakAdmin,
    IIntegrationEventPublisher publisher) : IRequestHandler<RegisterUser, Guid>
{
    public async Task<Guid> Handle(RegisterUser command, CancellationToken cancellationToken)
    {
        var existing = await keycloakAdmin.GetUserByEmailAsync(command.Email, cancellationToken);
        if (existing != null)
            throw new EmailAlreadyTakenException(command.Email);

        var keycloakId = await keycloakAdmin.RegisterUserAsync(
            command.Email, command.Password, command.FirstName, command.LastName, cancellationToken);

        var userId = Guid.CreateVersion7();
        var user = new AuthUserAggregate();
        user.Register(userId, keycloakId, command.Email, command.FirstName, command.LastName);

        session.Events.StartStream<AuthUserAggregate>(userId, user.DomainEvents);
        await session.SaveChangesAsync(cancellationToken);

        await publisher.PublishAsync(new UserRegisteredIntegrationEvent
        {
            UserId = userId,
            KeycloakId = keycloakId,
            Email = command.Email,
            FirstName = command.FirstName,
            LastName = command.LastName,
            RegisteredAt = DateTime.UtcNow
        }, cancellationToken);

        return userId;
    }
}
