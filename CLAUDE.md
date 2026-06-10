# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
# Build
dotnet build src

# Run all tests
dotnet test src

# Run tests for a specific domain
dotnet test src/CookieCommerce.CustomerDomain.Tests/CookieCommerce.CustomerDomain.Tests.csproj

# Run a specific test
dotnet test src --filter "FullyQualifiedName~CreateCustomerTests.ShouldCreateCustomer"

# Start the app (Aspire orchestrator — launches all services + infrastructure)
dotnet run --project src/CookieCommerce.AppHost
```

## Architecture

**CookieCommerce** is a cookie e-commerce platform built with Domain-Driven Design, Vertical Slice Architecture, CQRS, and Event Sourcing.

**Tech stack:** .NET 10, Blazor (frontend), Marten (event store + document DB on PostgreSQL), MediatR (CQRS), MassTransit + RabbitMQ (async messaging), Aspire (local orchestration), xUnit + Testcontainers (integration tests).

### Domain Layout

Each bounded context follows this structure:

```
CookieCommerce.{Domain}.Api/           # Controllers, DTOs, middleware
CookieCommerce.{Domain}.Application/   # Commands, queries, MediatR handlers
CookieCommerce.{Domain}.Domain/        # Aggregates, domain events, exceptions
CookieCommerce.{Domain}.Infrastructure/# Marten projections, outbox, DI config
CookieCommerce.{Domain}.Tests/         # Unit + integration tests
```

Current domains: **Customer**, **Product**, **Order**, **Catalog**.

Shared libraries:
- `CookieCommerce.DomainBuildingBlocks` — `AggregateBase`, `IIntegrationEventPublisher`
- `CookieCommerce.CookieMassTransit` — MassTransit/RabbitMQ wiring
- `CookieCommerce.ServiceDefaults` — Aspire service discovery, health checks, OpenTelemetry
- `CookieCommerce.AppHost` — Aspire orchestration entry point

### Key Patterns

#### Event Sourcing (Aggregates)

All aggregates extend `AggregateBase`. State changes go through `Apply(domainEvent)`, which appends to `_uncommittedEvents` and calls `When()` to update in-memory state.

```csharp
public class CustomerAggregate : AggregateBase
{
    public void Create(Guid id, string encryptedEmail, ...)
    {
        // validate invariants here; throw domain exceptions on failure
        Apply(new CustomerCreated { CustomerId = id, EncryptedEmail = encryptedEmail, ... });
    }

    protected override void When(object @event)
    {
        switch (@event)
        {
            case CustomerCreated e: Id = e.CustomerId; EncryptedEmail = e.EncryptedEmail; break;
        }
    }
}
```

Handlers write to Marten via `session.Events.Append(id, aggregate.DomainEvents.ToArray())`, then call `aggregate.ClearUncommitedEvents()` before `session.SaveChangesAsync()`.

Domain events must be **mutable classes** (Marten serialization requirement).

#### Projections (Event → Read Model)

Projections live in the Infrastructure layer and transform domain events into queryable read models stored by Marten.

```csharp
public class CustomerProjection : SingleStreamProjection<CustomerReadModel, Guid>
{
    public CustomerProjection() { DeleteEvent<CustomerForgotten>(); } // GDPR crypto-shred

    public async Task<CustomerReadModel> Create(CustomerCreated e, IQuerySession session)
    {
        // async Create for first event; decrypt PII here using customer-specific key
    }

    public CustomerReadModel Apply(CustomerAddressAdded e, CustomerReadModel current) { ... }
}
```

Projections are registered as `ProjectionLifecycle.Inline` — they update synchronously with event writes.

PII (email, name) is **encrypted in domain events** and **decrypted in projections** using per-customer AES keys stored separately. Deleting the key (crypto-shredding) is how GDPR erasure works.

#### CQRS with MediatR

Commands mutate state via `ICommandHandler<TCommand, TResult>`; queries read from Marten via `IQueryHandler<TQuery, TResult>`. Handlers are auto-registered via `AddMediatR()`.

Queries load denormalized read models directly: `session.LoadAsync<ProductReadModel>(id)`.

#### Cross-Domain Integration (No Sync HTTP Calls)

Domains never call each other's APIs. They communicate exclusively via integration events over RabbitMQ (MassTransit).

The **Outbox pattern** ensures transactional delivery: the integration event is written to an `OutboxMessage` document in the same Marten transaction as the domain events, then a `BackgroundService` (`OutboxBackgroundPublisher`) polls every 5 seconds to publish unprocessed messages to the bus.

When domain B needs data from domain A, it consumes an integration event and stores a local snapshot read model. Example: Order domain stores a `CustomerSnapshotReadModel` by consuming `CustomerSnapshotPublished` — no live call to Customer API at order time.

MassTransit auto-discovers `IConsumer<T>` implementations via `RegisterEndpointsFromAssembly()`.

#### Exception Handling

Domain exceptions (e.g., `CustomerNotFoundException`, `DuplicateAddressLabelException`) are caught by `DomainExceptionHandlerMiddleware`, which maps them to appropriate HTTP problem responses. Add new domain exceptions to the middleware's switch expression.

### Testing

Unit tests cover aggregate logic with no infrastructure. Integration tests use Testcontainers to spin up real PostgreSQL and use `WebApplicationFactory<Program>` for API-level tests.

```csharp
public class CreateProductTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder().Build();

    public async Task InitializeAsync() => await _container.StartAsync();
    public async Task DisposeAsync() => await _container.DisposeAsync();

    [Fact]
    public async Task ShouldProjectProductOnCreate() { ... }
}
```

## Documentation

### Backend API Contracts

`Documentation/backend-documentation.md` catalogs every service interface in `src/CookieCommerce.Web/Services/` and assigns each method to a backend domain (Authentication Management, Order, Inventory, Cart, Logistics, Customer Management, Store Management).

**Rule: whenever a file matching `src/CookieCommerce.Web/Services/**/*.cs` is added, modified, or deleted and contains an interface, update `Documentation/backend-documentation.md` in the same change.** Specifically:
- New interface → add methods to the relevant domain section and update the Domain Overview table.
- Existing interface gains or loses methods → update the method table in the relevant domain section(s).
- Interface moves domain or is deleted → update or remove the section and the Domain Overview table.
- Update the *Last updated* footer line with today's date and the current commit.

---

### Configuration

Aspire injects connection strings at runtime. For running individual services locally, set them in `appsettings.json` or user secrets:

```bash
dotnet user-secrets set "ConnectionStrings:ProductDomain" "Host=localhost;..."
```
