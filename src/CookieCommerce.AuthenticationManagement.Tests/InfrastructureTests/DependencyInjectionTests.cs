using CookieCommerce.AuthenticationManagement.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CookieCommerce.AuthenticationManagement.Tests.InfrastructureTests;

public sealed class DependencyInjectionTests
{
    [Theory]
    [InlineData(true, "length(12) and upperCase(1)", "length(3)")]
    [InlineData(false, "length(3)", "length(8)")]
    [InlineData(false, "length(12) and upperCase(1)", "length(12) and upperCase(1)")]
    [InlineData(false, "upperCase(1)", "upperCase(1) and length(8)")]
    public void Mock_password_policy_is_development_only(bool development, string existing, string expected)
    {
        CookieCommerce.AuthenticationManagement.Infrastructure.Keycloak.KeycloakPasswordPolicy.Resolve(development, existing).Should().Be(expected);
    }

    [Fact]
    public void Aspire_Keycloak_endpoint_overrides_the_standalone_default()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["services:keycloak:http:0"] = "http://localhost:18080"
        }).Build();
        services.AddLogging(); services.AddInfrastructure(configuration);
        using var provider = services.BuildServiceProvider();
        provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<CookieCommerce.AuthenticationManagement.Infrastructure.Keycloak.KeycloakOptions>>()
            .Value.BaseUrl.Should().Be("http://localhost:18080");
    }

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

    [Fact]
    public void Production_admin_configuration_is_bound()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ProductionAdmin:Enabled"] = "true",
            ["ProductionAdmin:Email"] = "owner@example.com",
            ["ProductionAdmin:InitialPassword"] = "initial-password"
        }).Build();

        services.AddLogging();
        services.AddInfrastructure(configuration);
        using var provider = services.BuildServiceProvider();

        var options = provider.GetRequiredService<IOptions<CookieCommerce.AuthenticationManagement.Infrastructure.Keycloak.ProductionAdminOptions>>().Value;
        options.Enabled.Should().BeTrue();
        options.Email.Should().Be("owner@example.com");
        options.InitialPassword.Should().Be("initial-password");
    }
}
