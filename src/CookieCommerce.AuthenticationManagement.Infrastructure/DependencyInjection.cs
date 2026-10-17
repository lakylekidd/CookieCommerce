using CookieCommerce.AuthenticationManagement.Application.Keycloak;
using CookieCommerce.AuthenticationManagement.Infrastructure.Consumers;
using CookieCommerce.AuthenticationManagement.Infrastructure.Keycloak;
using CookieCommerce.DevBootstrap;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CookieCommerce.AuthenticationManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<KeycloakOptions>(opts =>
        {
            configuration.GetSection("Keycloak").Bind(opts);
            // Aspire injects the Keycloak URL as a connection string named "keycloak"
            var keycloakUrl = configuration.GetConnectionString("keycloak")
                ?? configuration["services:keycloak:http:0"] ?? configuration["services:keycloak:https:0"];
            if (!string.IsNullOrEmpty(keycloakUrl))
                opts.BaseUrl = keycloakUrl;
        });
        services.Configure<ProductionAdminOptions>(configuration.GetSection("ProductionAdmin"));

        services.AddHttpClient("keycloak", (sp, client) =>
        {
            var opts = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<KeycloakOptions>>().Value;
            client.BaseAddress = new Uri(opts.BaseUrl);
        });

        services.AddScoped<IKeycloakAdminService, KeycloakAdminService>();
        services.AddScoped<IKeycloakTokenService, KeycloakTokenService>();
        services.AddScoped<IDevelopmentIdentityBootstrapAdapter, KeycloakDevelopmentIdentityBootstrapAdapter>();
        services.AddHostedService<KeycloakBootstrapService>();
        services.AddHostedService<ProductionAdminBootstrapService>();
        services.AddHostedService<DevelopmentBootstrapService>();
        services.AddHostedService<RegistrationDeliveryWorker>();
        services.AddHostedService<PasswordRecoveryDeliveryWorker>();

        return services;
    }

    public static void AddAuthenticationManagementConsumers(this IBusRegistrationConfigurator configurator)
    {
        configurator.AddConsumer<CustomerProfileUpdatedConsumer>();
        configurator.AddConsumer<VerifiedCustomerEmailChangedConsumer>();
        configurator.AddConsumer<CustomerForgottenConsumer>();
    }
}
