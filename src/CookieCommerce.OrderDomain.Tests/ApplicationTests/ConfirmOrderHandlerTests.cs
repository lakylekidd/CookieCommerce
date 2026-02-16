using CookieCommerce.OrderDomain.Application.Orders.Commands;
using CookieCommerce.OrderDomain.Application.Orders.Handlers;
using CookieCommerce.OrderDomain.Domain.Aggregates;
using CookieCommerce.OrderDomain.Domain.Exceptions;
using CookieCommerce.OrderDomain.Domain.ValueObjects;
using FluentAssertions;
using Marten;
using NSubstitute;

namespace CookieCommerce.OrderDomain.Tests.ApplicationTests;

public class ConfirmOrderHandlerTests
{
    private readonly IDocumentSession _session;
    private readonly ConfirmOrderHandler _handler;

    public ConfirmOrderHandlerTests()
    {
        _session = Substitute.For<IDocumentSession>();
        _handler = new ConfirmOrderHandler(_session);
    }

    [Fact]
    public async Task Handle_ValidCommand_ConfirmsOrder()
    {
        // Arrange
        var orderId = Guid.CreateVersion7();
        var customerId = Guid.CreateVersion7();
        var productId = Guid.CreateVersion7();
        
        var order = new OrderAggregate();
        order.PlaceOrder(orderId, customerId);
        order.AddOrderLine(productId, 1, 10m);

        _session.Events.AggregateStreamAsync<OrderAggregate>(orderId, token: Arg.Any<CancellationToken>())
            .Returns(order);

        var command = new ConfirmOrder(orderId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        order.Status.Should().Be(OrderStatus.Confirmed);
        order.ConfirmedAtUtc.Should().NotBeNull();
        
        await _session.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_OrderNotFound_ThrowsOrderNotFoundException()
    {
        // Arrange
        var orderId = Guid.CreateVersion7();
        
        _session.Events.AggregateStreamAsync<OrderAggregate>(orderId, token: Arg.Any<CancellationToken>())
            .Returns((OrderAggregate?)null);

        var command = new ConfirmOrder(orderId);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<OrderNotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None));
        
        exception.OrderId.Should().Be(orderId);
    }
}
