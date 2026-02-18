using CookieCommerce.OrderDomain.Application.Customers;
using CookieCommerce.OrderDomain.Infrastructure.Customers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CookieCommerce.OrderDomain.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Register HttpClient for Customer API
        services.AddHttpClient<ICustomerService, CustomerService>(client =>
        {
            client.BaseAddress = new Uri("http://customerdomainapi");
        });

        return services;
    }
}
