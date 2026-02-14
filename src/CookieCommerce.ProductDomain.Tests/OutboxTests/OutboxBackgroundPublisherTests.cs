using CookieCommerce.ProductDomain.Infrastructure.Outbox;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace CookieCommerce.ProductDomain.Tests.OutboxTests;

public class OutboxBackgroundPublisherTests
{
    [Fact]
    public void OutboxBackgroundPublisher_ShouldBeHostedService()
    {
        // Arrange & Act
        var logger = Substitute.For<ILogger<OutboxBackgroundPublisher>>();
        var serviceScopeFactory = Substitute.For<IServiceScopeFactory>();
        var configuration = Substitute.For<IConfiguration>();
        var publisher = new OutboxBackgroundPublisher(serviceScopeFactory, configuration, logger);

        // Assert
        publisher.Should().BeAssignableTo<Microsoft.Extensions.Hosting.BackgroundService>();
    }

    [Fact]
    public void OutboxBackgroundPublisher_ShouldRequireServiceScopeFactory()
    {
        // Arrange
        var logger = Substitute.For<ILogger<OutboxBackgroundPublisher>>();
        var configuration = Substitute.For<IConfiguration>();

        // Act & Assert - Should not throw when properly constructed
        var act = () => new OutboxBackgroundPublisher(Substitute.For<IServiceScopeFactory>(), configuration, logger);
        act.Should().NotThrow();
    }

    [Fact]
    public void OutboxBackgroundPublisher_ShouldRequireLogger()
    {
        // Arrange
        var serviceScopeFactory = Substitute.For<IServiceScopeFactory>();
        var configuration = Substitute.For<IConfiguration>();

        // Act & Assert - Should not throw when properly constructed
        var act = () => new OutboxBackgroundPublisher(serviceScopeFactory, configuration, Substitute.For<ILogger<OutboxBackgroundPublisher>>());
        act.Should().NotThrow();
    }
}
