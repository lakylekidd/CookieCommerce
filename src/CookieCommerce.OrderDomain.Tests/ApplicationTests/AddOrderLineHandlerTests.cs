using CookieCommerce.OrderDomain.Application.Orders.Commands;
using CookieCommerce.OrderDomain.Application.Orders.Handlers;
using CookieCommerce.OrderDomain.Domain.Aggregates;
using CookieCommerce.OrderDomain.Domain.Exceptions;
using CookieCommerce.OrderDomain.Domain.ValueObjects;
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

        _session.Events.AggregateStreamAsync<OrderAggregate>(orderId, token: Arg.Any<CancellationToken>())
            .Returns(order);

        var command = new AddOrderLine(orderId, productId, 2, 10.50m);

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
        
        _session.Events.AggregateStreamAsync<OrderAggregate>(orderId, token: Arg.Any<CancellationToken>())
            .Returns((OrderAggregate?)null);

        var command = new AddOrderLine(orderId, productId, 2, 10.50m);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<OrderNotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None));
        
        exception.OrderId.Should().Be(orderId);
    }
}
