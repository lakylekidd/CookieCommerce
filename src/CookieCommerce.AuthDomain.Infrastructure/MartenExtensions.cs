using CookieCommerce.AuthDomain.Infrastructure.Projections;
using Marten;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CookieCommerce.AuthDomain.Infrastructure;

public static class MartenExtensions
{
    public static IServiceCollection AddMartenConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMarten(opts =>
        {
            var connectionString = configuration.GetConnectionString("AuthDomain");

            if (string.IsNullOrEmpty(connectionString))
                throw new InvalidOperationException("Connection string 'AuthDomain' not found.");

            opts.Connection(connectionString);
            opts.Projections.Add<AuthUserProjection>(JasperFx.Events.Projections.ProjectionLifecycle.Inline);
            opts.AutoCreateSchemaObjects = JasperFx.AutoCreate.All;
        })
        .ApplyAllDatabaseChangesOnStartup();

        return services;
    }
}
