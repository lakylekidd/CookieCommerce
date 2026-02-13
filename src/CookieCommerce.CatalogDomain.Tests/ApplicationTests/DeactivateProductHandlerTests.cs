using CookieCommerce.CatalogDomain.Application.Products.Commands;
using CookieCommerce.CatalogDomain.Application.Products.Handlers;
using CookieCommerce.CatalogDomain.Domain.Aggregates;
using FluentAssertions;
using Marten;
using NSubstitute;

namespace CookieCommerce.CatalogDomain.Tests.ApplicationTests;

public class DeactivateProductHandlerTests
{
    [Fact]
    public async Task Handle_ShouldDeactivateProduct_WhenProductExists()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var session = Substitute.For<IDocumentSession>();

        var catalog = new CatalogAggregate();
        catalog.Create(productId, "Test Product", "Description", 99.99m);
        session.Events.AggregateStreamAsync<CatalogAggregate>(productId, token: Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(catalog));

        var handler = new DeactivateProductHandler(session);
        var command = new DeactivateProduct(productId);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        session.Events.Received(1).Append(productId, Arg.Any<IEnumerable<object>>());
        await session.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenProductNotFound()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var session = Substitute.For<IDocumentSession>();

        session.Events.AggregateStreamAsync<CatalogAggregate>(productId, token: Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<CatalogAggregate>(null!));

        var handler = new DeactivateProductHandler(session);
        var command = new DeactivateProduct(productId);

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Product with ID {productId} not found.");
    }

    [Fact]
    public async Task Handle_ShouldBeIdempotent_WhenCalledMultipleTimes()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var session = Substitute.For<IDocumentSession>();

        var catalog = new CatalogAggregate();
        catalog.Create(productId, "Test Product", "Description", 99.99m);
        session.Events.AggregateStreamAsync<CatalogAggregate>(productId, token: Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(catalog));

        var handler = new DeactivateProductHandler(session);
        var command = new DeactivateProduct(productId);

        // Act - Call deactivate twice
        await handler.Handle(command, CancellationToken.None);
        
        // Reset call tracking for second call
        session.Events.ClearReceivedCalls();
        session.ClearReceivedCalls();
        
        // Act - Catalog is already deactivated, but should not throw
        await handler.Handle(command, CancellationToken.None);

        // Assert - Should still append events and save
        session.Events.Received(1).Append(productId, Arg.Any<IEnumerable<object>>());
        await session.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
