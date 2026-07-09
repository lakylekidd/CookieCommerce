# Implementation Plan: Customer Address Management

**Branch**: `master` | **Date**: 2026-07-07 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/002-customer-address-management/spec.md`

## Summary

Move authenticated customer address get/add/delete behavior from `FakeAccountService` mock state into CustomerDomain-owned event-sourced state. The implementation extends the existing CustomerDomain address aggregate/events/read-model/API slice to support the full customer-facing address shape, current-customer ownership, validation, default-address invariants, encrypted address PII handling, and Web service integration so account and checkout address flows use live CustomerDomain data without fake fallback.

## Technical Context

**Language/Version**: .NET 10 / C# 14, Blazor interactive server components.

**Primary Dependencies**: Marten event store/projections, MediatR command/query handlers, Aspire service discovery, Blazor typed services, System.Net.Http.Json, existing CustomerDomain encryption/key-store infrastructure.

**Storage**: CustomerDomain Marten/PostgreSQL event stream plus `CustomerReadModel` projection. Address recipient/contact/location fields are customer PII and must be encrypted before immutable event storage using the customer-specific key. Decrypted address data belongs only in removable read models and approved customer-facing responses.

**Testing**: xUnit, FluentAssertions, Moq where useful, Marten/test-host integration patterns in `CookieCommerce.CustomerDomain.Tests`, existing `CookieCommerce.Web.Tests` service tests. `dotnet test src` must discover and run feature tests.

**Target Platform**: Aspire-orchestrated CustomerDomain API and Blazor Web app.

**Project Type**: CustomerDomain bounded-context API/Application/Domain/Infrastructure/Test slice plus `src/CookieCommerce.Web` service/model integration and documentation update.

**Performance Goals**: Valid address additions and deletions are visible to the same customer in profile/checkout flows within 2 seconds; full view-add-delete validation can be completed by a tester in under 3 minutes.

**Constraints**: No customer-facing fake/static address fallback; customer-facing address operations must derive ownership from the authenticated customer; domain code remains persistence-ignorant; controllers stay thin; handlers are async and pass cancellation tokens; address PII is not logged or stored plaintext in immutable events; default-address invariant is enforced by the customer domain.

**Scale/Scope**: One address book per authenticated customer. Scope covers get addresses, add address, delete address, default-address read/reassignment as needed by existing UI, Web integration, tests, and documentation. Address update/edit, admin address management, broader `FakeAccountService` replacement for favorites/orders/reviews, and cross-domain address snapshots are out of scope.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- **Domain boundaries**: PASS — address-book state belongs to `CustomerDomain`; Web consumes it through typed services. No new bounded context is introduced.
- **Backend standards**: PASS — plan uses thin API endpoints, MediatR commands/queries, explicit DTO mappings, domain-specific exceptions, and persistence-ignorant aggregate/value-object logic.
- **Event sourcing + CQRS**: PASS — add/delete/default behavior is modeled as Customer aggregate state changes persisted as events and projected into read models.
- **Testing discipline**: PASS — plan requires aggregate invariant tests, application/handler tests, projection/API integration tests, and Web service tests; all must run through `dotnet test src`.
- **Async integration**: PASS — no new cross-domain business decision is introduced. Future cross-domain address needs must use integration events/local snapshots, not synchronous runtime CustomerDomain calls.
- **Privacy + operations**: PASS — address PII is encrypted before event storage, decrypted only into removable projections/responses, erased through existing customer-key crypto-shredding, and excluded from logs/problem details.
- **Aspire + containerization**: PASS — CustomerDomain API and Web already have runtime surfaces and Dockerfiles; no new service/container is needed. Existing AppHost composition remains valid.
- **Frontend standards**: PASS — Web components continue to use services under `src/CookieCommerce.Web/Services`; backend calls are not made directly from components; existing responsive/account/checkout UI behavior is preserved.
- **Documentation impact**: PASS — changing customer-facing Web address service behavior requires `Documentation/backend-documentation.md` updates.

## Project Structure

### Documentation (this feature)

```text
specs/002-customer-address-management/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── customer-address-api.md
└── tasks.md             # Created later by /speckit.tasks
```

### Source Code (repository root)

```text
src/
├── CookieCommerce.CustomerDomain.Api/
│   ├── Controllers/CustomerController.cs
│   └── Models/Customers/
├── CookieCommerce.CustomerDomain.Application/
│   └── Customers/
│       ├── Commands/
│       ├── Handlers/
│       ├── Profile/
│       ├── Queries/
│       └── ReadModels/
├── CookieCommerce.CustomerDomain.Domain/
│   ├── Aggregates/CustomerAggregate.cs
│   ├── DomainEvents/
│   ├── Exceptions/
│   └── ValueObjects/
├── CookieCommerce.CustomerDomain.Infrastructure/
│   └── Projections/CustomerProjection.cs
├── CookieCommerce.CustomerDomain.Tests/
│   ├── ApplicationTests/
│   ├── DomainTests/
│   ├── IntegrationTests/
│   └── ProjectionsTests/
├── CookieCommerce.Web/
│   ├── Components/Account/
│   ├── Components/Storefront/Checkout/
│   ├── Models/Addresses/
│   └── Services/
├── CookieCommerce.Web.Tests/
│   └── Services/
└── Documentation/backend-documentation.md
```

**Structure Decision**: Implement the source of truth in the existing CustomerDomain five-project layout. Add or adapt current-customer address API contracts so Web address operations do not trust caller-supplied customer ids. Update existing Web account/profile/checkout address service behavior to delegate get/add/delete/default-address reads to CustomerDomain while leaving unrelated fake account features out of scope. Update backend documentation for the changed service behavior.

## Phase 0: Research Summary

See [research.md](./research.md). Key decisions:

- CustomerDomain owns address-book state and contracts.
- Address changes are Customer aggregate events, not mutable Web/mock state.
- Address PII must be encrypted before immutable event storage and decrypted only for approved projections/responses.
- Customer-facing contracts use authenticated current-customer routes such as `/api/customer/me/addresses`.
- Default-address invariants are enforced inside the customer domain.
- Only address-related `FakeAccountService` behavior is replaced; favorites/orders/reviews remain separate concerns.

## Phase 1: Design Summary

See [data-model.md](./data-model.md), [contracts/customer-address-api.md](./contracts/customer-address-api.md), and [quickstart.md](./quickstart.md).

Design artifacts define:

- Customer, Customer Address, Address Book, Customer Address Read Model, and Web Address DTO shapes and validation rules.
- Address lifecycle states for add, active/default, and delete.
- Customer-facing address API contracts for get, add, delete, and default-address retrieval.
- Web service expectations for mapping CustomerDomain responses to the existing UI address DTO and refreshing state after writes.
- Validation scenarios for empty state, add, invalid input, delete, authorization/ownership, fake fallback prevention, privacy/erasure, and documentation.

## Post-Design Constitution Check

- **Domain boundaries**: PASS — data model and contracts keep address ownership in CustomerDomain; Web remains a consumer.
- **Backend standards**: PASS — contracts require DTO/request/response shapes, user-safe errors, and current-customer ownership; implementation can remain thin-controller/focused-handler.
- **Event sourcing + CQRS**: PASS — data model specifies aggregate events and read-model projection behavior for address reads.
- **Testing discipline**: PASS — quickstart requires domain/application/projection/API/Web tests through `dotnet test src`.
- **Async integration**: PASS — no cross-domain runtime dependency is introduced.
- **Privacy + operations**: PASS — data model and contracts require encrypted event payloads, removable decrypted projections, crypto-shredding preservation, and no PII in logs/errors.
- **Aspire + containerization**: PASS — no new container; existing CustomerDomain/Web composition remains the validation target.
- **Frontend standards**: PASS — Web access remains service-mediated and preserves existing account/checkout components.
- **Documentation impact**: PASS — documentation update is included for changed Web service/address contract behavior.

## Complexity Tracking

No constitution violations requiring justification.

## Agent Context Update

No agent context update script is present under `.specify/scripts` in this repository, so this step was checked and skipped without changes.
