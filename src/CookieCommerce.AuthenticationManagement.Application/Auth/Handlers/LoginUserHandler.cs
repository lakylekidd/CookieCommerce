using CookieCommerce.AuthenticationManagement.Application.Auth.Commands;
using CookieCommerce.AuthenticationManagement.Application.Auth.ReadModels;
using CookieCommerce.AuthenticationManagement.Application.Keycloak;
using CookieCommerce.AuthenticationManagement.Domain.Aggregates;
using Marten;
using CookieCommerce.AuthenticationManagement.Application.Auth.Integration;
using MediatR;

namespace CookieCommerce.AuthenticationManagement.Application.Auth.Handlers;

public class LoginUserHandler(
    IDocumentSession session,
    IKeycloakTokenService keycloakTokenService,
    IKeycloakAdminService keycloakAdmin) : IRequestHandler<LoginUser, LoginResult>
{
    public async Task<LoginResult> Handle(LoginUser command, CancellationToken cancellationToken)
    {
        command = command with { Email = command.Email.Trim().ToLowerInvariant() };
        var tokenResponse = await keycloakTokenService.LoginAsync(command.Email, command.Password, cancellationToken);

        var userInfo = await keycloakAdmin.GetUserByEmailAsync(command.Email, cancellationToken);
        var identityId = userInfo?.Id ?? "";

        var readModel = await session.Query<AuthUserReadModel>()
            .Where(u => u.KeycloakId == identityId)
            .FirstOrDefaultAsync(cancellationToken);

        if (readModel is null && userInfo is not null)
        {
            var id = Guid.CreateVersion7();
            var aggregate = new AuthUserAggregate();
            aggregate.Register(id, userInfo.Id, userInfo.Email, userInfo.FirstName, userInfo.LastName, userInfo.CustomerId);
            session.Events.StartStream<AuthUserAggregate>(id, aggregate.DomainEvents);
            session.Store(new RegistrationDelivery { Id = id, Message = new UserRegisteredIntegrationEvent
            {
                UserId = id, CustomerId = userInfo.CustomerId, KeycloakId = userInfo.Id, Email = userInfo.Email,
                FirstName = userInfo.FirstName, LastName = userInfo.LastName, RegisteredAt = DateTime.UtcNow
            } });
            await session.SaveChangesAsync(cancellationToken);
        }

        if (readModel != null)
        {
            var stream = await session.Events.FetchForExclusiveWriting<AuthUserAggregate>(readModel.Id, cancellationToken);
            var aggregate = stream.Aggregate;
            if (aggregate != null)
            {
                aggregate.ClearUncommitedEvents();
                aggregate.RecordLogin(Guid.CreateVersion7().ToString());
                stream.AppendMany(aggregate.DomainEvents.ToArray());
                await session.SaveChangesAsync(cancellationToken);
            }
        }

        var roles = userInfo is null
            ? []
            : await keycloakAdmin.GetRealmRolesAsync(userInfo.Id, cancellationToken);

        return new LoginResult
        {
            AccessToken = tokenResponse.AccessToken,
            RefreshToken = tokenResponse.RefreshToken,
            ExpiresIn = tokenResponse.ExpiresIn,
            UserId = userInfo?.CustomerId ?? (Guid.TryParse(userInfo?.Id, out var customerId) ? customerId : Guid.Empty),
            Email = command.Email,
            FirstName = userInfo?.FirstName ?? "",
            LastName = userInfo?.LastName ?? "",
            Role = roles.Any(role => string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
                ? "Admin"
                : "User"
        };
    }
}
