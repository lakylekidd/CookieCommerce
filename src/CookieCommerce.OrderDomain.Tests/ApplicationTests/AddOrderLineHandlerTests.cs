using CookieCommerce.OrderDomain.Application.Orders.Commands;
using CookieCommerce.OrderDomain.Application.Orders.Handlers;
using CookieCommerce.OrderDomain.Application.Products.ReadModels;
using CookieCommerce.OrderDomain.Domain.Aggregates;
using CookieCommerce.OrderDomain.Domain.Exceptions;
using FluentAssertions;
using Marten;
using NSubstitute;

namespace CookieCommerce.OrderDomain.Tests.ApplicationTests;

public class AddOrderLineHandlerTests
{
    private readonly IDocumentSession _session;
    private readonly AddOrderLineHandler _handler;

    public AddOrderLineHandlerTests()
    {
        _session = Substitute.For<IDocumentSession>();
        _handler = new AddOrderLineHandler(_session);
    }

    [Fact]
    public async Task Handle_ValidCommand_AddsOrderLine()
    {
        // Arrange
        var orderId = Guid.CreateVersion7();
        var productId = Guid.CreateVersion7();
        var customerId = Guid.CreateVersion7();
        
        var order = new OrderAggregate();
        order.PlaceOrder(orderId, customerId);

        var productSnapshot = new ProductSnapshotReadModel
        {
            Id = productId,
            Name = "Test Product",
            Price = 10.50m,
            IsActive = true
        };

        _session.Events.AggregateStreamAsync<OrderAggregate>(orderId, token: Arg.Any<CancellationToken>())
            .Returns(order);
        
        _session.LoadAsync<ProductSnapshotReadModel>(productId, Arg.Any<CancellationToken>())
            .Returns(productSnapshot);

        var command = new AddOrderLine(orderId, productId, 2);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        order.OrderLines.Should().HaveCount(1);
        order.OrderLines.First().ProductId.Should().Be(productId);
        order.OrderLines.First().Quantity.Should().Be(2);
        order.OrderLines.First().UnitPrice.Should().Be(10.50m);
        
        await _session.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_OrderNotFound_ThrowsOrderNotFoundException()
    {
        // Arrange
        var orderId = Guid.CreateVersion7();
        var productId = Guid.CreateVersion7();
        
        var productSnapshot = new ProductSnapshotReadModel
        {
            Id = productId,
            Name = "Test Product",
            Price = 10.50m,
            IsActive = true
        };
        
        _session.Events.AggregateStreamAsync<OrderAggregate>(orderId, token: Arg.Any<CancellationToken>())
            .Returns((OrderAggregate?)null);
        
        _session.LoadAsync<ProductSnapshotReadModel>(productId, Arg.Any<CancellationToken>())
            .Returns(productSnapshot);

        var command = new AddOrderLine(orderId, productId, 2);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<OrderNotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None));
        
        exception.OrderId.Should().Be(orderId);
    }

    [Fact]
    public async Task Handle_ProductNotFound_ThrowsProductNotFoundException()
    {
        // Arrange
        var orderId = Guid.CreateVersion7();
        var productId = Guid.CreateVersion7();
        var customerId = Guid.CreateVersion7();
        
        var order = new OrderAggregate();
        order.PlaceOrder(orderId, customerId);

        _session.Events.AggregateStreamAsync<OrderAggregate>(orderId, token: Arg.Any<CancellationToken>())
            .Returns(order);
        
        _session.LoadAsync<ProductSnapshotReadModel>(productId, Arg.Any<CancellationToken>())
            .Returns((ProductSnapshotReadModel?)null);

        var command = new AddOrderLine(orderId, productId, 2);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ProductNotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None));
        
        exception.ProductId.Should().Be(productId);
    }

    [Fact]
    public async Task Handle_InactiveProduct_ThrowsProductNotFoundException()
    {
        // Arrange
        var orderId = Guid.CreateVersion7();
        var productId = Guid.CreateVersion7();
        var customerId = Guid.CreateVersion7();
        
        var order = new OrderAggregate();
        order.PlaceOrder(orderId, customerId);

        var productSnapshot = new ProductSnapshotReadModel
        {
            Id = productId,
            Name = "Inactive Product",
            Price = 10.50m,
            IsActive = false
        };

        _session.Events.AggregateStreamAsync<OrderAggregate>(orderId, token: Arg.Any<CancellationToken>())
            .Returns(order);
        
        _session.LoadAsync<ProductSnapshotReadModel>(productId, Arg.Any<CancellationToken>())
            .Returns(productSnapshot);

        var command = new AddOrderLine(orderId, productId, 2);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ProductNotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None));
        
        exception.ProductId.Should().Be(productId);
    }
}
