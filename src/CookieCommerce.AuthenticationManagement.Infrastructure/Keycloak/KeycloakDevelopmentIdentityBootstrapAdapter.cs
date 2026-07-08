using CookieCommerce.AuthenticationManagement.Application.Auth.Commands;
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
        CancellationToken cancellationToken)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            await mediator.Send(new RegisterUser(email, password, firstName, lastName), cancellationToken);
            logger.LogInformation("Seed identity user '{Email}' created", email);
        }
        catch (EmailAlreadyTakenException)
        {
            logger.LogInformation("Seed identity user '{Email}' already exists — skipping", email);
        }
    }
}
