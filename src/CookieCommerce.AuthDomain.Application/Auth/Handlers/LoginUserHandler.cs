using CookieCommerce.AuthDomain.Application.Auth.Commands;
using CookieCommerce.AuthDomain.Application.Auth.ReadModels;
using CookieCommerce.AuthDomain.Application.Keycloak;
using CookieCommerce.AuthDomain.Domain.Aggregates;
using Marten;
using MediatR;

namespace CookieCommerce.AuthDomain.Application.Auth.Handlers;

public class LoginUserHandler(
    IDocumentSession session,
    IKeycloakTokenService keycloakTokenService,
    IKeycloakAdminService keycloakAdmin) : IRequestHandler<LoginUser, LoginResult>
{
    public async Task<LoginResult> Handle(LoginUser command, CancellationToken cancellationToken)
    {
        var tokenResponse = await keycloakTokenService.LoginAsync(command.Email, command.Password, cancellationToken);

        var userInfo = await keycloakAdmin.GetUserByEmailAsync(command.Email, cancellationToken);

        var readModel = await session.Query<AuthUserReadModel>()
            .Where(u => u.Email == command.Email)
            .FirstOrDefaultAsync(cancellationToken);

        if (readModel != null)
        {
            var aggregate = await session.Events.AggregateStreamAsync<AuthUserAggregate>(readModel.Id, token: cancellationToken);
            if (aggregate != null)
            {
                aggregate.RecordLogin(Guid.CreateVersion7().ToString());
                session.Events.Append(aggregate.Id, aggregate.DomainEvents);
                await session.SaveChangesAsync(cancellationToken);
            }
        }

        return new LoginResult
        {
            AccessToken = tokenResponse.AccessToken,
            RefreshToken = tokenResponse.RefreshToken,
            ExpiresIn = tokenResponse.ExpiresIn,
            UserId = readModel?.Id ?? Guid.Empty,
            Email = command.Email,
            FirstName = userInfo?.FirstName ?? "",
            LastName = userInfo?.LastName ?? ""
        };
    }
}
