using CookieCommerce.OrderDomain.Application.Orders.ReadModels;
using CookieCommerce.OrderDomain.Domain.DomainEvents;
using CookieCommerce.OrderDomain.Domain.ValueObjects;
using CookieCommerce.OrderDomain.Infrastructure.Projections;
using FluentAssertions;

namespace CookieCommerce.OrderDomain.Tests.ProjectionsTests;

public class CustomerOrderHistoryProjectionTests
{
    private readonly CustomerOrderHistoryProjection _projection;

    public CustomerOrderHistoryProjectionTests()
    {
        _projection = new CustomerOrderHistoryProjection();
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

        var current = new CustomerOrderHistoryReadModel
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
        var current = new CustomerOrderHistoryReadModel
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
    public void Apply_OrderConfirmed_UpdatesStatus()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var confirmedAt = DateTime.UtcNow;
        var current = new CustomerOrderHistoryReadModel
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
    public void Apply_OrderPaid_UpdatesStatus()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var paidAt = DateTime.UtcNow;
        var current = new CustomerOrderHistoryReadModel
        {
            Id = orderId,
            Status = OrderStatus.Confirmed
        };

        var orderPaid = new OrderPaid
        {
            OrderId = orderId,
            PaidAtUtc = paidAt
        };

        // Act
        var result = _projection.Apply(orderPaid, current);

        // Assert
        result.Status.Should().Be(OrderStatus.Paid);
        result.PaidAtUtc.Should().Be(paidAt);
    }

    [Fact]
    public void Apply_OrderCancelled_UpdatesStatus()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var current = new CustomerOrderHistoryReadModel
        {
            Id = orderId,
            Status = OrderStatus.Pending
        };

        var orderCancelled = new OrderCancelled
        {
            OrderId = orderId,
            Reason = "Customer requested",
            CancelledAtUtc = DateTime.UtcNow
        };

        // Act
        var result = _projection.Apply(orderCancelled, current);

        // Assert
        result.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public void Apply_OrderRejected_UpdatesStatus()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var current = new CustomerOrderHistoryReadModel
        {
            Id = orderId,
            Status = OrderStatus.Pending
        };

        var orderRejected = new OrderRejected
        {
            OrderId = orderId,
            Reason = "Payment failed",
            RejectedAtUtc = DateTime.UtcNow
        };

        // Act
        var result = _projection.Apply(orderRejected, current);

        // Assert
        result.Status.Should().Be(OrderStatus.Rejected);
    }
}
