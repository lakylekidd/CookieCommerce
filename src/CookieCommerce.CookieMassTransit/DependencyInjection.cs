using CookieCommerce.CustomerDomain.Application;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CookieCommerce.CookieMassTransit;

public static class DependencyInjection
{
    public static IServiceCollection AddCookieMassTransit(this IServiceCollection services, IConfiguration configuration)
    {
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
