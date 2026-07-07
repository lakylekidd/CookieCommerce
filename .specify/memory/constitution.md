<!--
Sync Impact Report
Version change: 1.3.0 → 1.4.0
Modified principles:
- III. Centralized Configuration via Aspire expanded with containerization standards
Added sections:
- None
Removed sections:
- None
Templates requiring updates:
- ✅ .specify/templates/plan-template.md
- ✅ .specify/templates/spec-template.md
- ✅ .specify/templates/tasks-template.md
- ✅ .specify/templates/commands/*.md (directory absent; no updates required)
Runtime guidance requiring updates:
- ✅ README.md (already aligned; no changes required)
- ✅ CLAUDE.md (already aligned; no changes required)
Follow-up TODOs:
- None
-->
# CookieCommerce Constitution

## Core Principles

### I. Code Quality & Consistency (Non-Negotiable)
- File-scoped namespaces must be used in every `.cs` file; block-scoped namespaces are forbidden.
- Naming conventions MUST be enforced:
  - Private fields: `_camelCase`
  - Constants: `PascalCase`
  - Interfaces: `I` prefix
  - Async methods: `Async` suffix
- `dotnet format` MUST pass with zero violations before any code is merged.
- API layer messages MUST use `*Request` / `*Response` suffixes.
- Use-case layer messages MUST use `*Command` / `*Query` suffixes.
- Interfaces and their implementations must not reside in the same project (Except in the Web Project)!

Backend code MUST follow these additional standards:
- Controllers MUST stay thin: accept requests, delegate to MediatR/application
  handlers, map responses, and return HTTP results. Business rules MUST NOT live
  in controllers, middleware, or DTO mapping code.
- Application handlers MUST represent one command or query use case. They MUST
  use async APIs, accept and pass through `CancellationToken`, and avoid
  synchronous blocking (`.Result`, `.Wait()`, or sync-over-async).
- Domain code MUST be persistence-ignorant. Aggregates, value objects, domain
  events, and domain exceptions MUST NOT reference Marten, MassTransit,
  ASP.NET Core, or infrastructure services.
- Infrastructure code MUST contain persistence, projections, outbox,
  integration-event consumers, external service clients, and dependency
  injection wiring. Infrastructure details MUST NOT leak into Domain contracts.
- Public API DTOs MUST be immutable records or simple DTO classes with explicit
  mapping extensions. API contracts MUST NOT expose aggregate types, Marten
  documents, or infrastructure models directly.
- Validation MUST happen at the boundary or use-case entry point before domain
  state changes are persisted. Domain invariants MUST still be enforced inside
  aggregates or value objects.
- Domain exceptions MUST be specific and mapped to ProblemDetails by the owning
  API middleware. Generic `Exception` throws are forbidden for expected business
  failures.
- Dependency injection registrations MUST live in the owning API or
  Infrastructure project, preferably behind explicit extension methods.
  Application and Domain projects MUST NOT depend on service-provider lookups.
- Date/time, IDs, and external side effects MUST be injectable or supplied by
  the application boundary when they affect business behavior. New aggregate IDs
  SHOULD use `Guid.CreateVersion7()` unless an existing contract requires a
  different format.
- Backend methods SHOULD remain short and focused. Deep nesting, large handlers,
  and broad utility classes MUST be refactored into named domain concepts,
  private helpers, or focused services with clear ownership.

Frontend code in `src/CookieCommerce.Web` MUST follow these standards:
- Razor components MUST be small, purpose-specific, and named in `PascalCase`.
  Components that represent pages MUST live under `Components/Pages`; reusable
  UI MUST live under `Components/Shared`, `Components/Layout`, or a feature
  folder such as `Components/Account`, `Components/Auth`, or
  `Components/Backoffice`.
- Component parameters MUST use `[Parameter]` with `PascalCase` names and MUST
  mark required inputs with `[EditorRequired]` when the component cannot render
  correctly without them.
- Components MUST NOT call backend APIs directly. Backend access MUST go through
  typed services under `src/CookieCommerce.Web/Services`, and UI-facing DTOs
  MUST live under `src/CookieCommerce.Web/Models`.
- Component state MUST stay local unless it is shared by multiple components or
  routes. Shared UI state MUST be encapsulated in a scoped service with clear
  ownership and lifecycle.
- Styling MUST prefer scoped `.razor.css` files for component-specific styles.
  Global styles under `Styles/` or `wwwroot/` MUST be limited to design tokens,
  layout primitives, resets, or intentionally shared utilities.
- JavaScript interop MUST be isolated to the smallest component or service that
  needs it. JavaScript files MUST live beside the owning component when they are
  component-specific, and interop calls MUST be async and disposable-safe.
- UI changes MUST preserve accessibility: semantic HTML, labels for inputs,
  keyboard-reachable controls, visible focus states, and meaningful alt text or
  `aria-*` attributes where needed.
- Customer-facing flows MUST be responsive across mobile and desktop viewport
  widths and MUST not depend on hover-only interactions.
- Forms MUST provide validation feedback without exposing sensitive data and
  MUST avoid logging PII in browser console output or telemetry.

### II. Test-Driver Quality Assurance
- xUnit MUST be the sole test framework for all test projects. 
- Tests MUST follow the arrange-act-assert pattern with white space separation (no inline comment labeling sections).
- Test class naming: `ClassName_MethodName_Tests.cs`.
- Test method naming: `DoesSomething_GivenSomeCondition` - Reading the fully qualified name MUST form a natural language sentence. 
- `[Fact]` For single-case tests; `[Theory]` with `[InlineData]` for parametrized tests.
- FluentAssertions MUST be used.
- Moq MUST be used.
- Unit and integration tests MUST be separated into distinct projects or folders. 
- Only code with business logic under the teams' control MUST be tested; framework code MUST NOT be tested. 
- Do NOT add any tests for behavioral orchestration or bootstraping projects.

### III. Centralized Configuration via Aspire
- All service configuration and service discovery must be managed by the Aspire AppHost project.
- `IConfiguration` and `IOptions<T>` MUST be used for configuration access; strongly-typed setting classes must be created for complex configurations.
- Sensitive data MUST NEVER be hard coded in source; User secrets in the AppHost must be used for development secrets.
- Every bounded context with a runtime surface MUST be fully containerized. Its runnable host project, normally `src/CookieCommerce.{BoundedContext}.Api/` or a worker host, MUST include a `Dockerfile` that can build and run the service without relying on Visual Studio or local machine state.
- The Aspire AppHost MUST register every bounded-context container or project service and its infrastructure dependencies so the complete system can be started with `dotnet run --project src/CookieCommerce.AppHost`.
- Dockerfiles MUST use configuration supplied by environment variables, Aspire service discovery, appsettings, or user secrets; Docker images MUST NOT bake in secrets or developer-specific paths.
- Container changes MUST be verified by building the affected image and by confirming the service can participate in the Aspire composition.

### IV. Domain Boundaries Are Explicit
CookieCommerce MUST be organized around bounded contexts and vertical slices. Each
bounded context MUST keep API, Application, Domain, Infrastructure, and Tests
concerns in its own project structure. Aggregates MUST enforce invariants,
domain events MUST represent business state changes, value objects MUST remain
immutable, and domain exceptions MUST express business rule violations.

Rationale: clear domain ownership keeps changes local, makes business language
visible in code, and prevents infrastructure concerns from weakening domain
rules.

### V. Event-Sourced State and CQRS
Durable business state MUST be changed through event-sourced aggregates and
persisted as domain events through Marten. Domain events used by Marten MUST be
mutable classes. Commands MUST mutate state through focused handlers, and
queries MUST read denormalized read models through projections or query models.
Projections MUST be updated when new events affect read behavior.

Rationale: event sourcing provides an auditable business history, while CQRS
keeps write-side invariants separate from read-side optimization.

### VII. Testable Slices Are Mandatory
Every behavioral change MUST include tests at the lowest useful level and at the
integration boundary when persistence, projections, messaging, APIs, or
cross-domain behavior changes. Aggregate invariants MUST have unit tests.
Marten projections, API contracts, and message consumers MUST have integration
or contract tests. Tests MUST be runnable with `dotnet test src`, and feature
work MUST document any intentionally deferred tests before merge.

Rationale: independently testable slices make event-sourced behavior safe to
change and keep regressions visible across asynchronous boundaries.

### VIII. Asynchronous Cross-Domain Integration
Bounded contexts MUST NOT make synchronous runtime HTTP calls to other bounded
contexts for business decisions. Cross-domain data MUST flow through integration
events published with the transactional outbox and consumed into local snapshot
or read models. Message consumers MUST be idempotent or otherwise safe to retry.

Rationale: asynchronous integration preserves service autonomy, supports
resilience, and makes eventual consistency explicit rather than accidental.

### IX. Privacy, Observability, and Operational Safety
Sensitive PII MUST be encrypted before it is stored in domain events and MUST be
decrypted only in projections or application boundaries that need clear text.
Customer erasure MUST preserve GDPR crypto-shredding semantics by removing the
customer-specific key. Services MUST use structured logging, health checks, and
OpenTelemetry defaults provided by the platform. Failures exposed through APIs
MUST use ProblemDetails and domain-specific exception mapping.

Rationale: the event store is a permanent business log, so privacy and
operability must be designed into every feature rather than added later.

## Project Structure

All production code MUST live under `src/`. New bounded-context work MUST use
this project pattern unless the plan documents an approved exception:

```text
src/CookieCommerce.{BoundedContext}.Api/            # Controllers, DTOs, middleware, Dockerfile
src/CookieCommerce.{BoundedContext}.Application/    # Commands, queries, handlers, use cases
src/CookieCommerce.{BoundedContext}.Domain/         # Aggregates, events, value objects, exceptions
src/CookieCommerce.{BoundedContext}.Infrastructure/ # Marten, projections, consumers, outbox, DI
src/CookieCommerce.{BoundedContext}.Tests/          # Unit, integration, contract, projection tests
```

Current bounded contexts include AuthenticationManagement, CustomerDomain,
ProductDomain, OrderDomain, and CatalogDomain. When a context is incomplete, new
work SHOULD extend it toward the standard five-project layout instead of adding
new cross-cutting folders or mixing layers. Each bounded context MUST expose a
runnable host project with a Dockerfile before it is considered production-ready.

Shared platform projects have explicit ownership:

```text
src/CookieCommerce.DomainBuildingBlocks/ # Aggregate base types and domain abstractions
src/CookieCommerce.CookieMassTransit/    # Shared MassTransit/RabbitMQ wiring
src/CookieCommerce.ServiceDefaults/      # Aspire defaults, health checks, telemetry
src/CookieCommerce.AppHost/              # Aspire orchestration and local infrastructure
src/CookieCommerce.Web/                  # Blazor UI, UI models, frontend service clients, Dockerfile
src/CookieCommerce.Core/                 # Shared primitives only when DomainBuildingBlocks is unsuitable
```

- Infrastructure-as-code MUST live under `infra/`. 
- Product and developer documentation MUST live in `Documentation/`, repository root Markdown files, or
feature-specific `specs/` folders. 
- Generated build output (`bin/`, `obj/`, `node_modules/`, `.vs/`) MUST NOT be used as source locations.

Within a bounded context, dependencies MUST point inward: API and Infrastructure
may depend on Application and Domain; Application may depend on Domain;
Domain MUST NOT depend on API, Infrastructure, UI, or another bounded context.
Cross-domain data dependencies MUST be represented by integration events and
local read models in the consuming context.

## Technology and Architecture Constraints

CookieCommerce is a .NET 10, C# 14, Blazor, Marten, MediatR, MassTransit,
RabbitMQ, PostgreSQL, and Aspire system. New production code MUST fit this stack
unless a plan documents the exception, the rejected simpler alternative, and the
migration impact.

Source code MUST follow the established solution layout under `src/`. Shared
abstractions belong in the existing shared projects only when at least two
bounded contexts need them. Feature code SHOULD remain in vertical slices rather
than generic service layers. Public API additions MUST use DTOs and mapping
extensions in the API layer, and POST endpoints MUST return an appropriate
creation response when they create resources.

Marten projections MUST be registered with the lifecycle required by the owning
domain. Integration events MUST be published through the outbox so event writes
and outgoing messages are transactionally coupled. Runtime configuration MUST be
provided through Aspire, appsettings, environment variables, or user secrets;
secrets MUST NOT be committed or baked into container images.

## Development Workflow and Quality Gates

Feature work MUST start from a specification with independently testable user
stories, measurable success criteria, assumptions, and edge cases. Plans MUST
run a constitution check before research and again after design. Any violation
MUST be recorded with a justification and the simpler alternative that was
rejected.

Implementation tasks MUST preserve MVP-first delivery by grouping work by user
story and priority. Tests for a user story MUST be written before or alongside
the implementation and MUST fail for the intended reason when test-first work is
practical. Changes to files matching `src/CookieCommerce.Web/Services/**/*.cs`
that add, remove, or alter an interface MUST update
`Documentation/backend-documentation.md` in the same change.

Before merge, contributors MUST run the smallest relevant test command and MUST
run `dotnet test src` for broad domain, persistence, messaging, or API changes.
Builds MUST pass with `dotnet build src`. Documentation and quickstart material
MUST be updated when behavior, configuration, or developer workflows change.

## Governance

This constitution supersedes conflicting repository conventions and templates.
Repository guidance files, feature plans, specifications, and task lists MUST be
kept consistent with these principles.

Amendments MUST include: the proposed text, a summary of affected principles or
sections, the version bump rationale, and updates to dependent templates or
runtime guidance. Material principle additions or expansions require a MINOR
version bump. Backward-incompatible governance changes, removed principles, or
principle redefinitions require a MAJOR version bump. Clarifications and
non-semantic wording changes require a PATCH version bump.

Every feature plan MUST include a constitution compliance review. Reviewers MUST
block changes that violate mandatory principles unless the plan documents an
approved exception and migration path. This constitution MUST be revisited when
major architectural decisions are accepted or when the platform stack changes.

**Version**: 1.4.0 | **Ratified**: 2026-07-06 | **Last Amended**: 2026-07-06
