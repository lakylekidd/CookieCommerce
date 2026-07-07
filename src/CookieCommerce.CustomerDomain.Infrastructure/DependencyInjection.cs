using CookieCommerce.CustomerDomain.Application.Customers.Integration;
using CookieCommerce.CustomerDomain.Application.Customers.Profile;
using CookieCommerce.CustomerDomain.Application.Security;
using CookieCommerce.CustomerDomain.Infrastructure.Consumers;
using CookieCommerce.CustomerDomain.Infrastructure.Integration;
using CookieCommerce.CustomerDomain.Infrastructure.Persistence;
using CookieCommerce.CustomerDomain.Infrastructure.Security;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CookieCommerce.CustomerDomain.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ICustomerKeyStore, CustomerKeyStore>();
        services.AddScoped<IEncryptionService, EncryptionService>();
        services.AddScoped<CustomerProfileEncryption>();
        services.AddScoped<CustomerEmailUniquenessService>();
        services.AddScoped<ICustomerEmailUniquenessChecker, MartenCustomerEmailUniquenessChecker>();

        // Email verification delivery settings (in Application layer for handler access)
        services.AddOptions<EmailVerificationDeliverySettings>()
            .Bind(configuration.GetSection(EmailVerificationDeliverySettings.SectionName));

        // US1: Concrete token store and external delivery sender
        services.AddScoped<IEmailVerificationTokenStore, EmailVerificationTokenStore>();
        services.AddScoped<IEmailVerificationDeliverySender, ExternalEmailVerificationSender>();

        // Named HTTP client for external email delivery dependency
        services.AddHttpClient("email-delivery");

        return services;
    }

    /// <summary>
    /// Registers all CustomerDomain MassTransit consumers with the bus configurator.
    /// Call this from <c>AddCookieMassTransit</c>'s <c>configureConsumers</c> parameter
    /// in the host's Program.cs so the correct exchange bindings are established.
    /// </summary>
    public static void AddCustomerDomainConsumers(this IBusRegistrationConfigurator configurator)
    {
        configurator.AddConsumer<UserRegisteredConsumer>();
    }
}
