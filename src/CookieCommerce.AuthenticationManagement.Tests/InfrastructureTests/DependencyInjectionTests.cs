using CookieCommerce.AuthenticationManagement.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CookieCommerce.AuthenticationManagement.Tests.InfrastructureTests;

public sealed class DependencyInjectionTests
{
    [Fact]
    public void AddInfrastructure_RegistersHostedServicesWithoutLifetimeValidationErrors()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Keycloak:BaseUrl"] = "http://localhost:8080",
            })
            .Build();

        services.AddLogging();
        services.AddInfrastructure(configuration);

        var act = () => services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true,
        });

        act.Should().NotThrow();
    }
}
