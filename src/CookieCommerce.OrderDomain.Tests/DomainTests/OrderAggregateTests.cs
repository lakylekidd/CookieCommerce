using CookieCommerce.OrderDomain.Domain.Aggregates;
using CookieCommerce.OrderDomain.Domain.DomainEvents;
using CookieCommerce.OrderDomain.Domain.Exceptions;
using CookieCommerce.OrderDomain.Domain.ValueObjects;
using FluentAssertions;

namespace CookieCommerce.OrderDomain.Tests.DomainTests;

public class OrderAggregateTests
{
    [Fact]
    public void PlaceOrder_ShouldEmitOrderPlacedEvent()
    {
        // Arrange
        var order = new OrderAggregate();
        var orderId = Guid.CreateVersion7();
        var customerId = Guid.CreateVersion7();

        // Act
        order.PlaceOrder(orderId, customerId);

        // Assert
        var events = order.DomainEvents;
        var placedEvent = Assert.Single(events) as OrderPlaced;

        placedEvent.Should().NotBeNull();
        placedEvent!.OrderId.Should().Be(orderId);
        placedEvent.CustomerId.Should().Be(customerId);
        placedEvent.PlacedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void PlaceOrder_WithCustomerSnapshot_ShouldEmitOrderPlacedEventWithSnapshot()
    {
        // Arrange
        var order = new OrderAggregate();
        var orderId = Guid.CreateVersion7();
        var customerId = Guid.CreateVersion7();
        var customerSnapshot = new CustomerSnapshot(customerId, "John Doe", "j***@example.com");

        // Act
        order.PlaceOrder(orderId, customerId, customerSnapshot);

        // Assert
        var events = order.DomainEvents;
        var placedEvent = Assert.Single(events) as OrderPlaced;

        placedEvent.Should().NotBeNull();
        placedEvent!.OrderId.Should().Be(orderId);
        placedEvent.CustomerId.Should().Be(customerId);
        placedEvent.CustomerSnapshot.Should().NotBeNull();
        placedEvent.CustomerSnapshot!.CustomerId.Should().Be(customerId);
        placedEvent.CustomerSnapshot.CustomerName.Should().Be("John Doe");
        placedEvent.CustomerSnapshot.MaskedEmail.Should().Be("j***@example.com");
    }

    [Fact]
    public void PlaceOrder_ShouldInitializeOrder_WithPendingStatus()
    {
        // Arrange
        var order = new OrderAggregate();
        var orderId = Guid.CreateVersion7();
        var customerId = Guid.CreateVersion7();

        // Act
        order.PlaceOrder(orderId, customerId);

        // Assert
        order.Id.Should().Be(orderId);
        order.CustomerId.Should().Be(customerId);
        order.Status.Should().Be(OrderStatus.Pending);
        order.OrderLines.Should().BeEmpty();
        order.Total.Should().Be(0);
    }

    [Fact]
    public void PlaceOrder_WithCustomerSnapshot_ShouldStoreSnapshot()
    {
        // Arrange
        var order = new OrderAggregate();
        var orderId = Guid.CreateVersion7();
        var customerId = Guid.CreateVersion7();
        var customerSnapshot = new CustomerSnapshot(customerId, "Jane Smith", "j***@example.com");

        // Act
        order.PlaceOrder(orderId, customerId, customerSnapshot);

        // Assert
        order.Id.Should().Be(orderId);
        order.CustomerId.Should().Be(customerId);
        order.CustomerSnapshot.Should().NotBeNull();
        order.CustomerSnapshot!.CustomerName.Should().Be("Jane Smith");
        order.CustomerSnapshot.MaskedEmail.Should().Be("j***@example.com");
    }

    [Fact]
    public void PlaceOrder_ShouldThrow_WhenCustomerIdIsEmpty()
    {
        // Arrange
        var order = new OrderAggregate();
        var orderId = Guid.CreateVersion7();

        // Act
        Action act = () => order.PlaceOrder(orderId, Guid.Empty);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Customer ID is required*");
    }

    [Fact]
    public void AddOrderLine_ShouldAddLine_WhenOrderIsPending()
    {
        // Arrange
        var order = new OrderAggregate();
        var orderId = Guid.CreateVersion7();
        var customerId = Guid.CreateVersion7();
        var productId = Guid.CreateVersion7();

        order.PlaceOrder(orderId, customerId);

        // Act
        order.AddOrderLine(productId, 2, 10.00m);

        // Assert
        order.OrderLines.Should().HaveCount(1);
        order.OrderLines.First().ProductId.Should().Be(productId);
        order.OrderLines.First().Quantity.Should().Be(2);
        order.OrderLines.First().UnitPrice.Should().Be(10.00m);
        order.SubTotal.Should().Be(20.00m);
        order.Total.Should().Be(22.00m); // 20 + 10% tax
    }

    [Fact]
    public void AddOrderLine_ShouldEmitOrderLineAddedEvent()
    {
        // Arrange
        var order = new OrderAggregate();
        var orderId = Guid.CreateVersion7();
        var customerId = Guid.CreateVersion7();
        var productId = Guid.CreateVersion7();

        order.PlaceOrder(orderId, customerId);

        // Act
        order.AddOrderLine(productId, 2, 10.00m);

        // Assert
        order.DomainEvents.Should().HaveCount(2); // OrderPlaced + OrderLineAdded
        var lineAddedEvent = order.DomainEvents.OfType<OrderLineAdded>().FirstOrDefault();
        lineAddedEvent.Should().NotBeNull();
        lineAddedEvent!.OrderId.Should().Be(orderId);
        lineAddedEvent.OrderLine.ProductId.Should().Be(productId);
    }

    [Fact]
    public void AddOrderLine_ShouldThrow_WhenOrderIsNotPending()
    {
        // Arrange
        var order = new OrderAggregate();
        var orderId = Guid.CreateVersion7();
        var customerId = Guid.CreateVersion7();
        var productId = Guid.CreateVersion7();

        order.PlaceOrder(orderId, customerId);
        order.AddOrderLine(productId, 1, 10.00m);
        order.Confirm();

        // Act
        Action act = () => order.AddOrderLine(Guid.CreateVersion7(), 1, 5.00m);

        // Assert
        act.Should().Throw<CannotModifyNonPendingOrderException>()
            .Which.OrderId.Should().Be(orderId);
    }

    [Fact]
    public void AddOrderLine_ShouldThrow_WhenProductAlreadyExists()
    {
        // Arrange
        var order = new OrderAggregate();
        var orderId = Guid.CreateVersion7();
        var customerId = Guid.CreateVersion7();
        var productId = Guid.CreateVersion7();

        order.PlaceOrder(orderId, customerId);
        order.AddOrderLine(productId, 1, 10.00m);

        // Act
        Action act = () => order.AddOrderLine(productId, 2, 10.00m);

        // Assert
        act.Should().Throw<DuplicateOrderLineException>()
            .Which.ProductId.Should().Be(productId);
    }

    [Fact]
    public void RemoveOrderLine_ShouldRemoveLine_WhenOrderIsPending()
    {
        // Arrange
        var order = new OrderAggregate();
        var orderId = Guid.CreateVersion7();
        var customerId = Guid.CreateVersion7();
        var productId = Guid.CreateVersion7();

        order.PlaceOrder(orderId, customerId);
        order.AddOrderLine(productId, 2, 10.00m);

        // Act
        order.RemoveOrderLine(productId);

        // Assert
        order.OrderLines.Should().BeEmpty();
        order.SubTotal.Should().Be(0);
        order.Total.Should().Be(0);
    }

    [Fact]
    public void RemoveOrderLine_ShouldThrow_WhenOrderIsNotPending()
    {
        // Arrange
        var order = new OrderAggregate();
        var orderId = Guid.CreateVersion7();
        var customerId = Guid.CreateVersion7();
        var productId = Guid.CreateVersion7();

        order.PlaceOrder(orderId, customerId);
        order.AddOrderLine(productId, 1, 10.00m);
        order.Confirm();

        // Act
        Action act = () => order.RemoveOrderLine(productId);

        // Assert
        act.Should().Throw<CannotModifyNonPendingOrderException>();
    }

    [Fact]
    public void RemoveOrderLine_ShouldThrow_WhenProductNotFound()
    {
        // Arrange
        var order = new OrderAggregate();
        var orderId = Guid.CreateVersion7();
        var customerId = Guid.CreateVersion7();
        var nonExistentProductId = Guid.CreateVersion7();

        order.PlaceOrder(orderId, customerId);

        // Act
        Action act = () => order.RemoveOrderLine(nonExistentProductId);

        // Assert
        act.Should().Throw<OrderLineNotFoundException>()
            .Which.ProductId.Should().Be(nonExistentProductId);
    }

    [Fact]
    public void Confirm_ShouldConfirmOrder_WhenOrderIsPendingWithLines()
    {
        // Arrange
        var order = new OrderAggregate();
        var orderId = Guid.CreateVersion7();
        var customerId = Guid.CreateVersion7();
        var productId = Guid.CreateVersion7();

        order.PlaceOrder(orderId, customerId);
        order.AddOrderLine(productId, 1, 10.00m);

        // Act
        order.Confirm();

        // Assert
        order.Status.Should().Be(OrderStatus.Confirmed);
        order.ConfirmedAtUtc.Should().NotBeNull();
        order.ConfirmedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Confirm_ShouldEmitOrderConfirmedEvent()
    {
        // Arrange
        var order = new OrderAggregate();
        var orderId = Guid.CreateVersion7();
        var customerId = Guid.CreateVersion7();
        var productId = Guid.CreateVersion7();

        order.PlaceOrder(orderId, customerId);
        order.AddOrderLine(productId, 1, 10.00m);

        // Act
        order.Confirm();

        // Assert
        var confirmedEvent = order.DomainEvents.OfType<OrderConfirmed>().FirstOrDefault();
        confirmedEvent.Should().NotBeNull();
        confirmedEvent!.OrderId.Should().Be(orderId);
    }

    [Fact]
    public void Confirm_ShouldThrow_WhenOrderIsCancelled()
    {
        // Arrange
        var order = new OrderAggregate();
        var orderId = Guid.CreateVersion7();
        var customerId = Guid.CreateVersion7();

        order.PlaceOrder(orderId, customerId);
        order.Cancel("Customer request");

        // Act
        Action act = () => order.Confirm();

        // Assert
        act.Should().Throw<CannotConfirmCancelledOrderException>()
            .Which.OrderId.Should().Be(orderId);
    }

    [Fact]
    public void Confirm_ShouldThrow_WhenOrderIsRejected()
    {
        // Arrange
        var order = new OrderAggregate();
        var orderId = Guid.CreateVersion7();
        var customerId = Guid.CreateVersion7();

        order.PlaceOrder(orderId, customerId);
        order.Reject("Fraud detected");

        // Act
        Action act = () => order.Confirm();

        // Assert
        act.Should().Throw<CannotConfirmRejectedOrderException>()
            .Which.OrderId.Should().Be(orderId);
    }

    [Fact]
    public void Confirm_ShouldThrow_WhenOrderHasNoLines()
    {
        // Arrange
        var order = new OrderAggregate();
        var orderId = Guid.CreateVersion7();
        var customerId = Guid.CreateVersion7();

        order.PlaceOrder(orderId, customerId);

        // Act
        Action act = () => order.Confirm();

        // Assert
        act.Should().Throw<EmptyOrderException>()
            .Which.OrderId.Should().Be(orderId);
    }

    [Fact]
    public void Cancel_ShouldCancelOrder_WithReason()
    {
        // Arrange
        var order = new OrderAggregate();
        var orderId = Guid.CreateVersion7();
        var customerId = Guid.CreateVersion7();

        order.PlaceOrder(orderId, customerId);

        // Act
        order.Cancel("Customer changed mind");

        // Assert
        order.Status.Should().Be(OrderStatus.Cancelled);
        order.CancelledAtUtc.Should().NotBeNull();
    }

    [Fact]
    public void Cancel_ShouldEmitOrderCancelledEvent()
    {
        // Arrange
        var order = new OrderAggregate();
        var orderId = Guid.CreateVersion7();
        var customerId = Guid.CreateVersion7();

        order.PlaceOrder(orderId, customerId);

        // Act
        order.Cancel("Customer request");

        // Assert
        var cancelledEvent = order.DomainEvents.OfType<OrderCancelled>().FirstOrDefault();
        cancelledEvent.Should().NotBeNull();
        cancelledEvent!.OrderId.Should().Be(orderId);
        cancelledEvent.Reason.Should().Be("Customer request");
    }

    [Fact]
    public void Cancel_ShouldThrow_WhenReasonIsEmpty()
    {
        // Arrange
        var order = new OrderAggregate();
        var orderId = Guid.CreateVersion7();
        var customerId = Guid.CreateVersion7();

        order.PlaceOrder(orderId, customerId);

        // Act
        Action act = () => order.Cancel("");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*reason*");
    }

    [Fact]
    public void MarkAsPaid_ShouldMarkOrderAsPaid_WhenOrderIsConfirmed()
    {
        // Arrange
        var order = new OrderAggregate();
        var orderId = Guid.CreateVersion7();
        var customerId = Guid.CreateVersion7();
        var productId = Guid.CreateVersion7();

        order.PlaceOrder(orderId, customerId);
        order.AddOrderLine(productId, 1, 10.00m);
        order.Confirm();

        // Act
        order.MarkAsPaid();

        // Assert
        order.Status.Should().Be(OrderStatus.Paid);
        order.PaidAtUtc.Should().NotBeNull();
    }

    [Fact]
    public void MarkAsPaid_ShouldEmitOrderPaidEvent()
    {
        // Arrange
        var order = new OrderAggregate();
        var orderId = Guid.CreateVersion7();
        var customerId = Guid.CreateVersion7();
        var productId = Guid.CreateVersion7();

        order.PlaceOrder(orderId, customerId);
        order.AddOrderLine(productId, 1, 10.00m);
        order.Confirm();

        // Act
        order.MarkAsPaid();

        // Assert
        var paidEvent = order.DomainEvents.OfType<OrderPaid>().FirstOrDefault();
        paidEvent.Should().NotBeNull();
        paidEvent!.OrderId.Should().Be(orderId);
    }

    [Fact]
    public void MarkAsPaid_ShouldThrow_WhenOrderIsNotConfirmed()
    {
        // Arrange
        var order = new OrderAggregate();
        var orderId = Guid.CreateVersion7();
        var customerId = Guid.CreateVersion7();

        order.PlaceOrder(orderId, customerId);

        // Act
        Action act = () => order.MarkAsPaid();

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Reject_ShouldRejectOrder_WithReason()
    {
        // Arrange
        var order = new OrderAggregate();
        var orderId = Guid.CreateVersion7();
        var customerId = Guid.CreateVersion7();

        order.PlaceOrder(orderId, customerId);

        // Act
        order.Reject("Payment method declined");

        // Assert
        order.Status.Should().Be(OrderStatus.Rejected);
        order.RejectedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public void Reject_ShouldEmitOrderRejectedEvent()
    {
        // Arrange
        var order = new OrderAggregate();
        var orderId = Guid.CreateVersion7();
        var customerId = Guid.CreateVersion7();

        order.PlaceOrder(orderId, customerId);

        // Act
        order.Reject("Fraud detected");

        // Assert
        var rejectedEvent = order.DomainEvents.OfType<OrderRejected>().FirstOrDefault();
        rejectedEvent.Should().NotBeNull();
        rejectedEvent!.OrderId.Should().Be(orderId);
        rejectedEvent.Reason.Should().Be("Fraud detected");
    }

    [Fact]
    public void Reject_ShouldThrow_WhenReasonIsEmpty()
    {
        // Arrange
        var order = new OrderAggregate();
        var orderId = Guid.CreateVersion7();
        var customerId = Guid.CreateVersion7();

        order.PlaceOrder(orderId, customerId);

        // Act
        Action act = () => order.Reject("");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*reason*");
    }

    [Fact]
    public void EventReplay_ShouldRebuildAggregateState()
    {
        // Arrange: Create aggregate and perform operations
        var originalOrder = new OrderAggregate();
        var orderId = Guid.CreateVersion7();
        var customerId = Guid.CreateVersion7();
        var productId1 = Guid.CreateVersion7();
        var productId2 = Guid.CreateVersion7();

        originalOrder.PlaceOrder(orderId, customerId);
        originalOrder.AddOrderLine(productId1, 2, 10.00m);
        originalOrder.AddOrderLine(productId2, 1, 15.00m);
        originalOrder.Confirm();
        originalOrder.MarkAsPaid();

        var events = originalOrder.DomainEvents.ToList();

        // Act: Replay events on a new aggregate
        var replayedOrder = new OrderAggregate();
        foreach (var evt in events)
        {
            var whenMethod = typeof(OrderAggregate).GetMethod("When",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            whenMethod?.Invoke(replayedOrder, new[] { evt });
        }

        // Assert: State should match original
        replayedOrder.Id.Should().Be(originalOrder.Id);
        replayedOrder.CustomerId.Should().Be(originalOrder.CustomerId);
        replayedOrder.Status.Should().Be(originalOrder.Status);
        replayedOrder.OrderLines.Should().HaveCount(originalOrder.OrderLines.Count);
        replayedOrder.SubTotal.Should().Be(originalOrder.SubTotal);
        replayedOrder.Total.Should().Be(originalOrder.Total);
    }

    [Fact]
    public void DomainEvents_ShouldBeTracked()
    {
        // Arrange
        var order = new OrderAggregate();
        var orderId = Guid.CreateVersion7();
        var customerId = Guid.CreateVersion7();
        var productId = Guid.CreateVersion7();

        // Act
        order.PlaceOrder(orderId, customerId);
        order.AddOrderLine(productId, 1, 10.00m);
        order.Confirm();

        // Assert
        order.DomainEvents.Should().HaveCount(3); // OrderPlaced, OrderLineAdded, OrderConfirmed
    }

    [Fact]
    public void MultipleOrderLines_ShouldCalculateTotalsCorrectly()
    {
        // Arrange
        var order = new OrderAggregate();
        var orderId = Guid.CreateVersion7();
        var customerId = Guid.CreateVersion7();

        order.PlaceOrder(orderId, customerId);

        // Act
        order.AddOrderLine(Guid.CreateVersion7(), 2, 10.00m);  // 20.00
        order.AddOrderLine(Guid.CreateVersion7(), 3, 5.00m);   // 15.00
        order.AddOrderLine(Guid.CreateVersion7(), 1, 25.00m);  // 25.00
        // SubTotal = 60.00, Tax = 6.00, Total = 66.00

        // Assert
        order.SubTotal.Should().Be(60.00m);
        order.TaxAmount.Should().Be(6.00m);
        order.Total.Should().Be(66.00m);
    }
}
