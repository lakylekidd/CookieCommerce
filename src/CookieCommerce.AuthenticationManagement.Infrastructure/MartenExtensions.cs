using CookieCommerce.AuthenticationManagement.Infrastructure.Projections;
using Marten;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CookieCommerce.AuthenticationManagement.Infrastructure;

public static class MartenExtensions
{
    public static IServiceCollection AddMartenConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMarten(opts =>
        {
            var connectionString = configuration.GetConnectionString("AuthenticationManagement");

            if (string.IsNullOrEmpty(connectionString))
                throw new InvalidOperationException("Connection string 'AuthenticationManagement' not found.");

            opts.Connection(connectionString);
            opts.Projections.Add<AuthUserProjection>(JasperFx.Events.Projections.ProjectionLifecycle.Inline);
            opts.AutoCreateSchemaObjects = JasperFx.AutoCreate.All;
        })
        .ApplyAllDatabaseChangesOnStartup();

        return services;
    }
}
