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

#### **[Domain].Api**
- ASP.NET Core Web API with **Controllers** (prefer controllers over Minimal APIs)
- Request/Response models (DTOs) in `Models/{Feature}/` folders
- Request mapping extensions in `Models/{Feature}/{Feature}Mapper.cs`
- API-specific middleware
- OpenAPI/Swagger configuration
- Endpoint routing: `[Route("api/[controller]")]`
- Use **ProblemDetails** for error responses
- Create and update (whenever necessary) the Readme, at the domain level, documenting the API endpoints and their expected request/response formats.

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

#### **[Domain].Infrastructure**
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

- **[Domain].Tests**
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