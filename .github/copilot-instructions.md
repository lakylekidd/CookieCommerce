# CookieCommerce - GitHub Copilot Instructions

## Project Overview
CookieCommerce is a modular e-commerce system built using **.NET 10**, **Domain-Driven Design (DDD)**, **Event Sourcing**, and **CQRS** patterns. It uses **Marten** for event store/document database, **MediatR** for CQRS, **MassTransit** for messaging, and **Aspire** for orchestration.

## Architecture & Structure

### Solution Structure
The solution follows **Vertical Slice Architecture** with bounded contexts organized as separate domains:

```
CookieCommerce/
├── src/
│   ├── CookieCommerce.{Domain}.Api/           # REST API endpoints (Controllers)
│   ├── CookieCommerce.{Domain}.Application/   # Application logic (Commands/Queries/Handlers)
│   ├── CookieCommerce.{Domain}.Domain/        # Domain models (Aggregates/Entities/Value Objects/Events)
│   ├── CookieCommerce.{Domain}.Infrastructure/# Infrastructure (Projections/Repositories/External Services)
│   ├── CookieCommerce.{Domain}.Tests/         # Unit & Integration tests
│   ├── CookieCommerce.DomainBuildingBlocks/  # Shared domain patterns
│   ├── CookieCommerce.ServiceDefaults/        # Aspire service defaults
│   ├── CookieCommerce.CookieMassTransit/      # Shared MassTransit configuration
│   ├── CookieCommerce.AppHost/                # Aspire orchestration host
│   └── CookieCommerce.Web/                    # Frontend (Blazor)
```

### Layer Responsibilities

#### **{Domain}.Api**
- ASP.NET Core Web API with **Controllers** (prefer controllers over Minimal APIs)
- Request/Response models (DTOs) in `Models/{Feature}/` folders
- Request mapping extensions in `Models/{Feature}/{Feature}Mapper.cs`
- API-specific middleware
- OpenAPI/Swagger configuration
- Endpoint routing: `[Route("api/[controller]")]`
- Use **ProblemDetails** for error responses

**Conventions:**
- Controllers use primary constructor: `public CustomerController(ISender mediator) : ControllerBase`
- Request models use `[Required]` data annotations for validation
- Mapping extensions: `public static CreateCustomer ToCommand(this CreateCustomerRequest request)`
- Controllers validate `ModelState` before processing
- Return `CreatedAtAction("GetById", new { id }, id)` for POST operations

#### **{Domain}.Application**
- **Commands** (write operations) in `{Feature}/Commands/`
- **Queries** (read operations) in `{Feature}/Queries/`
- **Handlers** in `{Feature}/Handlers/` implementing `IRequestHandler<TRequest, TResponse>`
- **ReadModels** in `{Feature}/ReadModels/`
- Domain event handlers for internal events
- Integration event publishers (future: via Outbox pattern)

**Conventions:**
- Commands/Queries are **immutable records**: `public record CreateCustomer(...) : IRequest<Guid>;`
- Handlers use **primary constructors**: `public class CreateCustomerHandler(IDocumentSession session, ...) : IRequestHandler<CreateCustomer, Guid>`
- ReadModels are **mutable classes** with `{ get; set; }` for projection updates
- MediatR registration in `DependencyInjection.cs`: `services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));`

#### **{Domain}.Domain**
- **Aggregates** in `Aggregates/` (inherit from `AggregateBase`)
- **Value Objects** in `ValueObjects/` (immutable)
- **Domain Events** in `DomainEvents/` (mutable classes for Marten serialization)
- **Domain Exceptions** in `Exceptions/` (custom exceptions with context)
- **Business logic and invariants** enforced in aggregates

**Conventions:**
- Aggregates inherit `AggregateBase` and implement `When(object @event)` for event application
- Use `Guid.CreateVersion7()` for IDs
- Domain events are applied via `Apply(domainEvent)` method
- Aggregates track uncommitted events in `DomainEvents` collection
- Value objects use **init-only properties** or constructor-based immutability
- Domain exceptions provide structured context (e.g., `CustomerId`, `Label`)

**Example Aggregate Pattern:**
```csharp
public class CustomerAggregate : AggregateBase
{
    public string EncryptedEmail { get; private set; } = default!;
    
    public void Create(Guid id, string email, ...)
    {
        // Validate
        if (string.IsNullOrEmpty(email))
            throw new DomainException("Email required");
            
        // Raise event
        Apply(new CustomerCreated { CustomerId = id, EncryptedEmail = email });
    }
    
    protected override void When(object @event)
    {
        switch (@event)
        {
            case CustomerCreated e:
                Handle(e);
                break;
        }
    }
    
    private void Handle(CustomerCreated e)
    {
        Id = e.CustomerId;
        EncryptedEmail = e.EncryptedEmail;
    }
}
```

#### **{Domain}.Infrastructure**
- **Projections** in `Projections/` (Marten event projections)
- **Repositories** for read models
- **External service implementations** (encryption, key storage)
- Marten/database configuration
- Outbox pattern implementation (future)

**Conventions:**
- Projections inherit `SingleStreamProjection<TReadModel, TId>` for stream-based projections
- Use `Create()` method for initial projection from first event
- Use `Apply()` methods for subsequent events
- Use `DeleteEvent<TEvent>()` for deletions
- Infrastructure services registered in `DependencyInjection.cs`

**Example Projection:**
```csharp
public class CustomerProjection : SingleStreamProjection<CustomerReadModel, Guid>
{
    private readonly IEncryptionService _encryption;
    
    public CustomerProjection(IEncryptionService encryption)
    {
        _encryption = encryption;
        DeleteEvent<CustomerForgotten>();
    }
    
    public async Task<CustomerReadModel> Create(CustomerCreated @event, IQuerySession session)
    {
        var key = await session.LoadAsync<KeyRecord>(@event.CustomerId);
        if (key == null) return null!;
        
        return new CustomerReadModel
        {
            Id = @event.CustomerId,
            Email = _encryption.Decrypt(@event.EncryptedEmail, key.Key)
        };
    }
    
    public CustomerReadModel Apply(CustomerAddressAdded @event, CustomerReadModel current)
    {
        current.Addresses.Add(...);
        return current;
    }
}
```

- **{Domain}.Tests**
- **Unit tests** in `DomainTests/` and `ApplicationTests/`
- **Integration tests** in `IntegrationTests/`
- **Projection tests** in `ProjectionsTests/`
- Use **xUnit**, **FluentAssertions**, **NSubstitute**
- **Testcontainers** for integration tests

**Conventions:**
- Integration tests use `WebApplicationFactory<Program>` with `IAsyncLifetime`
- Testcontainers for PostgreSQL: `new PostgreSqlBuilder("postgres:17-alpine")`
- Test class names: `{Feature}Tests` or `{Feature}IntegrationTests`
- Test method names: `{Method}_{Scenario}_{ExpectedResult}`
- Use `IClassFixture<TestWebApplicationFactory>` for integration tests
- FluentAssertions for readable assertions: `result.Should().Be(expected)`

## Technology Stack & Patterns

### Event Sourcing with Marten
- **Marten** is used as both event store and document database
- Events stored in streams identified by aggregate ID
- Use `session.Events.StartStream<TAggregate>(id, events)` for new aggregates
- Use `session.Events.Append(id, events)` for existing aggregates
- Rehydrate aggregates: `await session.Events.AggregateStreamAsync<TAggregate>(id)`
- Inline projections for read models

### CQRS with MediatR
- **Commands** mutate state, return scalars (Guid, void)
- **Queries** read data, return DTOs/ReadModels
- Use `ISender` interface in controllers (not `IMediator`)
- Handlers are registered automatically via assembly scanning

### Dependency Injection
- Each layer has `DependencyInjection.cs` with `AddXxx()` extension method
- Register services with appropriate lifetime (Scoped for Marten sessions)
- API layer calls: `services.AddApplication()`, `AddInfrastructure()`, `AddMartenConfiguration()`

### Marten Configuration
- Connection strings in `appsettings.json` under `ConnectionStrings:{Domain}Domain`
- Projections registered with lifecycle: `ProjectionLifecycle.Inline`
- Auto-create schema: `opts.AutoCreateSchemaObjects = JasperFx.AutoCreate.All`

### Exception Handling
- Custom domain exceptions in `{Domain}.Domain/Exceptions/`
- Global exception middleware maps exceptions to HTTP status codes
- Middleware returns **ProblemDetails** responses
- Example: `CustomerNotFoundException` → 404, `DuplicateAddressLabelException` → 409

### Security & Data Protection
- **Field-level encryption** for sensitive PII (names, emails, DOB)
- Encryption keys stored per customer in document store
- Events store encrypted data only
- Projections decrypt data using customer-specific keys
- Key shredding for GDPR "right to be forgotten"

## Coding Standards

### C# Language & .NET
- **Target Framework:** .NET 10 (`net10.0`)
- **C# Version:** 14.0
- **Nullable Reference Types:** Enabled
- **Implicit Usings:** Enabled
- Use **file-scoped namespaces**: `namespace CookieCommerce.CustomerDomain.Api;`
- Use **primary constructors** for classes with DI
- Use **records** for immutable DTOs, commands, queries
- Use **init-only properties** (`{ get; init; }`) for immutable data
- Use **null-forgiving operator** (`!`) carefully, prefer null checks

### Naming Conventions
- **Aggregates:** `{Name}Aggregate` (e.g., `CustomerAggregate`)
- **Commands:** `{Verb}{Entity}` (e.g., `CreateCustomer`, `AddCustomerAddress`)
- **Queries:** `Get{Entity}By{Criteria}` (e.g., `GetCustomerById`)
- **Handlers:** `{Command/Query}Handler` (e.g., `CreateCustomerHandler`)
- **Domain Events:** `{Entity}{PastTenseVerb}` (e.g., `CustomerCreated`, `CustomerAddressAdded`)
- **Integration Events:** `{Entity}{PastTenseVerb}IntegrationEvent` (future)
- **ReadModels:** `{Entity}ReadModel` (e.g., `CustomerReadModel`)
- **Projections:** `{Entity}Projection` (e.g., `CustomerProjection`)
- **Value Objects:** Descriptive noun (e.g., `Address`, `Gender`)
- **Exceptions:** `{Entity}{Condition}Exception` (e.g., `CustomerNotFoundException`)

### Code Organization
- One class per file
- File name matches type name
- Group using statements (System, Third-party, Project)
- Order class members: Fields → Properties → Constructors → Public Methods → Protected/Private Methods
- Keep methods short and focused (< 20 lines ideal)

### Comments & Documentation
- **XML comments** for public APIs (especially handlers, aggregates, commands)
- Avoid obvious comments (code should be self-documenting)
- Use `// TODO:` for technical debt
- Document **why**, not **what** (code shows what)

### Error Handling
- Throw domain exceptions with context for business rule violations
- Use domain-specific exceptions (not generic `Exception` or `InvalidOperationException`)
- Middleware handles exception-to-HTTP mapping
- Log errors at appropriate levels (Error for exceptions, Warning for business rules)

### Testing Best Practices
- **Arrange-Act-Assert** pattern
- **One assertion concept per test** (can have multiple assertion statements)
- Test names should be readable: `AddAddress_DuplicateLabel_ReturnsConflict`
- Integration tests should use real database (via Testcontainers)
- Mock only external dependencies (use NSubstitute)
- Use FluentAssertions for expressive assertions
- Tests should be isolated and repeatable

## MassTransit & Messaging (Future)
- Integration events published via MassTransit
- Use Outbox pattern for transactional messaging
- Message contracts in `{Domain}.Contracts` project
- RabbitMQ as message broker
- Configure in `CookieCommerce.CookieMassTransit`

## Aspire & Service Orchestration
- Aspire AppHost orchestrates all services
- Service discovery via Aspire
- PostgreSQL and RabbitMQ managed by Aspire
- Use `builder.AddServiceDefaults()` in each service
- Dashboard at `http://localhost:15888` (default)

## Common Pitfalls to Avoid

### ❌ Don't
- Don't use generic `Exception` - use custom domain exceptions
- Don't inject `IServiceCollection` into projections
- Don't mutate aggregates outside of event handlers
- Don't build service providers in projections (`_services.BuildServiceProvider()`)
- Don't return domain entities from queries - use ReadModels
- Don't use `IMediator` - use `ISender` instead
- Don't forget `CancellationToken` parameters in async methods
- Don't store decrypted PII in events - encrypt first
- Don't use `Task.Result` or `.Wait()` - use `await`

### ✅ Do
- Use custom exceptions with context properties
- Inject dependencies directly into projections
- Apply events via `Apply()` method in aggregates
- Use `IQuerySession` to load data in projections
- Return immutable DTOs/ReadModels from queries
- Pass `ISender` to controllers for CQRS
- Always accept and propagate `CancellationToken`
- Encrypt sensitive data before storing in events
- Use async/await for all I/O operations
- Use `Guid.CreateVersion7()` for new IDs

## Key Principles

1. **Domain-Driven Design:** Model the business domain accurately
2. **Event Sourcing:** All state changes captured as events
3. **CQRS:** Separate read and write models
4. **Vertical Slices:** Organize by feature, not layer
5. **Explicit is Better:** Clear, verbose code over clever code
6. **Test Behavior:** Test business rules, not implementation
7. **Security by Design:** Encrypt PII at rest and in events
8. **Eventual Consistency:** Projections may lag behind events

## Resources & References

- **Marten:** https://martendb.io/
- **MediatR:** https://github.com/jbogard/MediatR
- **MassTransit:** https://masstransit.io/
- **.NET Aspire:** https://learn.microsoft.com/en-us/dotnet/aspire/
- **DDD Patterns:** Domain-Driven Design by Eric Evans
- **Event Sourcing:** https://martinfowler.com/eaaDev/EventSourcing.html
