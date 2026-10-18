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

### Frontend startup and authentication

The routed Blazor UI runs with interactive server rendering and prerendering disabled. The web client restores
authentication from `ProtectedLocalStorage`, which cannot be accessed during server prerendering. Disabling
prerendering ensures the launch gate evaluates the current authentication state before choosing between the
requested page and the coming-soon page, avoiding an incorrect coming-soon flash on refresh.

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

## Customer Account Lifecycle

Backoffice customer creation spans the Customer and Authentication Management bounded contexts:

1. The Web backoffice validates and creates the encrypted customer profile through Customer Domain.
2. Web calls the administrator-only Authentication Management provisioning endpoint with the returned customer id.
3. Authentication Management idempotently creates a linked Keycloak identity with the `User` realm role, the `UPDATE_PASSWORD` required action, and no credential.
4. If requested, Authentication Management creates a cryptographically random, 30-minute, single-use password-setup token. Only its SHA-256 hash is stored in Marten.
5. Web renders the published `account-invitation` template and queues it through the existing outbound-email pipeline. The link targets `/auth/password/setup/{token}`.
6. The public password-setup endpoint validates and consumes the token under a PostgreSQL advisory lock, writes the permanent Keycloak password, clears the required action, and uses the regular login handler to return an authenticated session.

Resending an invitation revokes all earlier unused invitations for that identity. Customer detail remains the recovery point when provisioning or initial delivery was skipped or failed.

Keycloak remains the source of truth for realm roles. Authentication Management owns role mutation and enforces both invariants before removing `Admin`: the acting identity cannot be the target, and more than one administrator must currently exist. These rules are enforced server-side; disabled UI controls are only a convenience.

## Backoffice template authoring

All rendered HTML email bodies pass through the shared email-safe branded shell. This keeps source previews and queued customer messages consistent with the shop's colours, logo URL, Fredoka/Nunito font fallbacks, responsive spacing, and inline action-button styling. Branding is derived from the persisted invoice company name/logo and configured public store URL, with safe defaults when those settings are unavailable.

The template editor derives a sample-value input from TemplateService.Allowed(template.Key) for every supported placeholder. Preview rendering consumes those values directly, so administrators can inspect realistic content without changing the saved template or introducing a second template format.

PDF invoice templates remain structured rather than arbitrary HTML: heading, footer and accent colour are persisted on MessageTemplate and rendered by InvoiceRenderer. The invoice preview loads the saved company/logo settings, and InvoiceRenderer applies the shop accent/header palette, embedded font, logo, and branded totals/header/footer styling to the generated PDF.

`MessageTemplate.HtmlBody` remains the canonical, versioned source for email markup; the backoffice does not persist a separate WYSIWYG document model. The Templates settings panel provides a source/preview switch, with previews rendered inside a sandboxed iframe. Its image helper only creates markup for public HTTPS URLs and uses escaped alternative text plus inline, email-compatible left, centre, or right alignment. Image files are not accepted or stored by the template feature, so no new media-upload API or persistence boundary is introduced. Existing template validation remains responsible for rejecting executable HTML and unsupported placeholders before drafts are saved or published.

## Logging

All production code must include appropriate structured logging for important application behavior.

When adding or modifying functionality:

- Log meaningful lifecycle events, important state transitions, external calls, failures, and unexpected conditions.
- Prefer structured logging with named properties instead of interpolated strings.
- Use appropriate log levels:
  - `Trace` / `Debug` — diagnostic information useful during development.
  - `Information` — important normal application events.
  - `Warning` — unexpected situations that are recoverable.
  - `Error` — failed operations requiring investigation.
  - `Critical` — failures that may prevent the application or service from operating.
- Do not log passwords, tokens, API keys, connection strings, authentication headers, personal data, or other secrets.
- Include useful context where available, such as request IDs, correlation IDs, entity IDs, operation names, service names, and external dependency names.
- Avoid noisy logging inside tight loops or high-frequency operations unless using `Debug` or `Trace`.
- Do not log the same exception multiple times at different layers unless each log adds meaningful context.
- When catching an exception, either handle it meaningfully or log it with relevant context before propagating it.
- Prefer logging at service boundaries and important business operations rather than logging every method invocation.
- Existing project logging conventions and abstractions must be reused instead of introducing a new logging framework.

Example:

```csharp
logger.LogInformation(
    "Order {OrderId} created for customer {CustomerId}",
    order.Id,
    customer.Id);

logger.LogError(
    exception,
    "Failed to process payment for order {OrderId}",
    order.Id);
```

When implementing a new feature, consider logging part of the implementation rather than an optional follow-up.

### Outbound email observability

Outbound SMTP delivery is an external-service boundary owned by the Web service. It logs successful acceptance and failures once at that boundary. Email logs include the delivery kind, SMTP host, port, connection security, and a sanitized failure reason; they must never include recipient addresses, message content, SMTP credentials, action tokens, or raw SMTP server responses.

All services use the shared `OperationalHttpLoggingHandler` for outbound HTTP dependencies. It logs only the dependency host, method, status code, and safe failure type; URLs, query strings, headers, request bodies, and response bodies are intentionally excluded. API exception middleware logs one safe, structured event per failed request using trace ID, method, status code, and exception type. Expected client rejections are warnings; unexpected server failures are errors.
