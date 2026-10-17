using CookieCommerce.DevBootstrap;
using CookieCommerce.ProductDomain.Application.Products.Media;
using CookieCommerce.ProductDomain.Infrastructure.Bootstrap;
using CookieCommerce.ProductDomain.Infrastructure.Media;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CookieCommerce.ProductDomain.Infrastructure.DependencyInjection;

public static class ProductMediaServiceCollectionExtensions
{
    public static IServiceCollection AddProductMedia(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ProductMediaOptions>(configuration.GetSection(ProductMediaOptions.SectionName));
        services.AddSingleton<IProductMediaStore, FileSystemProductMediaStore>();
        services.AddSingleton<IProductImageValidator, ProductImageValidator>();
        services.AddScoped<IDevelopmentProductBootstrapAdapter, ProductDomainDevelopmentBootstrapAdapter>();
        services.AddHostedService<DevelopmentBootstrapService>();

        return services;
    }
}
