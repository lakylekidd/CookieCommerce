# Order Domain - Infrastructure Layer

This layer contains the infrastructure concerns for the Order domain, including Marten projections for building read models from event streams.

## Overview

The Order Domain Infrastructure layer provides:
- **Projections**: Marten event projections that transform domain events into optimized read models
- **Infrastructure Services**: Support for external integrations (future)

## Architecture

This layer follows the **Hexagonal Architecture** pattern, implementing the interfaces defined in the Application layer and providing concrete implementations for external concerns.

## Projections

Projections use Marten's `SingleStreamProjection` to build read models from event streams. Each projection handles specific domain events and maintains a denormalized view optimized for querying.

### OrderDetailsProjection

**Purpose:** Provides complete order details including all order lines, calculated totals, and status history.

**Target Read Model:** `OrderDetailsReadModel`

**Event Handlers:**

| Event | Method | Description |
|-------|--------|-------------|
| `OrderPlaced` | `Create()` | Initializes the read model with order ID, customer ID, and initial status. Populates `StatusHistory` with Pending status |
| `OrderLineAdded` | `Apply()` | Adds order line with `PriceInfo` and recalculates SubTotal, TaxAmount, and Total |
| `OrderLineRemoved` | `Apply()` | Removes order line and recalculates totals |
| `OrderConfirmed` | `Apply()` | Updates status, `StatusDate`, and adds entry to `StatusHistory` |
| `OrderPaid` | `Apply()` | Updates status, `StatusDate`, and adds entry to `StatusHistory` |
| `OrderCancelled` | `Apply()` | Updates status, `StatusDate`, and adds entry to `StatusHistory` |
| `OrderRejected` | `Apply()` | Updates status, `StatusDate`, and adds entry to `StatusHistory` |

**Key Features:**
- **Status Tracking**: Maintains both current `StatusDate` and complete `StatusHistory` dictionary
- **PriceInfo**: Each order line includes detailed price information with tax configuration
- **Product Description**: Order lines include product description for display purposes

**Tax Calculation:**
- Tax Rate: 10% (0.10m)
- TaxAmount = SubTotal × TaxRate
- Total = SubTotal + TaxAmount

**Usage:**
```csharp
// Query the projection
var order = await session.LoadAsync<OrderDetailsReadModel>(orderId);

// Access complete order details
Console.WriteLine($"Order {order.Id}: {order.Status}");
Console.WriteLine($"  Status Date: {order.StatusDate}");
Console.WriteLine($"  SubTotal: ${order.SubTotal}");
Console.WriteLine($"  Tax (10%): ${order.TaxAmount}");
Console.WriteLine($"  Total: ${order.Total}");
Console.WriteLine($"  Lines: {order.OrderLines.Count}");

// View status history
foreach (var (status, timestamp) in order.StatusHistory)
{
    Console.WriteLine($"  {status}: {timestamp:yyyy-MM-dd HH:mm}");
}

// Access order line with price info
foreach (var line in order.OrderLines)
{
    Console.WriteLine($"  {line.ProductDescription}: {line.Quantity} x ${line.PriceInfo.BasePrice}");
    Console.WriteLine($"    Tax: {line.PriceInfo.TaxValue * 100}% ({line.PriceInfo.TaxType})");
}
```

---

### CustomerOrderHistoryProjection

**Purpose:** Provides a lightweight summary of all orders for a specific customer, optimized for listing and filtering.

**Target Read Model:** `CustomerOrderHistoryReadModel`

**Event Handlers:**

| Event | Method | Description |
|-------|--------|-------------|
| `OrderPlaced` | `Create()` | Initializes the read model with basic order info |
| `OrderLineAdded` | `Apply()` | Updates the total (approximation) |
| `OrderLineRemoved` | `Apply()` | No-op (limitation: cannot accurately recalculate) |
| `OrderConfirmed` | `Apply()` | Updates status and timestamp |
| `OrderPaid` | `Apply()` | Updates status and timestamp |
| `OrderCancelled` | `Apply()` | Updates status |
| `OrderRejected` | `Apply()` | Updates status |

**Limitations:**
- Total calculation is approximate when order lines are removed
- For accurate totals, query `OrderDetailsReadModel` instead
- This is an intentional trade-off for performance and simplicity

**Usage:**
```csharp
// Query all orders for a customer
var orders = await session.Query<CustomerOrderHistoryReadModel>()
    .Where(o => o.CustomerId == customerId)
    .OrderByDescending(o => o.PlacedAtUtc)
    .ToListAsync();

// Display customer order history
foreach (var order in orders)
{
    Console.WriteLine($"{order.PlacedAtUtc:yyyy-MM-dd} - Order {order.Id}");
    Console.WriteLine($"  Status: {order.Status}, Total: ${order.Total}");
}
```

---

### OpenOrdersProjection

**Purpose:** Provides a list of orders that are still open (not paid, cancelled, or rejected), optimized for operational dashboards and fulfillment workflows.

**Target Read Model:** `OpenOrderReadModel`

**Event Handlers:**

| Event | Method | Description |
|-------|--------|-------------|
| `OrderPlaced` | `Create()` | Initializes the read model for a new open order |
| `OrderLineAdded` | `Apply()` | Updates the total (approximation) |
| `OrderLineRemoved` | `Apply()` | No-op (limitation: cannot accurately recalculate) |
| `OrderConfirmed` | `Apply()` | Updates status and timestamp |
| `OrderPaid` | `DeleteEvent<>()` | **Removes the read model** (order is closed) |
| `OrderCancelled` | `DeleteEvent<>()` | **Removes the read model** (order is closed) |
| `OrderRejected` | `DeleteEvent<>()` | **Removes the read model** (order is closed) |

**Key Feature:** Automatic cleanup of closed orders using Marten's `DeleteEvent<>()` mechanism.

**Usage:**
```csharp
// Query all open orders
var openOrders = await session.Query<OpenOrderReadModel>()
    .OrderByDescending(o => o.PlacedAtUtc)
    .ToListAsync();

// Display fulfillment queue
Console.WriteLine($"Open Orders: {openOrders.Count}");
foreach (var order in openOrders)
{
    Console.WriteLine($"  {order.Id} - {order.Status} - ${order.Total}");
}
```

---

## Projection Lifecycle

Projections are registered as **Inline** projections, which means:
- Read models are updated **synchronously** during event append
- Queries always see the latest state
- No eventual consistency lag
- Higher write latency (acceptable for this domain)

**Alternative:** Async projections could be used for better write performance but would introduce eventual consistency.

## Dependency Injection

Register infrastructure services in `Startup.cs` or `Program.cs`:

```csharp
// In Order API layer (when created)
builder.Services.AddInfrastructure(builder.Configuration);

// In MartenExtensions.cs
services.AddMarten(opts =>
{
    var connectionString = configuration.GetConnectionString("OrderDomain");
    opts.Connection(connectionString);
    
    // Register projections
    opts.Projections.Add(new OrderDetailsProjection(), 
        JasperFx.Events.Projections.ProjectionLifecycle.Inline);
    opts.Projections.Add(new CustomerOrderHistoryProjection(), 
        JasperFx.Events.Projections.ProjectionLifecycle.Inline);
    opts.Projections.Add(new OpenOrdersProjection(), 
        JasperFx.Events.Projections.ProjectionLifecycle.Inline);
        
    opts.AutoCreateSchemaObjects = JasperFx.AutoCreate.All;
})
.ApplyAllDatabaseChangesOnStartup();
```

## Testing

Projection tests are located in `CookieCommerce.OrderDomain.Tests/ProjectionsTests/`.

**Test Pattern:**
```csharp
public class OrderDetailsProjectionTests
{
    [Fact]
    public void Apply_OrderLineAdded_AddsLineAndRecalculatesTotals()
    {
        // Arrange
        var projection = new OrderDetailsProjection();
        var current = new OrderDetailsReadModel { /* ... */ };
        var @event = new OrderLineAdded { /* ... */ };
        
        // Act
        var result = projection.Apply(@event, current);
        
        // Assert
        result.OrderLines.Should().HaveCount(1);
        result.Total.Should().Be(expectedTotal);
    }
}
```

Tests validate:
- Event-to-read-model transformations
- Total calculations
- Status transitions
- Edge cases (empty orders, multiple lines, etc.)

## Future Enhancements

1. **Multi-Projection Queries**: Combine data from multiple read models
2. **Projection Rebuild**: Support for rebuilding projections from event streams
3. **Async Projections**: Option for eventual consistency with better write performance
4. **Custom Indexes**: Optimize queries with database indexes
5. **Archived Orders**: Separate projection for historical/archived orders

## Related Documentation

- [Order Domain - Domain Layer](../CookieCommerce.OrderDomain.Domain/README.md)
- [Order Domain - Application Layer](../CookieCommerce.OrderDomain.Application/README.md)
- [Main Solution README](../../README.md)
- [Marten Documentation](https://martendb.io/)
