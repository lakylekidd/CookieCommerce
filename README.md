# 🍪 Cookie Commerce

Modular e‑commerce platform built with .NET 10, DDD, CQRS, and Event Sourcing

CookieCommerce is a modern, modular, event‑sourced e‑commerce system designed for your cookie business, Tiger van Cookie, and future‑proofed for any type of product. The system follows Domain‑Driven Design, Vertical Slice Architecture, CQRS, and Event Sourcing using Marten, MediatR, MassTransit, and Aspire.

This README provides an overview of the architecture, conventions, and development guidelines used across all bounded contexts.

---

## Live commerce services

The storefront at `/` now uses the ProductDomain and CustomerDomain APIs together with a PostgreSQL **Commerce** database for carts, checkout, orders, favorites, reviews, production batches, settings and subscriptions. Stripe is the default hosted-payment provider; Mollie is also implemented behind the payment gateway interface. Payment confirmation comes from verified provider notifications. Mock services and sample commerce records have moved into test fixtures.

See [the service audit and setup guide](Documentation/commerce-services.md) for payment environment variables, webhook setup, persistence and remaining integration boundaries. AppHost now provisions the Commerce database. Keep the web application's Data Protection keys on a persistent volume alongside the database backups.

Development startup seeds ten customer accounts (the existing admin/customer users plus eight reproducible fictional customers), the original six cookie products and their single/5/12-cookie variants, ten unique cookie/packaging SKUs with their recipes, and 100 customer-owned demo orders: 90 shipped, 2 ready to ship and 8 processing. Existing demo orders receive this distribution once; later status edits are preserved. Orders are marked as development data and never call payment providers. Every development restart restores each cookie's available stock to 30 and each packaging SKU to 100, preserving other customer, catalog, recipe, order and production edits. See [development seeding](Documentation/commerce-services.md#development-seeding) for details and test accounts. Set `DevelopmentBootstrap__Enabled=false` to disable it; demo seeding is disabled outside Development.

Product thumbnails in carts, checkout, order history/details, customer administration, dashboard rankings and inventory use the current catalog main image, including for inactive products. Display-time image refresh leaves financial order snapshots intact; variant photos remain available in variant galleries. Missing main images use the logo placeholder.

AppHost uses named PostgreSQL, RabbitMQ and Keycloak data volumes with persistent container lifetimes. Product media and the Commerce key ring use `%LOCALAPPDATA%/CookieCommerce` on Windows (override `DevelopmentStorage:RootPath` for another location). These files and volumes must be retained together; changing the key directory or deleting data volumes loses access to saved development data.

## 🚀 Features

Customer account administration, password recovery, configurable SMTP, versioned email/PDF templates and private PDF invoices are available in the back office. All seeded development accounts use password `123`. See [customer access, email and invoice setup](Documentation/customer-access-email-invoices.md) for configuration and verification details.

- Bounded contexts: `Customer`, `Catalog/Product`, `Ordering`, `Payments`, `Inventory (future)`, `Notifications (future)`
- Event Sourcing with Marten
- CQRS with MediatR
- MassTransit for integration events (with Outbox pattern)
- Blazor UI for customer‑facing and admin experiences
- Checkout delivery methods are resolved from the selected shipping address
- Anonymous-safe account reads return empty/null values instead of throwing
- Checkout address entry switches between address-book selection and Dutch lookup based on account state
- Keycloak authentication persists in protected browser storage and refreshes expired access tokens
- Field‑level encryption for sensitive PII
- Vertical Slice Architecture for clean separation by feature
- Aspire for orchestration, service discovery, and local infrastructure

---

## 🧱 Solution Structure
```
CookieCommerce/
├── src/
│   ├── CookieCommerce.{Domain}.Api/           # REST API (Controllers)
│   ├── CookieCommerce.{Domain}.Application/   # Commands, Queries, Handlers
│   ├── CookieCommerce.{Domain}.Domain/        # Aggregates, Events, Value Objects
│   ├── CookieCommerce.{Domain}.Infrastructure/# Projections, Marten, Outbox
│   ├── CookieCommerce.{Domain}.Tests/         # Unit & Integration tests
│   ├── CookieCommerce.DomainBuildingBlocks/   # Shared domain abstractions
│   ├── CookieCommerce.CookieMassTransit/      # Shared MassTransit config
│   ├── CookieCommerce.ServiceDefaults/        # Aspire defaults
│   └── CookieCommerce.Web/                    # Blazor UI

```
Each domain is isolated and follows the same structure for consistency and maintainability.

---

## 🧭 Architectural Principles
**Domain‑Driven Design**
- Aggregates enforce invariants
- Domain events represent state changes
- Value objects are immutable
- Domain exceptions express business rule violations

**Event Sourcing**
- All state changes are stored as events
- Aggregates rehydrate from event streams
- Read models are built via projections

**CQRS**
- Commands mutate state
- Queries read state
- Handlers are single‑responsibility vertical slices

**Vertical Slice Architecture**
- Features are grouped by domain behavior, not layers
- Each slice contains its own commands, queries, handlers, and models

---

🧩 Domain Conventions

**Aggregates**
- Located in `{Domain}.Domain/Aggregates`
- Inherit from AggregateBase
- Apply events via `Apply()` and `When()`
- Use `Guid.CreateVersion7()` for IDs

**Domain Events**
- Located in `{Domain}.Domain/DomainEvents`
- Named `{Entity}{PastTenseVerb} (e.g., CustomerCreated)`
- Mutable classes for Marten serialization

**Commands & Queries**
- Immutable records
- Located in `{Domain}.Application/{Feature}/Commands and Queries`
- Handlers use primary constructors

**Projections**
- Located in `{Domain}.Infrastructure/Projections`
- Use Marten `SingleStreamProjection<TReadModel, TId>`
- Decrypt PII in projections only

**API Layer**
- Controllers use primary constructors
- DTOs in `Models/{Feature}`
- Mapping extensions in `{Feature}Mapper.cs`
- Use ProblemDetails for errors
- POST returns `CreatedAtAction`

---

## 🔐 Security & Data Protection
- Sensitive PII (email, names, DOB, gender) is encrypted at rest
- Encryption keys stored per customer
- Events store only encrypted values
- Projections decrypt using customer‑specific keys
- GDPR “forget customer” supported via key shredding

---

## 🔄 Outbox Pattern (Messaging)
CookieCommerce uses a transactional outbox for reliable integration event publishing.

**Key points:**
- Outbox messages are written in the same transaction as domain events
- MassTransit publishes integration events
- Background publisher processes outbox messages
- PostgreSQL LISTEN/NOTIFY + fallback polling ensures immediate dispatch
- A Marten `ISchemaObject` installs:
   - `notify_outbox()` function
   - `outbox_notify` trigger
- Shared outbox infrastructure is reused across domains (DRY)

---

## 🧪 Testing
- xUnit + FluentAssertions
- NSubstitute for mocks
- Testcontainers for PostgreSQL
- Integration tests use WebApplicationFactory
- Projection tests validate event → read model behavior

---

## 🛠️ Coding Standards
- .NET 10, C# 14
- File‑scoped namespaces
- Primary constructors for DI
- Immutable DTOs and commands
- Methods < 20 lines
- Avoid deep nesting
- Domain exceptions instead of generic exceptions
- Always use async/await

---

## 🧰 Development Workflow
- Add new domain events → update aggregate → update projections
- Add commands/queries → implement handlers
- Add API endpoints → map DTOs → update README
- Add tests (domain, application, integration)
- Run locally via Aspire AppHost

---

## 🔗 Cross-Domain Integration

CookieCommerce domains are designed to be loosely coupled using **event-driven eventual consistency**.

### Event-Driven Integration (Recommended)

Domains communicate asynchronously through integration events via MassTransit + RabbitMQ:

**Publishing Integration Events:**
```csharp
// In Customer domain (CreateCustomerHandler)
await publisher.PublishAsync(new CustomerSnapshotPublished
{
    CustomerId = customerId,
    FirstName = request.FirstName,
    LastName = request.LastName,
    Email = request.Email
}, cancellationToken);
```

**Consuming Integration Events:**
```csharp
// In Order domain
public class CustomerSnapshotPublishedConsumer(IDocumentSession session) 
    : IConsumer<CustomerSnapshotPublished>
{
    public async Task Consume(ConsumeContext<CustomerSnapshotPublished> context)
    {
        var snapshot = new CustomerSnapshotReadModel 
        { 
            Id = context.Message.CustomerId,
            FirstName = context.Message.FirstName,
            // ... populate fields
        };
        
        session.Store(snapshot);
        await session.SaveChangesAsync();
    }
}
```

**Key Principles:**
- **Eventual Consistency**: Read models are updated asynchronously via events
- **No Runtime Dependencies**: Services don't make synchronous HTTP calls to each other
- **Local Read Models**: Each domain maintains its own denormalized views
- **Fault Tolerance**: Services work independently even if others are down
- **MassTransit**: Handles reliable message delivery with retries

**Example: Order → Customer Integration**

The Order domain maintains a local copy of customer data:

1. Customer domain publishes `CustomerSnapshotPublished` on create/update
2. Order domain's `CustomerSnapshotPublishedConsumer` receives event
3. Consumer updates local `CustomerSnapshotReadModel` in Order database
4. `PlaceOrderHandler` queries local read model (no HTTP call needed)
5. If customer doesn't exist locally → throws `CustomerNotFoundException`

**Customer Snapshot Pattern:**
- Customer data replicated to Order domain via integration events
- Email addresses are masked when stored in orders (e.g., `j***@example.com`)
- Names are denormalized for order context
- Snapshots captured at event time, providing temporal consistency

### Integration Events (Async, via RabbitMQ)

For cross-domain notifications, domains publish integration events:

```csharp
// Customer domain events
- CustomerSnapshotPublished (on create/update)
- CustomerDeletedIntegrationEvent (on forget/delete)

// Product domain events  
- ProductSnapshotPublished (on create/update)
- ProductDeactivatedIntegrationEvent (on deactivation)
```

Integration events are used for:
- Replicating data across bounded contexts
- Achieving eventual consistency
- Avoiding tight coupling between domains
- Building resilient distributed systems

---

# 📚 Resources
- Marten: https://martendb.io
- MediatR: https://github.com/jbogard/MediatR
- MassTransit: https://masstransit.io
- .NET Aspire: https://learn.microsoft.com/en-us/dotnet/aspire (learn.microsoft.com in Bing)
- DDD: Eric Evans
- Event Sourcing: https://martinfowler.com/eaaDev/EventSourcing.html (martinfowler.com in Bing)
