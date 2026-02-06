using CookieCommerce.CustomerDomain.Infrastructure.Projections;

using Marten;

namespace CookieCommerce.CustomerDomain.Api.DependencyInjection;

public static class MartenExtensions
{
    public static IServiceCollection AddMartenConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMarten(opts =>
        {
            var connectionString = configuration.GetConnectionString("CustomerDomain");

            if (string.IsNullOrEmpty(connectionString))
                throw new InvalidOperationException("Connection string 'CustomerDomain' not found.");

            opts.Connection(connectionString);

            opts.Projections.Add(new CustomerProjection(services), JasperFx.Events.Projections.ProjectionLifecycle.Inline);

            opts.AutoCreateSchemaObjects = JasperFx.AutoCreate.All;
        })
        .ApplyAllDatabaseChangesOnStartup();

        return services;
    }
}
