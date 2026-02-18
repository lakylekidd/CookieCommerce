# 🍪 Cookie Commerce

Modular e‑commerce platform built with .NET 10, DDD, CQRS, and Event Sourcing

CookieCommerce is a modern, modular, event‑sourced e‑commerce system designed for your cookie business, Tiger van Cookie, and future‑proofed for any type of product. The system follows Domain‑Driven Design, Vertical Slice Architecture, CQRS, and Event Sourcing using Marten, MediatR, MassTransit, and Aspire.

This README provides an overview of the architecture, conventions, and development guidelines used across all bounded contexts.

---

## 🚀 Features

- Bounded contexts: `Customer`, `Catalog/Product`, `Ordering`, `Payments`, `Inventory (future)`, `Notifications (future)`
- Event Sourcing with Marten
- CQRS with MediatR
- MassTransit for integration events (with Outbox pattern)
- Blazor UI for customer‑facing and admin experiences
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

CookieCommerce domains are designed to be loosely coupled, with clear integration patterns for cross-domain data access.

### Service-to-Service Communication

**HTTP Client Integration (Read-Side Only)**

When one domain needs to read data from another domain, it uses HTTP clients with Aspire service discovery:

```csharp
// In Infrastructure layer DependencyInjection
services.AddHttpClient<ICustomerService, CustomerService>(client =>
{
    client.BaseAddress = new Uri("http://customerdomainapi");
});
```

**Key Principles:**
- Use service name-based URLs (e.g., `http://customerdomainapi`) for Aspire service discovery
- Implement graceful failure handling (return null/throw domain exception)
- Cache sparingly—prefer fresh data from source
- Use dedicated service interfaces (e.g., `ICustomerService`) in Application layer

**Example: Order → Customer Integration**

The Order domain validates customer existence when placing an order:

1. `PlaceOrderHandler` calls `ICustomerService.GetCustomerByIdAsync()`
2. If customer not found → throws `CustomerNotFoundException`
3. If customer found → creates `CustomerSnapshot` with masked/non-PII data
4. Snapshot stored in `OrderPlaced` event and projected to `OrderDetailsReadModel`

**Customer Snapshot Pattern:**
- Store only essential, non-PII customer data with orders
- Email addresses are masked (e.g., `j***@example.com`)
- Names are denormalized but not considered sensitive in this context
- Snapshots captured at event time, not queried live

### Integration Events (Async, Future)

For asynchronous cross-domain notifications, use MassTransit with the Outbox pattern:

```csharp
// Publish integration event
await _publisher.Publish(new ProductDeactivatedIntegrationEvent(productId));
```

Integration events are used for:
- Notifying other domains of state changes
- Eventual consistency across bounded contexts
- Avoiding tight coupling between domains

---

# 📚 Resources
- Marten: https://martendb.io
- MediatR: https://github.com/jbogard/MediatR
- MassTransit: https://masstransit.io
- .NET Aspire: https://learn.microsoft.com/en-us/dotnet/aspire (learn.microsoft.com in Bing)
- DDD: Eric Evans
- Event Sourcing: https://martinfowler.com/eaaDev/EventSourcing.html (martinfowler.com in Bing)