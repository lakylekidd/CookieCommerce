# Order Domain - Application Layer

This layer contains the CQRS commands, queries, and their handlers for the Order domain. It follows the patterns established in the Customer and Product domains.

## Overview

The Order Domain Application layer provides the use cases for managing orders throughout their lifecycle:
- Placing new orders
- Adding/removing products to/from pending orders
- Confirming orders for fulfillment
- Processing payments
- Cancelling or rejecting orders

## Architecture

This layer follows **CQRS** (Command Query Responsibility Segregation) and **Vertical Slice Architecture**:

- **Commands**: Write operations that mutate state
- **Queries**: Read operations (to be implemented)
- **Handlers**: Single-responsibility classes that process commands/queries
- **Integration**: Events published to other bounded contexts (future)

## Commands

All commands are immutable records that implement `IRequest` or `IRequest<TResult>` from MediatR.

### PlaceOrder

Creates a new order for a customer.

```csharp
public record PlaceOrder(Guid CustomerId) : IRequest<Guid>;
```

**Parameters:**
- `CustomerId`: The ID of the customer placing the order

**Returns:** The newly created Order ID (Guid)

**Business Rules:**
- Customer ID must not be empty
- Creates order in `Pending` status

**Usage:**
```csharp
var command = new PlaceOrder(customerId);
var orderId = await mediator.Send(command);
```

---

### AddOrderLine

Adds a product line item to a pending order.

```csharp
public record AddOrderLine(
    Guid OrderId,
    Guid ProductId,
    int Quantity,
    decimal UnitPrice) : IRequest;
```

**Parameters:**
- `OrderId`: The ID of the order to modify
- `ProductId`: The ID of the product to add
- `Quantity`: Number of units
- `UnitPrice`: Price per unit at time of ordering

**Business Rules:**
- Order must exist
- Order must be in `Pending` status
- Product cannot already exist in the order (use quantity adjustment instead)
- Quantity must be greater than 0
- Unit price must be greater than 0

**Throws:**
- `OrderNotFoundException`: If the order doesn't exist
- `CannotModifyNonPendingOrderException`: If order is not pending
- `DuplicateOrderLineException`: If product already in order

**Usage:**
```csharp
var command = new AddOrderLine(orderId, productId, quantity: 2, unitPrice: 10.50m);
await mediator.Send(command);
```

---

### RemoveOrderLine

Removes a product line item from a pending order.

```csharp
public record RemoveOrderLine(
    Guid OrderId,
    Guid ProductId) : IRequest;
```

**Parameters:**
- `OrderId`: The ID of the order to modify
- `ProductId`: The ID of the product to remove

**Business Rules:**
- Order must exist
- Order must be in `Pending` status
- Product must exist in the order

**Throws:**
- `OrderNotFoundException`: If the order doesn't exist
- `CannotModifyNonPendingOrderException`: If order is not pending
- `OrderLineNotFoundException`: If product not in order

**Usage:**
```csharp
var command = new RemoveOrderLine(orderId, productId);
await mediator.Send(command);
```

---

### ConfirmOrder

Confirms a pending order, locking it for fulfillment.

```csharp
public record ConfirmOrder(Guid OrderId) : IRequest;
```

**Parameters:**
- `OrderId`: The ID of the order to confirm

**Business Rules:**
- Order must exist
- Order cannot be `Cancelled` or `Rejected`
- Order must have at least one order line
- Transitions order to `Confirmed` status
- Idempotent: If already confirmed, no-op

**Throws:**
- `OrderNotFoundException`: If the order doesn't exist
- `CannotConfirmCancelledOrderException`: If order is cancelled
- `CannotConfirmRejectedOrderException`: If order is rejected
- `EmptyOrderException`: If order has no line items

**Usage:**
```csharp
var command = new ConfirmOrder(orderId);
await mediator.Send(command);
```

---

### MarkOrderAsPaid

Marks a confirmed order as paid after payment processing.

```csharp
public record MarkOrderAsPaid(Guid OrderId) : IRequest;
```

**Parameters:**
- `OrderId`: The ID of the order to mark as paid

**Business Rules:**
- Order must exist
- Order must be in `Confirmed` status
- Transitions order to `Paid` status
- Idempotent: If already paid, no-op

**Throws:**
- `OrderNotFoundException`: If the order doesn't exist
- `InvalidOperationException`: If order is not confirmed

**Usage:**
```csharp
var command = new MarkOrderAsPaid(orderId);
await mediator.Send(command);
```

---

### CancelOrder

Cancels an order (customer or admin action).

```csharp
public record CancelOrder(
    Guid OrderId,
    string Reason) : IRequest;
```

**Parameters:**
- `OrderId`: The ID of the order to cancel
- `Reason`: The reason for cancellation (required)

**Business Rules:**
- Order must exist
- Reason must not be empty or whitespace
- Transitions order to `Cancelled` status
- Idempotent: If already cancelled, no-op

**Throws:**
- `OrderNotFoundException`: If the order doesn't exist
- `ArgumentException`: If reason is empty

**Usage:**
```csharp
var command = new CancelOrder(orderId, "Customer requested cancellation");
await mediator.Send(command);
```

---

### RejectOrder

Rejects an order (admin action, e.g., fraud detection, inventory issues).

```csharp
public record RejectOrder(
    Guid OrderId,
    string Reason) : IRequest;
```

**Parameters:**
- `OrderId`: The ID of the order to reject
- `Reason`: The reason for rejection (required)

**Business Rules:**
- Order must exist
- Reason must not be empty or whitespace
- Transitions order to `Rejected` status
- Idempotent: If already rejected, no-op

**Throws:**
- `OrderNotFoundException`: If the order doesn't exist
- `ArgumentException`: If reason is empty

**Usage:**
```csharp
var command = new RejectOrder(orderId, "Payment verification failed");
await mediator.Send(command);
```

---

## Handlers

Each command has a corresponding handler that implements `IRequestHandler<TCommand>` or `IRequestHandler<TCommand, TResult>`.

### Handler Pattern

All handlers follow this pattern:

1. **Load aggregate** from event store (except for PlaceOrder which creates new)
2. **Validate** the aggregate exists (throw `OrderNotFoundException` if not)
3. **Call aggregate method** to apply business logic
4. **Append events** to the stream (or start new stream)
5. **Save changes** to commit transaction

**Example:**
```csharp
public class ConfirmOrderHandler(IDocumentSession session) : IRequestHandler<ConfirmOrder>
{
    public async Task Handle(ConfirmOrder request, CancellationToken cancellationToken)
    {
        // 1. Load aggregate
        var order = await session.Events.AggregateStreamAsync<OrderAggregate>(
            request.OrderId, 
            token: cancellationToken)
            ?? throw new OrderNotFoundException(request.OrderId); // 2. Validate
        
        // 3. Call aggregate method
        order.Confirm();
        
        // 4. Append events
        session.Events.Append(request.OrderId, order.DomainEvents);
        
        // 5. Save changes
        await session.SaveChangesAsync(cancellationToken);
    }
}
```

### Handler Dependencies

- **IDocumentSession**: Marten session for event sourcing operations
  - `session.Events.StartStream<T>()`: Create new event stream
  - `session.Events.AggregateStreamAsync<T>()`: Rehydrate aggregate from events
  - `session.Events.Append()`: Add new events to existing stream
  - `session.SaveChangesAsync()`: Commit transaction

## Order Lifecycle

```
        PlaceOrder
            ↓
    [Pending] ←──────────────────┐
        │                        │
        │ AddOrderLine           │
        │ RemoveOrderLine        │
        │                        │
        ↓                        │
    ConfirmOrder                 │
        ↓                        │
    [Confirmed]                  │
        │                        │
        ↓                        │
    MarkOrderAsPaid              │
        ↓                        │
    [Paid]                       │
                                 │
    CancelOrder/RejectOrder ─────┘
        ↓
    [Cancelled/Rejected]
```

## Order Status Values

From `OrderStatus` enum in the Domain layer:

- **Pending**: Order created, can be modified
- **Confirmed**: Order locked for fulfillment
- **Paid**: Payment received
- **Cancelled**: Order cancelled (customer or admin)
- **Rejected**: Order rejected (admin, e.g., fraud)

## Business Rule Summary

| Command | Status Required | Can Modify Lines | Idempotent | Returns Value |
|---------|----------------|------------------|------------|---------------|
| PlaceOrder | N/A (creates) | N/A | No | Guid |
| AddOrderLine | Pending | Yes | No | void |
| RemoveOrderLine | Pending | Yes | No | void |
| ConfirmOrder | Not Cancelled/Rejected | No | Yes | void |
| MarkOrderAsPaid | Confirmed | No | Yes | void |
| CancelOrder | Any | No | Yes | void |
| RejectOrder | Any | No | Yes | void |

## Testing

Tests are located in `CookieCommerce.OrderDomain.Tests/ApplicationTests/`.

Each handler has a corresponding test class using:
- **xUnit**: Test framework
- **FluentAssertions**: Readable assertions
- **NSubstitute**: Mocking Marten session

**Test Pattern:**
```csharp
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
        var order = CreateValidPendingOrder();
        _session.Events.AggregateStreamAsync<OrderAggregate>(...)
            .Returns(order);
        
        // Act
        await _handler.Handle(new ConfirmOrder(orderId), CancellationToken.None);
        
        // Assert
        order.Status.Should().Be(OrderStatus.Confirmed);
    }
}
```

## Integration

### MediatR Registration

Register all handlers in `DependencyInjection.cs`:

```csharp
public static IServiceCollection AddApplication(this IServiceCollection services)
{
    services.AddMediatR(cfg => 
        cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
    return services;
}
```

### API Layer Integration

In the API layer (when created), inject `ISender`:

```csharp
public class OrderController(ISender mediator) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<Guid>> PlaceOrder([FromBody] PlaceOrderRequest request)
    {
        var command = request.ToCommand();
        var orderId = await mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = orderId }, orderId);
    }
}
```

## Domain Events

Each command triggers domain events in the aggregate:

| Command | Domain Event(s) |
|---------|----------------|
| PlaceOrder | `OrderPlaced` |
| AddOrderLine | `OrderLineAdded` |
| RemoveOrderLine | `OrderLineRemoved` |
| ConfirmOrder | `OrderConfirmed` |
| MarkOrderAsPaid | `OrderPaid` |
| CancelOrder | `OrderCancelled` |
| RejectOrder | `OrderRejected` |

## Future Enhancements

1. **Queries**: Read models and query handlers
2. **Projections**: Marten projections for optimized reads
3. **Integration Events**: Publish to other bounded contexts
4. **Outbox Pattern**: Transactional messaging
5. **Validation**: FluentValidation for command validation
6. **API Layer**: REST endpoints and DTOs
7. **Authorization**: Role-based access for admin operations

## Related Documentation

- [Order Domain - Domain Layer](../CookieCommerce.OrderDomain.Domain/README.md)
- [Main Solution README](../../README.md)
- [Customer Domain Application](../CookieCommerce.CustomerDomain.Application/README.md)
- [Product Domain Application](../CookieCommerce.ProductDomain.Application/README.md)
