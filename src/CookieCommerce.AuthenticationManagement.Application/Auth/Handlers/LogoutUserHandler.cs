using CookieCommerce.AuthenticationManagement.Application.Auth.Commands;
using CookieCommerce.AuthenticationManagement.Application.Keycloak;
using MediatR;

namespace CookieCommerce.AuthenticationManagement.Application.Auth.Handlers;

public class LogoutUserHandler(IKeycloakTokenService keycloakTokenService) : IRequestHandler<LogoutUser>
{
    public async Task Handle(LogoutUser command, CancellationToken cancellationToken)
    {
        await keycloakTokenService.RevokeTokenAsync(command.RefreshToken, cancellationToken);
    }
}
