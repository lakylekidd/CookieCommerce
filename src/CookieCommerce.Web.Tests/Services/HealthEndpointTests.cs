using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CookieCommerce.Web.Tests.Services;

public sealed class HealthEndpointTests
{
    [Theory]
    [InlineData(false, HttpStatusCode.OK)]
    [InlineData(true, HttpStatusCode.ServiceUnavailable)]
    public async Task Production_health_returns_status_without_diagnostics(bool unhealthy, HttpStatusCode expectedStatus)
    {
        await using var app = CreateApp(Environments.Production, unhealthy);
        app.MapDefaultEndpoints();
        await app.StartAsync();

        using var client = app.GetTestClient();
        using var response = await client.GetAsync("/health");
        Assert.Equal(expectedStatus, response.StatusCode);
        Assert.Equal(string.Empty, await response.Content.ReadAsStringAsync());
        using var alive = await client.GetAsync("/alive");
        Assert.Equal(HttpStatusCode.NotFound, alive.StatusCode);
    }

    [Fact]
    public async Task Development_health_endpoints_keep_their_responses()
    {
        await using var app = CreateApp(Environments.Development, unhealthy: true);
        app.MapDefaultEndpoints();
        await app.StartAsync();

        using var client = app.GetTestClient();
        using var health = await client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.ServiceUnavailable, health.StatusCode);
        Assert.Equal("Unhealthy", await health.Content.ReadAsStringAsync());
        Assert.Equal("Healthy", await client.GetStringAsync("/alive"));
    }

    private static WebApplication CreateApp(string environment, bool unhealthy)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = environment });
        builder.WebHost.UseTestServer();
        builder.AddDefaultHealthChecks();
        builder.Services.AddHealthChecks().AddCheck("dependency", () => unhealthy
            ? HealthCheckResult.Unhealthy("Private diagnostic")
            : HealthCheckResult.Healthy());
        return builder.Build();
    }
}
