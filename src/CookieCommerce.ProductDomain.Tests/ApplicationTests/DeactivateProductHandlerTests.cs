using CookieCommerce.DomainBuildingBlocks;
using CookieCommerce.ProductDomain.Application.Products.Commands;
using CookieCommerce.ProductDomain.Application.Products.Handlers;
using CookieCommerce.ProductDomain.Application.Products.Integration;
using CookieCommerce.ProductDomain.Domain.Aggregates;
using CookieCommerce.ProductDomain.Domain.Exceptions;
using FluentAssertions;
using Marten;
using NSubstitute;

namespace CookieCommerce.ProductDomain.Tests.ApplicationTests;

public class DeactivateProductHandlerTests
{
    [Fact]
    public async Task Handle_ShouldDeactivateProduct_WhenProductExists()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var session = Substitute.For<IDocumentSession>();
        var publisher = Substitute.For<IIntegrationEventPublisher>();

        var product = new ProductAggregate();
        product.Create(productId, "Test Product", "Description", 99.99m);
        session.Events.AggregateStreamAsync<ProductAggregate>(productId, token: Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(product));

        var handler = new DeactivateProductHandler(session, publisher);
        var command = new DeactivateProduct(productId);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        session.Events.Received(1).Append(productId, Arg.Any<IEnumerable<object>>());
        await publisher.Received(1).PublishAsync(
            Arg.Is<ProductDeactivatedIntegrationEvent>(e => e.ProductId == productId),
            Arg.Any<CancellationToken>());
        await session.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenProductNotFound()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var session = Substitute.For<IDocumentSession>();
        var publisher = Substitute.For<IIntegrationEventPublisher>();

        session.Events.AggregateStreamAsync<ProductAggregate>(productId, token: Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ProductAggregate>(null!));

        var handler = new DeactivateProductHandler(session, publisher);
        var command = new DeactivateProduct(productId);

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        var exception = await act.Should().ThrowAsync<ProductNotFoundException>();
        exception.Which.ProductId.Should().Be(productId);
    }

    [Fact]
    public async Task Handle_ShouldBeIdempotent_WhenCalledMultipleTimes()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var session = Substitute.For<IDocumentSession>();
        var publisher = Substitute.For<IIntegrationEventPublisher>();

        var product = new ProductAggregate();
        product.Create(productId, "Test Product", "Description", 99.99m);
        session.Events.AggregateStreamAsync<ProductAggregate>(productId, token: Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(product));

        var handler = new DeactivateProductHandler(session, publisher);
        var command = new DeactivateProduct(productId);

        // Act - Call deactivate first time
        await handler.Handle(command, CancellationToken.None);
        
        // Clear domain events after first call (simulating what happens after persist)
        product.ClearUncommitedEvents();
        
        // Reset call tracking for second call
        session.Events.ClearReceivedCalls();
        session.ClearReceivedCalls();
        publisher.ClearReceivedCalls();
        
        // Act - Product is already deactivated, but should not throw
        await handler.Handle(command, CancellationToken.None);

        // Assert - Should not append events, publish, or save when already deactivated
        session.Events.Received(0).Append(productId, Arg.Any<IEnumerable<object>>());
        await publisher.Received(0).PublishAsync(Arg.Any<object>(), Arg.Any<CancellationToken>());
        await session.Received(0).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
