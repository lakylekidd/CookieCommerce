using CookieCommerce.DomainBuildingBlocks;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CookieCommerce.CookieMassTransit;

public static class DependencyInjection
{
    public static IServiceCollection AddCookieMassTransit(this IServiceCollection services, IConfiguration configuration)
    {
        // Ensure MassTransit license is available to MassTransit by setting environment variables
        // MassTransit will read MT_LICENSE or MT_LICENSE_PATH. Prefer existing environment variables;
        // if they're not set, read configuration keys MassTransit:License and MassTransit:LicensePath.
        var envLicense = Environment.GetEnvironmentVariable("MT_LICENSE");
        var envLicensePath = Environment.GetEnvironmentVariable("MT_LICENSE_PATH");

        var configLicense = configuration["MassTransit:License"];
        var configLicensePath = configuration["MassTransit:LicensePath"];

        if (string.IsNullOrWhiteSpace(envLicense) && !string.IsNullOrWhiteSpace(configLicense))
        {
            Environment.SetEnvironmentVariable("MT_LICENSE", configLicense);
        }
        if (string.IsNullOrWhiteSpace(envLicensePath) && !string.IsNullOrWhiteSpace(configLicensePath))
        {
            Environment.SetEnvironmentVariable("MT_LICENSE_PATH", configLicensePath);
        }

        services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();

            x.UsingRabbitMq((ctx, cfg) =>
            {
                var connectionString = configuration.GetConnectionString("Messaging");
                ArgumentNullException.ThrowIfNull(connectionString, "Connection string 'Messaging' not found.");
                cfg.Host(connectionString);
            });
        });

        services.AddScoped<IIntegrationEventPublisher, MassTransitIntegrationEventPublisher>();

        return services;
    }
}
