using CookieCommerce.CookieMassTransit;
using CookieCommerce.CustomerDomain.Application.Security;
using CookieCommerce.CustomerDomain.Infrastructure.Projections;
using CookieCommerce.CustomerDomain.Infrastructure.Security;
using Marten.Events.Projections;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CookieCommerce.CustomerDomain.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IProjection, CustomerProjection>();

        services.AddScoped<ICustomerKeyStore, CustomerKeyStore>();
        services.AddScoped<IEncryptionService, EncryptionService>();

        return services;
    }
}
