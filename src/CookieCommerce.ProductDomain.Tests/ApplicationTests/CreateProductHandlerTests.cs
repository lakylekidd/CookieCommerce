using CookieCommerce.ProductDomain.Application.Products.Commands;
using CookieCommerce.ProductDomain.Application.Products.Handlers;
using CookieCommerce.ProductDomain.Domain.Aggregates;
using CookieCommerce.ProductDomain.Domain.Exceptions;
using FluentAssertions;
using Marten;
using NSubstitute;

namespace CookieCommerce.ProductDomain.Tests.ApplicationTests;

public class CreateProductHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreateProduct_WhenValidDataProvided()
    {
        // Arrange
        var session = Substitute.For<IDocumentSession>();
        var handler = new CreateProductHandler(session);
        var command = new CreateProduct("Test Product", "Test Description", 99.99m);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        session.Events.Received(1).StartStream<ProductAggregate>(result, Arg.Any<IEnumerable<object>>());
        await session.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldThrowArgumentException_WhenNameIsEmpty()
    {
        // Arrange
        var session = Substitute.For<IDocumentSession>();
        var handler = new CreateProductHandler(session);
        var command = new CreateProduct("", "Test Description", 99.99m);

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*name*");
    }

    [Fact]
    public async Task Handle_ShouldThrowArgumentException_WhenDescriptionIsEmpty()
    {
        // Arrange
        var session = Substitute.For<IDocumentSession>();
        var handler = new CreateProductHandler(session);
        var command = new CreateProduct("Test Product", "", 99.99m);

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*description*");
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidPriceException_WhenPriceIsZero()
    {
        // Arrange
        var session = Substitute.For<IDocumentSession>();
        var handler = new CreateProductHandler(session);
        var command = new CreateProduct("Test Product", "Test Description", 0m);

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidPriceException>();
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidPriceException_WhenPriceIsNegative()
    {
        // Arrange
        var session = Substitute.For<IDocumentSession>();
        var handler = new CreateProductHandler(session);
        var command = new CreateProduct("Test Product", "Test Description", -10.50m);

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidPriceException>();
    }
}
