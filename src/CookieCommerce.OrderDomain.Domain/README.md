# Order Domain

## Overview

The Order domain handles order placement, management, and lifecycle within the CookieCommerce e-commerce system. It follows Domain-Driven Design (DDD) principles, Event Sourcing, and CQRS patterns using Marten as the event store.

## Architecture

### Aggregate Root

#### OrderAggregate
The `OrderAggregate` is the central entity that enforces all business rules and invariants related to orders.

**Key Properties:**
- `Id` (Guid) - Unique order identifier (Version 7 UUID)
- `CustomerId` (Guid) - Reference to the customer who placed the order
- `Status` (OrderStatus) - Current order status (Pending, Confirmed, Paid, Cancelled, Rejected)
- `OrderLines` (IReadOnlyCollection<OrderLine>) - Products and quantities in the order
- `SubTotal` (decimal) - Sum of all order line totals before tax
- `TaxAmount` (decimal) - Calculated tax (10% of subtotal)
- `Total` (decimal) - Final order total (subtotal + tax)
- `PlacedAtUtc` (DateTime) - Timestamp when order was placed
- `ConfirmedAtUtc` (DateTime?) - Timestamp when order was confirmed
- `PaidAtUtc` (DateTime?) - Timestamp when payment was received
- `CancelledAtUtc` (DateTime?) - Timestamp when order was cancelled
- `RejectedAtUtc` (DateTime?) - Timestamp when order was rejected

### Value Objects

#### OrderLine
Represents a single product line item in an order. This is an immutable value object.

**Properties:**
- `ProductId` (Guid) - Reference to the product
- `Quantity` (int) - Number of units ordered
- `UnitPrice` (decimal) - Price per unit at time of order
- `LineTotal` (decimal) - Calculated total (Quantity × UnitPrice)

**Invariants:**
- Quantity must be greater than 0
- Unit price must be greater than 0
- All properties are immutable (init-only)

#### OrderStatus (Enum)
Defines the possible states of an order:
- `Pending` (0) - Order placed but not yet confirmed
- `Confirmed` (1) - Order confirmed and ready for processing
- `Paid` (2) - Payment received
- `Cancelled` (3) - Order cancelled by customer or system
- `Rejected` (4) - Order rejected (e.g., fraud detection, payment declined)

## Domain Events

All state changes in the Order aggregate are captured as events:

### OrderPlaced
Emitted when a new order is created.
- `OrderId` (Guid)
- `CustomerId` (Guid)
- `PlacedAtUtc` (DateTime)

### OrderLineAdded
Emitted when a product is added to the order.
- `OrderId` (Guid)
- `OrderLine` (OrderLine)

### OrderLineRemoved
Emitted when a product is removed from the order.
- `OrderId` (Guid)
- `ProductId` (Guid)

### OrderConfirmed
Emitted when an order is confirmed.
- `OrderId` (Guid)
- `ConfirmedAtUtc` (DateTime)

### OrderCancelled
Emitted when an order is cancelled.
- `OrderId` (Guid)
- `Reason` (string)
- `CancelledAtUtc` (DateTime)

### OrderPaid
Emitted when payment is received for an order.
- `OrderId` (Guid)
- `PaidAtUtc` (DateTime)

### OrderRejected
Emitted when an order is rejected.
- `OrderId` (Guid)
- `Reason` (string)
- `RejectedAtUtc` (DateTime)

## Business Rules & Invariants

### Order Placement
- Customer ID must be valid (not empty)
- New orders start in `Pending` status

### Adding/Removing Order Lines
- Order lines can only be modified when order status is `Pending`
- Cannot add duplicate products (same ProductId) to an order
- Cannot remove a product that doesn't exist in the order
- Totals are automatically recalculated when lines are added/removed

### Order Confirmation
- Cannot confirm an order that has been cancelled
- Cannot confirm an order that has been rejected
- Cannot confirm an empty order (must have at least one order line)
- Order must be in `Pending` status
- Confirming an already confirmed order is idempotent

### Payment
- Can only mark an order as paid when it is in `Confirmed` status
- Marking an already paid order as paid is idempotent

### Cancellation
- Orders can be cancelled at any time
- Cancellation requires a reason
- Cancelling an already cancelled order is idempotent

### Rejection
- Orders can be rejected at any time (e.g., fraud detection, payment issues)
- Rejection requires a reason
- Rejecting an already rejected order is idempotent

## Domain Exceptions

The domain enforces business rules through custom exceptions:

- `OrderNotFoundException` - Order with specified ID not found
- `CannotConfirmCancelledOrderException` - Attempt to confirm a cancelled order
- `CannotConfirmRejectedOrderException` - Attempt to confirm a rejected order
- `CannotModifyNonPendingOrderException` - Attempt to add/remove lines from non-pending order
- `OrderLineNotFoundException` - Attempt to remove a non-existent order line
- `DuplicateOrderLineException` - Attempt to add a product that already exists in the order
- `EmptyOrderException` - Attempt to confirm an order with no order lines

## Usage Examples

### Placing a New Order
```csharp
var order = new OrderAggregate();
var orderId = Guid.CreateVersion7();
var customerId = Guid.CreateVersion7();

order.PlaceOrder(orderId, customerId);
```

### Adding Order Lines
```csharp
var productId = Guid.CreateVersion7();
order.AddOrderLine(productId, quantity: 2, unitPrice: 10.00m);
```

### Confirming an Order
```csharp
order.Confirm();
```

### Processing Payment
```csharp
order.MarkAsPaid();
```

### Cancelling an Order
```csharp
order.Cancel("Customer changed mind");
```

### Rejecting an Order
```csharp
order.Reject("Payment method declined");
```

## Event Sourcing

The `OrderAggregate` implements event sourcing through the `AggregateBase` class:

1. **Apply Pattern**: Business methods call `Apply(event)` to emit events
2. **When Pattern**: The `When(object @event)` method routes events to specific handlers
3. **Handle Methods**: Private `Handle()` methods apply state changes from events
4. **Event Replay**: Aggregates can be reconstructed from their event stream

### Example Event Flow
```csharp
// 1. Business method
order.Confirm();

// 2. Emits event
Apply(new OrderConfirmed { OrderId = Id, ConfirmedAtUtc = DateTime.UtcNow });

// 3. Routes to handler
protected override void When(object @event)
{
    case OrderConfirmed e: Handle(e); break;
}

// 4. Applies state change
private void Handle(OrderConfirmed e)
{
    Status = OrderStatus.Confirmed;
    ConfirmedAtUtc = e.ConfirmedAtUtc;
}
```

## Testing

Comprehensive unit tests are located in `CookieCommerce.OrderDomain.Tests/DomainTests/OrderAggregateTests.cs`:

- **27 test cases** covering all business rules and invariants
- Tests for successful operations
- Tests for exception cases
- Event sourcing replay tests
- Domain event emission tests

Run tests with:
```bash
dotnet test src/CookieCommerce.OrderDomain.Tests/CookieCommerce.OrderDomain.Tests.csproj
```

## Future Enhancements

Potential additions to the Order domain:

1. **Order Shipping**: Add shipping address and tracking information
2. **Partial Payments**: Support for split payments and payment plans
3. **Order History**: Track all status changes with audit trail
4. **Discount/Promotion Support**: Apply coupons and promotional codes
5. **Inventory Reservation**: Reserve inventory when order is confirmed
6. **Integration Events**: Publish events to other bounded contexts (Product, Customer, Payment)

## Related Domains

- **Customer Domain**: Provides customer information for orders
- **Product Domain**: Provides product details and pricing
- **Payment Domain** (future): Handles payment processing
- **Shipping Domain** (future): Manages order fulfillment and delivery

## Dependencies

- `CookieCommerce.DomainBuildingBlocks` - Base classes and patterns for DDD
- Marten (via infrastructure layer) - Event store and document database
- MediatR (via application layer) - CQRS command and query handling
