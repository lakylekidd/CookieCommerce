using JasperFx;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CookieCommerce.CustomerDomain.Tests;

internal class TestHostFactory
{
    public static async Task<IHost> BuildAsync()
    {
        var builder = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddMarten(options =>
                {
                    options.Connection("host=localhost;database=not-used;mode=memory");
                    options.AutoCreateSchemaObjects = AutoCreate.All;
                });

                // Register sessions for tests
                services.AddScoped<IQuerySession>(sp => sp.GetRequiredService<IDocumentStore>().QuerySession());
                services.AddScoped<IDocumentSession>(sp => sp.GetRequiredService<IDocumentStore>().LightweightSession());

            });

        var host = builder.Build();

        var store = host.Services.GetRequiredService<IDocumentStore>(); 
        await store.Advanced.Clean.CompletelyRemoveAllAsync();

        return host;
    }
}
