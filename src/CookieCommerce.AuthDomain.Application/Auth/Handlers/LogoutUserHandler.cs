using CookieCommerce.AuthDomain.Application.Auth.Commands;
using CookieCommerce.AuthDomain.Application.Keycloak;
using MediatR;

namespace CookieCommerce.AuthDomain.Application.Auth.Handlers;

public class LogoutUserHandler(IKeycloakTokenService keycloakTokenService) : IRequestHandler<LogoutUser>
{
    public async Task Handle(LogoutUser command, CancellationToken cancellationToken)
    {
        await keycloakTokenService.RevokeTokenAsync(command.RefreshToken, cancellationToken);
    }
}
