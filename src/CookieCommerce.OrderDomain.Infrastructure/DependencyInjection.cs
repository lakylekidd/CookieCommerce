using CookieCommerce.OrderDomain.Infrastructure.Consumers;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CookieCommerce.OrderDomain.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Register MassTransit consumers for integration events
        services.AddMassTransit(x =>
        {
            // Register consumers for customer integration events
            x.AddConsumer<CustomerSnapshotPublishedConsumer>();
            x.AddConsumer<CustomerDeletedConsumer>();

            x.SetKebabCaseEndpointNameFormatter();

            x.UsingRabbitMq((ctx, cfg) =>
            {
                var connectionString = configuration.GetConnectionString("Messaging");
                ArgumentNullException.ThrowIfNull(connectionString, "Connection string 'Messaging' not found.");
                cfg.Host(connectionString);

                cfg.ConfigureEndpoints(ctx);
            });
        });

        return services;
    }
}
