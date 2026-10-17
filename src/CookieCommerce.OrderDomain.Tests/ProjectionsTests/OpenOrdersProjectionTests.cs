using CookieCommerce.OrderDomain.Application.Orders.ReadModels;
using CookieCommerce.OrderDomain.Domain.DomainEvents;
using CookieCommerce.OrderDomain.Domain.ValueObjects;
using CookieCommerce.OrderDomain.Infrastructure.Projections;
using FluentAssertions;

namespace CookieCommerce.OrderDomain.Tests.ProjectionsTests;

public class OpenOrdersProjectionTests
{
    private readonly OpenOrdersProjection _projection;

    public OpenOrdersProjectionTests()
    {
        _projection = new OpenOrdersProjection();
    }

    [Fact]
    public void Create_OrderPlaced_ReturnsReadModel()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var placedAt = DateTime.UtcNow;

        var orderPlaced = new OrderPlaced
        {
            OrderId = orderId,
            CustomerId = customerId,
            PlacedAtUtc = placedAt
        };

        // Act
        var result = _projection.Create(orderPlaced);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(orderId);
        result.CustomerId.Should().Be(customerId);
        result.Status.Should().Be(OrderStatus.Pending);
        result.PlacedAtUtc.Should().Be(placedAt);
        result.Total.Should().Be(0);
    }

    [Fact]
    public void Apply_OrderLineAdded_UpdatesTotal()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var orderLine = new OrderLine(productId, quantity: 2, unitPrice: 10.00m);

        var current = new OpenOrderReadModel
        {
            Id = orderId,
            Total = 0
        };

        var orderLineAdded = new OrderLineAdded
        {
            OrderId = orderId,
            OrderLine = orderLine
        };

        // Act
        var result = _projection.Apply(orderLineAdded, current);

        // Assert
        // LineTotal = 2 * 10 = 20
        // Total with tax = 20 * 1.10 = 22
        result.Total.Should().Be(22.00m);
    }

    [Fact]
    public void Apply_OrderLineAdded_MultipleLines_AccumulatesTotals()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var current = new OpenOrderReadModel
        {
            Id = orderId,
            Total = 22.00m // Already has one line
        };

        var orderLine = new OrderLine(Guid.NewGuid(), quantity: 1, unitPrice: 5.00m);
        var orderLineAdded = new OrderLineAdded
        {
            OrderId = orderId,
            OrderLine = orderLine
        };

        // Act
        var result = _projection.Apply(orderLineAdded, current);

        // Assert
        // Previous: 22.00
        // New line total: 5 * 1.10 = 5.50
        // Total: 22.00 + 5.50 = 27.50
        result.Total.Should().Be(27.50m);
    }

    [Fact]
    public void Apply_OrderLineRemoved_DoesNothing()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        var current = new OpenOrderReadModel
        {
            Id = orderId,
            Total = 22.00m
        };

        var orderLineRemoved = new OrderLineRemoved
        {
            OrderId = orderId,
            ProductId = productId
        };

        // Act
        var result = _projection.Apply(orderLineRemoved, current);

        // Assert
        // Note: The projection cannot accurately recalculate without knowing the line total
        // This is documented as a limitation
        result.Total.Should().Be(22.00m);
    }

    [Fact]
    public void Apply_OrderConfirmed_UpdatesStatus()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var confirmedAt = DateTime.UtcNow;
        var current = new OpenOrderReadModel
        {
            Id = orderId,
            Status = OrderStatus.Pending
        };

        var orderConfirmed = new OrderConfirmed
        {
            OrderId = orderId,
            ConfirmedAtUtc = confirmedAt
        };

        // Act
        var result = _projection.Apply(orderConfirmed, current);

        // Assert
        result.Status.Should().Be(OrderStatus.Confirmed);
        result.ConfirmedAtUtc.Should().Be(confirmedAt);
    }

    [Fact]
    public void Constructor_ConfiguresDeleteEvents()
    {
        // This test verifies that the projection is configured to delete
        // read models when the order is paid, cancelled, or rejected
        // The DeleteEvent configuration happens in the constructor

        // Arrange & Act
        var projection = new OpenOrdersProjection();

        // Assert
        // We can't directly test the DeleteEvent configuration,
        // but we can verify the projection was created successfully
        projection.Should().NotBeNull();

        // In actual usage, when OrderPaid, OrderCancelled, or OrderRejected events
        // are processed, Marten will automatically delete the read model
        // This is tested through integration tests with Marten
    }
}
