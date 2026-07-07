using CookieCommerce.OrderDomain.Application.Orders.ReadModels;
using CookieCommerce.OrderDomain.Application.Orders.ValueObjects;
using CookieCommerce.OrderDomain.Domain.DomainEvents;
using CookieCommerce.OrderDomain.Domain.ValueObjects;
using CookieCommerce.OrderDomain.Infrastructure.Projections;
using FluentAssertions;

namespace CookieCommerce.OrderDomain.Tests.ProjectionsTests;

public class OrderDetailsProjectionTests
{
    private readonly OrderDetailsProjection _projection;

    public OrderDetailsProjectionTests()
    {
        _projection = new OrderDetailsProjection();
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
        result.OrderLines.Should().BeEmpty();
        result.SubTotal.Should().Be(0);
        result.TaxAmount.Should().Be(0);
        result.Total.Should().Be(0);
    }

    [Fact]
    public void Apply_OrderLineAdded_AddsLineAndRecalculatesTotals()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var orderLine = new OrderLine(productId, quantity: 2, unitPrice: 10.00m);

        var current = new OrderDetailsReadModel
        {
            Id = orderId,
            OrderLines = new List<OrderLineReadModel>()
        };

        var orderLineAdded = new OrderLineAdded
        {
            OrderId = orderId,
            OrderLine = orderLine
        };

        // Act
        var result = _projection.Apply(orderLineAdded, current);

        // Assert
        result.OrderLines.Should().HaveCount(1);
        result.OrderLines[0].ProductId.Should().Be(productId);
        result.OrderLines[0].Quantity.Should().Be(2);
        result.OrderLines[0].PriceInfo.Should().NotBeNull();
        result.OrderLines[0].PriceInfo.BasePrice.Should().Be(10.00m);
        result.OrderLines[0].LineTotal.Should().Be(20.00m);
        result.SubTotal.Should().Be(20.00m);
        result.TaxAmount.Should().Be(2.00m); // 10% tax
        result.Total.Should().Be(22.00m);
    }

    [Fact]
    public void Apply_OrderLineAdded_MultipleLines_CalculatesCorrectTotals()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var current = new OrderDetailsReadModel
        {
            Id = orderId,
            OrderLines = new List<OrderLineReadModel>
            {
                new() {
                    ProductId = Guid.NewGuid(),
                    Quantity = 1,
                    PriceInfo = new PriceInfo { BasePrice = 10.00m, TaxType = TaxType.Percentage, TaxValue = 0.10m, IsTaxIncluded = false },
                    LineTotal = 10.00m
                }
            },
            SubTotal = 10.00m,
            TaxAmount = 1.00m,
            Total = 11.00m
        };

        var orderLine = new OrderLine(Guid.NewGuid(), quantity: 3, unitPrice: 5.00m);
        var orderLineAdded = new OrderLineAdded
        {
            OrderId = orderId,
            OrderLine = orderLine
        };

        // Act
        var result = _projection.Apply(orderLineAdded, current);

        // Assert
        result.OrderLines.Should().HaveCount(2);
        result.SubTotal.Should().Be(25.00m); // 10 + 15
        result.TaxAmount.Should().Be(2.50m); // 10% of 25
        result.Total.Should().Be(27.50m);
    }

    [Fact]
    public void Apply_OrderLineRemoved_RemovesLineAndRecalculatesTotals()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var productId1 = Guid.NewGuid();
        var productId2 = Guid.NewGuid();

        var current = new OrderDetailsReadModel
        {
            Id = orderId,
            OrderLines = new List<OrderLineReadModel>
            {
                new() {
                    ProductId = productId1,
                    Quantity = 2,
                    PriceInfo = new PriceInfo { BasePrice = 10.00m, TaxType = TaxType.Percentage, TaxValue = 0.10m, IsTaxIncluded = false },
                    LineTotal = 20.00m
                },
                new() {
                    ProductId = productId2,
                    Quantity = 1,
                    PriceInfo = new PriceInfo { BasePrice = 15.00m, TaxType = TaxType.Percentage, TaxValue = 0.10m, IsTaxIncluded = false },
                    LineTotal = 15.00m
                }
            },
            SubTotal = 35.00m,
            TaxAmount = 3.50m,
            Total = 38.50m
        };

        var orderLineRemoved = new OrderLineRemoved
        {
            OrderId = orderId,
            ProductId = productId1
        };

        // Act
        var result = _projection.Apply(orderLineRemoved, current);

        // Assert
        result.OrderLines.Should().HaveCount(1);
        result.OrderLines[0].ProductId.Should().Be(productId2);
        result.SubTotal.Should().Be(15.00m);
        result.TaxAmount.Should().Be(1.50m);
        result.Total.Should().Be(16.50m);
    }

    [Fact]
    public void Apply_OrderConfirmed_UpdatesStatus()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var confirmedAt = DateTime.UtcNow;
        var current = new OrderDetailsReadModel
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
        result.StatusDate.Should().Be(confirmedAt);
        result.StatusHistory.Should().ContainKey(OrderStatus.Confirmed.ToString());
        result.StatusHistory[OrderStatus.Confirmed.ToString()].Should().Be(confirmedAt);
    }

    [Fact]
    public void Apply_OrderPaid_UpdatesStatus()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var paidAt = DateTime.UtcNow;
        var current = new OrderDetailsReadModel
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
        result.StatusDate.Should().Be(paidAt);
        result.StatusHistory.Should().ContainKey(OrderStatus.Paid.ToString());
        result.StatusHistory[OrderStatus.Paid.ToString()].Should().Be(paidAt);
    }

    [Fact]
    public void Apply_OrderCancelled_UpdatesStatus()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var cancelledAt = DateTime.UtcNow;
        var current = new OrderDetailsReadModel
        {
            Id = orderId,
            Status = OrderStatus.Pending
        };

        var orderCancelled = new OrderCancelled
        {
            OrderId = orderId,
            Reason = "Customer requested",
            CancelledAtUtc = cancelledAt
        };

        // Act
        var result = _projection.Apply(orderCancelled, current);

        // Assert
        result.Status.Should().Be(OrderStatus.Cancelled);
        result.StatusDate.Should().Be(cancelledAt);
        result.StatusHistory.Should().ContainKey(OrderStatus.Cancelled.ToString());
        result.StatusHistory[OrderStatus.Cancelled.ToString()].Should().Be(cancelledAt);
    }

    [Fact]
    public void Apply_OrderRejected_UpdatesStatus()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var rejectedAt = DateTime.UtcNow;
        var current = new OrderDetailsReadModel
        {
            Id = orderId,
            Status = OrderStatus.Pending
        };

        var orderRejected = new OrderRejected
        {
            OrderId = orderId,
            Reason = "Payment failed",
            RejectedAtUtc = rejectedAt
        };

        // Act
        var result = _projection.Apply(orderRejected, current);

        // Assert
        result.Status.Should().Be(OrderStatus.Rejected);
        result.StatusDate.Should().Be(rejectedAt);
        result.StatusHistory.Should().ContainKey(OrderStatus.Rejected.ToString());
        result.StatusHistory[OrderStatus.Rejected.ToString()].Should().Be(rejectedAt);
    }
}
