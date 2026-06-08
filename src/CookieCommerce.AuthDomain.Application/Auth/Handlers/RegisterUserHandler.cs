using CookieCommerce.AuthDomain.Application.Auth.Commands;
using CookieCommerce.AuthDomain.Application.Keycloak;
using CookieCommerce.AuthDomain.Domain.Aggregates;
using CookieCommerce.AuthDomain.Domain.Exceptions;
using Marten;
using MediatR;

namespace CookieCommerce.AuthDomain.Application.Auth.Handlers;

public class RegisterUserHandler(
    IDocumentSession session,
    IKeycloakAdminService keycloakAdmin) : IRequestHandler<RegisterUser, Guid>
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

        return userId;
    }
}
