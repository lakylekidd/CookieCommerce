using Marten;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Testcontainers.PostgreSql;

namespace CookieCommerce.CustomerDomain.Tests;

public class TestWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder("postgres:17-alpine")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:CustomerDomain", _dbContainer.GetConnectionString());

        builder.UseSetting("ConnectionStrings:Messaging", "unused-for-tests");

        builder.ConfigureTestServices(services =>
        {
            // We override the connection string to use the container's random port
            var connectionString = _dbContainer.GetConnectionString();

            // Re-configure Marten to use the container
            services.AddMarten(opts =>
            {
                opts.Connection(connectionString);
                
                // Register projections
                opts.Projections.Add(new Infrastructure.Projections.CustomerProjection(services), JasperFx.Events.Projections.ProjectionLifecycle.Inline);
                
                opts.AutoCreateSchemaObjects = JasperFx.AutoCreate.All;
            }).ApplyAllDatabaseChangesOnStartup();

            services.AddMassTransitTestHarness(x =>
            {
                // If your AddCookieMassTransit adds Consumers, 
                // you might need to re-register them here or ensure they are picked up.

                x.UsingInMemory((context, cfg) =>
                {
                    cfg.ConfigureEndpoints(context);
                });
            });
        });
    }

    public async Task InitializeAsync() => await _dbContainer.StartAsync();

    public new async Task DisposeAsync() => await _dbContainer.StopAsync();
}
