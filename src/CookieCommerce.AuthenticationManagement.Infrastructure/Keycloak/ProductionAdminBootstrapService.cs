using CookieCommerce.AuthenticationManagement.Application.Auth.Commands;
using CookieCommerce.AuthenticationManagement.Application.Keycloak;
using CookieCommerce.AuthenticationManagement.Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CookieCommerce.AuthenticationManagement.Infrastructure.Keycloak;

internal sealed class ProductionAdminBootstrapService(
    IServiceScopeFactory scopeFactory,
    IOptions<ProductionAdminOptions> options,
    ILogger<ProductionAdminBootstrapService> logger,
    IHostEnvironment? environment = null) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var admin = options.Value;
        if (environment?.IsProduction() != true || !admin.Enabled)
            return;

        if (string.IsNullOrWhiteSpace(admin.Email) || string.IsNullOrWhiteSpace(admin.InitialPassword))
        {
            logger.LogError("Production admin bootstrap is enabled but its email or initial password is missing");
            return;
        }

        for (var attempt = 1; attempt <= 15 && !stoppingToken.IsCancellationRequested; attempt++)
        {
            try
            {
                await EnsureAdminAsync(admin, stoppingToken);
                return;
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or InvalidOperationException)
            {
                if (attempt == 15)
                {
                    logger.LogError(ex, "Production admin account '{Email}' could not be bootstrapped", admin.Email);
                    return;
                }

                logger.LogWarning("Production admin bootstrap is waiting for Keycloak (attempt {Attempt}/15)", attempt);
                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
            }
        }
    }

    private async Task EnsureAdminAsync(ProductionAdminOptions admin, CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var identities = scope.ServiceProvider.GetRequiredService<IKeycloakAdminService>();
        var email = admin.Email.Trim().ToLowerInvariant();
        var identity = await identities.GetUserByEmailAsync(email, cancellationToken);

        if (identity is null)
        {
            try
            {
                await scope.ServiceProvider.GetRequiredService<IMediator>().Send(
                    new RegisterUser(email, admin.InitialPassword, admin.FirstName, admin.LastName),
                    cancellationToken);
            }
            catch (EmailAlreadyTakenException)
            {
                // A concurrent instance created it; the lookup below completes the role setup.
            }

            identity = await identities.GetUserByEmailAsync(email, cancellationToken)
                ?? throw new InvalidOperationException("The production admin identity was not available after registration.");
            logger.LogInformation("Production admin account '{Email}' was created with its configured initial password", email);
        }

        await identities.AssignRealmRoleAsync(identity.Id, "Admin", cancellationToken);
        logger.LogInformation("Production admin account '{Email}' has the Admin role", email);
    }
}
