using CookieCommerce.AuthenticationManagement.Application.Auth.Commands;
using CookieCommerce.AuthenticationManagement.Application.Keycloak;
using CookieCommerce.AuthenticationManagement.Domain.Exceptions;
using CookieCommerce.DevBootstrap;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CookieCommerce.AuthenticationManagement.Infrastructure.Keycloak;

internal sealed class KeycloakDevelopmentIdentityBootstrapAdapter(
    IServiceScopeFactory scopeFactory,
    ILogger<KeycloakDevelopmentIdentityBootstrapAdapter> logger) : IDevelopmentIdentityBootstrapAdapter
{
    public async Task EnsureUserAsync(
        string email,
        string firstName,
        string lastName,
        string password,
        string role,
        CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var keycloakAdmin = scope.ServiceProvider.GetRequiredService<IKeycloakAdminService>();

        try
        {
            await mediator.Send(new RegisterUser(email, password, firstName, lastName), cancellationToken);
            logger.LogInformation("Seed identity user '{Email}' created", email);
        }
        catch (EmailAlreadyTakenException)
        {
            logger.LogInformation("Seed identity user '{Email}' already exists — ensuring role", email);
        }

        var user = await keycloakAdmin.GetUserByEmailAsync(email, cancellationToken);
        if (user is null)
        {
            logger.LogWarning("Seed identity user '{Email}' was not found after registration; role '{Role}' was not assigned", email, role);
            return;
        }

        await keycloakAdmin.AssignRealmRoleAsync(user.Id, role, cancellationToken);
        logger.LogInformation("Seed identity user '{Email}' has Keycloak realm role '{Role}'", email, role);
    }
}
