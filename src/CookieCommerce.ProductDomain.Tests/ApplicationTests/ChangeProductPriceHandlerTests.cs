using CookieCommerce.ProductDomain.Application.Products.Commands;
using CookieCommerce.ProductDomain.Application.Products.Handlers;
using CookieCommerce.ProductDomain.Domain.Aggregates;
using CookieCommerce.ProductDomain.Domain.Exceptions;
using FluentAssertions;
using Marten;
using NSubstitute;

namespace CookieCommerce.ProductDomain.Tests.ApplicationTests;

public class ChangeProductPriceHandlerTests
{
    [Fact]
    public async Task Handle_ShouldChangePrice_WhenProductExistsAndIsActive()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var session = Substitute.For<IDocumentSession>();

        var product = new ProductAggregate();
        product.Create(productId, "Test Product", "Description", 99.99m);
        session.Events.AggregateStreamAsync<ProductAggregate>(productId, token: Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(product));

        var handler = new ChangeProductPriceHandler(session);
        var command = new ChangeProductPrice(productId, 149.99m);

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

        session.Events.AggregateStreamAsync<ProductAggregate>(productId, token: Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ProductAggregate>(null!));

        var handler = new ChangeProductPriceHandler(session);
        var command = new ChangeProductPrice(productId, 149.99m);

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        var exception = await act.Should().ThrowAsync<ProductNotFoundException>();
        exception.Which.ProductId.Should().Be(productId);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenNewPriceIsZero()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var session = Substitute.For<IDocumentSession>();

        var product = new ProductAggregate();
        product.Create(productId, "Test Product", "Description", 99.99m);
        session.Events.AggregateStreamAsync<ProductAggregate>(productId, token: Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(product));

        var handler = new ChangeProductPriceHandler(session);
        var command = new ChangeProductPrice(productId, 0m);

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .Where(e => e.GetType().Name.Contains("InvalidPrice"));
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenNewPriceIsNegative()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var session = Substitute.For<IDocumentSession>();

        var product = new ProductAggregate();
        product.Create(productId, "Test Product", "Description", 99.99m);
        session.Events.AggregateStreamAsync<ProductAggregate>(productId, token: Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(product));

        var handler = new ChangeProductPriceHandler(session);
        var command = new ChangeProductPrice(productId, -10.50m);

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .Where(e => e.GetType().Name.Contains("InvalidPrice"));
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenProductIsDeactivated()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var session = Substitute.For<IDocumentSession>();

        var product = new ProductAggregate();
        product.Create(productId, "Test Product", "Description", 99.99m);
        product.Deactivate();
        session.Events.AggregateStreamAsync<ProductAggregate>(productId, token: Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(product));

        var handler = new ChangeProductPriceHandler(session);
        var command = new ChangeProductPrice(productId, 149.99m);

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .Where(e => e.GetType().Name.Contains("CannotChangePrice"));
    }
}
