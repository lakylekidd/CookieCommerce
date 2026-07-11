using Marten;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Testcontainers.PostgreSql;

namespace CookieCommerce.ProductDomain.Tests;

public class TestWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder("postgres:17-alpine")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:ProductDomain", _dbContainer.GetConnectionString());

        builder.UseSetting("ConnectionStrings:Messaging", "unused-for-tests");

        builder.ConfigureTestServices(services =>
        {
            // We override the connection string to use the container's random port
            var connectionString = _dbContainer.GetConnectionString();

            // Re-configure Marten to use the container
            services.AddMarten(opts =>
            {
                opts.Connection(connectionString);

                // Register projections with inline lifecycle
                opts.Projections.Add<Infrastructure.Projections.ProductDetailsProjection>(
                    JasperFx.Events.Projections.ProjectionLifecycle.Inline);
                opts.Projections.Add<Infrastructure.Projections.ProductListItemProjection>(
                    JasperFx.Events.Projections.ProjectionLifecycle.Inline);

                opts.AutoCreateSchemaObjects = JasperFx.AutoCreate.All;
            }).ApplyAllDatabaseChangesOnStartup();

            services.AddMassTransitTestHarness(x =>
            {
                x.UsingInMemory((context, cfg) =>
                {
                    cfg.ConfigureEndpoints(context);
                });
            });
        });
    }

    public async Task InitializeAsync() => await _dbContainer.StartAsync();

    public new async Task DisposeAsync() => await _dbContainer.StopAsync();

    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<IDocumentStore>();
        await store.Advanced.Clean.DeleteAllDocumentsAsync();
        await store.Advanced.Clean.DeleteAllEventDataAsync();

        await using var connection = new NpgsqlConnection(_dbContainer.GetConnectionString());
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            DO $$
            BEGIN
                IF to_regclass('public.mt_events') IS NOT NULL THEN
                    TRUNCATE TABLE public.mt_events RESTART IDENTITY CASCADE;
                END IF;

                IF to_regclass('public.mt_streams') IS NOT NULL THEN
                    TRUNCATE TABLE public.mt_streams RESTART IDENTITY CASCADE;
                END IF;

                IF EXISTS (SELECT 1 FROM information_schema.sequences WHERE sequence_schema = 'public' AND sequence_name = 'mt_events_sequence') THEN
                    ALTER SEQUENCE public.mt_events_sequence RESTART WITH 1;
                END IF;
            END $$;
            """;
        await command.ExecuteNonQueryAsync();
    }
}
