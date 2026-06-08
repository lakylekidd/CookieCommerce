using CookieCommerce.AuthDomain.Application.Keycloak;
using CookieCommerce.AuthDomain.Infrastructure.Keycloak;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CookieCommerce.AuthDomain.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<KeycloakOptions>(opts =>
        {
            configuration.GetSection("Keycloak").Bind(opts);
            // Aspire injects the Keycloak URL as a connection string named "keycloak"
            var keycloakUrl = configuration.GetConnectionString("keycloak");
            if (!string.IsNullOrEmpty(keycloakUrl))
                opts.BaseUrl = keycloakUrl;
        });

        services.AddHttpClient("keycloak", (sp, client) =>
        {
            var opts = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<KeycloakOptions>>().Value;
            client.BaseAddress = new Uri(opts.BaseUrl);
        });

        services.AddScoped<IKeycloakAdminService, KeycloakAdminService>();
        services.AddScoped<IKeycloakTokenService, KeycloakTokenService>();
        services.AddHostedService<KeycloakBootstrapService>();

        return services;
    }
}
