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
        command = command with { Email = command.Email.Trim().ToLowerInvariant() };
        var existing = await keycloakAdmin.GetUserByEmailAsync(command.Email, cancellationToken);
        if (existing != null)
            throw new EmailAlreadyTakenException(command.Email);

        var keycloakId = await keycloakAdmin.RegisterUserAsync(
            command.Email, command.Password, command.FirstName, command.LastName, cancellationToken);
        await keycloakAdmin.AssignRealmRoleAsync(keycloakId, "User", cancellationToken);

        var userId = Guid.CreateVersion7();
        var user = new AuthUserAggregate();
        user.Register(userId, keycloakId, command.Email, command.FirstName, command.LastName);

        session.Events.StartStream<AuthUserAggregate>(userId, user.DomainEvents);
        var delivery = new RegistrationDelivery { Id = userId, Message = new UserRegisteredIntegrationEvent
        {
            UserId = userId,
            KeycloakId = keycloakId,
            Email = command.Email,
            FirstName = command.FirstName,
            LastName = command.LastName,
            RegisteredAt = DateTime.UtcNow
        } };
        session.Store(delivery);
        await session.SaveChangesAsync(cancellationToken);
        // The persisted delivery survives process failure and broker downtime.
        try
        {
            await publisher.PublishAsync(delivery.Message, cancellationToken);
            delivery.PublishedAt = DateTimeOffset.UtcNow;
            session.Store(delivery);
            await session.SaveChangesAsync(cancellationToken);
        }
        catch (Exception) when (!cancellationToken.IsCancellationRequested) { /* Retried by RegistrationDeliveryWorker. */ }

        return userId;
    }
}
