using CookieCommerce.ProductDomain.Infrastructure.Projections;
using Marten;

namespace CookieCommerce.ProductDomain.Api.DependencyInjection;

public static class MartenExtensions
{
    public static IServiceCollection AddMartenConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMarten(opts =>
        {
            var connectionString = configuration.GetConnectionString("ProductDomain");

            if (string.IsNullOrEmpty(connectionString))
                throw new InvalidOperationException("Connection string 'ProductDomain' not found.");

            opts.Connection(connectionString);

            // Register projections with inline lifecycle
            opts.Projections.Add<ProductDetailsProjection>(JasperFx.Events.Projections.ProjectionLifecycle.Inline);
            opts.Projections.Add<ProductListItemProjection>(JasperFx.Events.Projections.ProjectionLifecycle.Inline);

            opts.AutoCreateSchemaObjects = JasperFx.AutoCreate.All;
        })
        .ApplyAllDatabaseChangesOnStartup();

        return services;
    }
}
