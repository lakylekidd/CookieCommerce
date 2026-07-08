using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CookieCommerce.DevBootstrap;

public sealed class DevelopmentBootstrapService(
    IServiceScopeFactory scopeFactory,
    ILogger<DevelopmentBootstrapService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Development bootstrap starting");

        using var scope = scopeFactory.CreateScope();
        var identityBootstrapAdapter = scope.ServiceProvider.GetRequiredService<IDevelopmentIdentityBootstrapAdapter>();

        foreach (var customer in DevelopmentBootstrapData.Customers)
        {
            await EnsureUserWithRetryAsync(
                identityBootstrapAdapter,
                customer.Email,
                customer.FirstName,
                customer.LastName,
                customer.Password,
                stoppingToken);
        }

        logger.LogInformation("Development bootstrap complete");
    }

    private async Task EnsureUserWithRetryAsync(
        IDevelopmentIdentityBootstrapAdapter identityBootstrapAdapter,
        string email,
        string firstName,
        string lastName,
        string password,
        CancellationToken cancellationToken)
    {
        for (var attempt = 1; attempt <= 15; attempt++)
        {
            try
            {
                await identityBootstrapAdapter.EnsureUserAsync(email, firstName, lastName, password, cancellationToken);
                return;
            }
            catch (Exception ex) when (attempt < 15)
            {
                logger.LogWarning(ex, "Development bootstrap could not ensure '{Email}' on attempt {Attempt}; retrying", email, attempt);
                await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);
            }
        }

        await identityBootstrapAdapter.EnsureUserAsync(email, firstName, lastName, password, cancellationToken);
    }
}

public interface IDevelopmentIdentityBootstrapAdapter
{
    Task EnsureUserAsync(
        string email,
        string firstName,
        string lastName,
        string password,
        CancellationToken cancellationToken);
}
