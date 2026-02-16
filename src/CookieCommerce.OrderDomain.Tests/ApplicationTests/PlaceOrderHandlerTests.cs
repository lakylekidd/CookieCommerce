using CookieCommerce.OrderDomain.Application.Orders.Commands;
using CookieCommerce.OrderDomain.Application.Orders.Handlers;
using CookieCommerce.OrderDomain.Domain.Aggregates;
using CookieCommerce.OrderDomain.Domain.Exceptions;
using FluentAssertions;
using Marten;
using NSubstitute;

namespace CookieCommerce.OrderDomain.Tests.ApplicationTests;

public class PlaceOrderHandlerTests
{
    private readonly IDocumentSession _session;
    private readonly PlaceOrderHandler _handler;

    public PlaceOrderHandlerTests()
    {
        _session = Substitute.For<IDocumentSession>();
        _handler = new PlaceOrderHandler(_session);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsNewOrderId()
    {
        // Arrange
        var customerId = Guid.CreateVersion7();
        var command = new PlaceOrder(customerId);

        // Act
        var orderId = await _handler.Handle(command, CancellationToken.None);

        // Assert
        orderId.Should().NotBeEmpty();
        await _session.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_EmptyCustomerId_ThrowsArgumentException()
    {
        // Arrange
        var command = new PlaceOrder(Guid.Empty);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }
}
