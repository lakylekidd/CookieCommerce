# Implementation Plan: Update Customer Profile

**Branch**: `feature/001-update-customer-profile` | **Date**: 2026-07-06 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/001-update-customer-profile/spec.md`

## Summary

Enable authenticated customers to view and update first name, last name, email address, gender, and date of birth from the dashboard profile flow, with gender limited to `Male`, `Female`, and `Prefer not to disclose`. The implementation extends the existing CustomerDomain vertical slice with profile update, pending email change, verification token creation/storage/validation, projection, and API contracts. US1 stops at editable profile data, pending email request/resend, and the old verified email remaining the active profile/login email; US3 owns verification confirmation, verified-email integration, and AuthenticationManagement login credential promotion. Email delivery is handled by an external dependency rather than a new delivery subsystem through an explicit handoff contract, concrete CustomerDomain sender/adapter, DI registration, safe payload, and retry/idempotency metadata. The work preserves GDPR crypto-shredding behavior from `Documentation/GDPR-Compliance.md`, requires CustomerDomain to publish an outbox-backed `CustomerForgotten`/`CustomerDeleted` integration event and requires AuthenticationManagement to consume it idempotently before deleted customers receive login not found responses and deleted-customer email addresses become reusable for new registration, validates profile fields at the use-case boundary, inventories and immediately refreshes all website surfaces showing customer name/email/gender/date of birth after a successful save without manual page reload in the same save response/render cycle, replaces `src/CookieCommerce.Web` mock profile data/services with typed live CustomerDomain service calls, and validates that affected services still participate in the Aspire composition during polish.

## Technical Context

**Language/Version**: .NET 10 / C# 14, Blazor interactive server components.

**Primary Dependencies**: Marten event store/projections, MediatR command/query handlers, MassTransit/RabbitMQ integration-event publishing through existing abstractions, Aspire service discovery, Blazor typed services, System.Net.Http.Json, existing AuthenticationManagement/Keycloak integration.

**Storage**: CustomerDomain Marten/PostgreSQL event stream plus `CustomerReadModel` projection. Full name, verified email, pending email, gender, and date of birth are stored in encrypted CustomerDomain profile events using the customer-specific key so projections can rebuild the complete profile while GDPR erasure still crypto-shreds those values by deleting the key; the `CustomerReadModel` stores decrypted gender/date of birth only as removable projection data and deletes them during erasure. Verification token references are stored as secret operational data only in hashed/reference form; raw verification tokens must not be logged or stored in immutable events. Gender is limited to `Male`, `Female`, and `Prefer not to disclose`. AuthenticationManagement stores/updates the verified login email only after the CustomerDomain verification event is consumed. Customer deletion in CustomerDomain crypto-shreds CustomerDomain data, writes PII-free deletion records, and publishes an outbox-backed `CustomerForgotten`/`CustomerDeleted` integration event; AuthenticationManagement owns all login identity removal/disablement and login-email uniqueness release after consuming that event idempotently.

**Testing**: xUnit, FluentAssertions, Moq, Marten/test-host integration coverage through existing `CookieCommerce.CustomerDomain.Tests` patterns; new runnable xUnit projects `src/CookieCommerce.Web.Tests` and `src/CookieCommerce.AuthenticationManagement.Tests` must be created, referenced by `src/CookieCommerce.slnx`, and used for all Web profile service/component tests and AuthenticationManagement consumer/integration tests. `dotnet test src` must discover and run these tests; orphan test files outside runnable test projects are not allowed.

**Target Platform**: Aspire-orchestrated CustomerDomain API, AuthenticationManagement API/Keycloak integration, RabbitMQ, and Blazor Web app.

**Project Type**: CustomerDomain bounded-context API/Application/Domain/Infrastructure/Test slice plus AuthenticationManagement integration-event consumer and `src/CookieCommerce.Web` UI models/services/components.

**Performance Goals**: Profile load and save should complete quickly enough for the customer to finish a valid update in under 2 minutes; after a successful save, all website surfaces that display customer name, email address, gender, or date of birth must refresh to the saved live profile data immediately, without manual page reload and within the same save response/render cycle, with no sync-pending state or stale profile display. Email verification delivery may be asynchronous through the external delivery dependency, but pending email state and verification token validation must be reflected clearly.

**Constraints**: No mock-data fallback in any customer-facing website surface that shows customer profile details; no plaintext PII or raw verification tokens in events or logs; pending email must not replace the CustomerDomain profile email before verification, and CustomerDomain verification alone must not replace the AuthenticationManagement login email; CustomerDomain owns verification token creation/storage/validation while email delivery is delegated to an external dependency through an explicit handoff payload, concrete sender/adapter, DI registration, expiry, recipient pending email, customer id, token/reference, verification URL or route data, purpose, correlation id, and idempotency/retry metadata; deleted customers must receive not found on login and their former email must be reusable for registration after AuthenticationManagement processes the forgotten/deleted-customer event; API failures must surface user-safe errors; cross-domain email credential coordination must not require synchronous runtime business decisions between bounded contexts.

**Scale/Scope**: One customer profile per authenticated customer, with at most one active pending email change. Feature scope covers personal details displayed anywhere on the customer-facing website, dashboard editing, verification token creation/storage/validation, external email delivery handoff, and US3 email verification/promotion. Password changes, admin profile editing, a new email delivery subsystem, and broader account/order/favorites mock service replacement remain outside this feature unless directly needed to prevent stale/mock profile details on surfaces that show customer name, email, gender, or date of birth.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- **Domain boundaries**: PASS — Customer profile state and pending email lifecycle are owned by `CustomerDomain`; Web only consumes CustomerDomain through typed services. AuthenticationManagement impact is limited to consuming a verified-email integration event and updating sign-in identity.
- **Backend standards**: PASS — API endpoints stay thin and delegate to MediatR commands/queries; validation occurs at request/use-case boundary; authenticated `/me` dashboard profile endpoints avoid trusting caller-supplied customer ids; DTOs are distinct from aggregate/projection types; domain remains persistence-ignorant.
- **Event sourcing + CQRS**: PASS — profile changes, pending email requests, and verified email promotions are modeled as Customer aggregate state changes persisted as events and reflected through read-model projection updates.
- **Testing discipline**: PASS — plan requires aggregate/application tests for validation/state transitions, projection tests for decrypted read model behavior, API integration tests for contracts/security, AuthenticationManagement consumer tests, and Web service/UI tests for mock removal and pending email state. It also requires creation of runnable `CookieCommerce.Web.Tests` and `CookieCommerce.AuthenticationManagement.Tests` xUnit projects, solution references in `src/CookieCommerce.slnx`, and restore-before-`--no-restore` `dotnet test src --list-tests` discovery before story work adds tests.
- **Async integration**: PASS — AuthenticationManagement receives verified-email changes through an outbox-backed integration event/local handling; no synchronous cross-context business decision is introduced.
- **Privacy + operations**: PASS — encrypted event fields, crypto-shredding, eventual deleted-login not-found behavior, deleted-email registration reuse after AuthenticationManagement consumption, no PII in deletion events/logs, redacted email-token logs, explicit operational log redaction tests for profile update/email verification/deletion, ProblemDetails-style API failures, and service-default observability remain required.
- **Aspire + containerization**: PASS — CustomerDomain API, AuthenticationManagement API, RabbitMQ, and Web already participate in Aspire; no new runtime service is introduced. Web must reference CustomerDomain API in Aspire service discovery.
- **Frontend standards**: PASS — dashboard components remain under `Components/Account`; API access goes through concrete typed services under `src/CookieCommerce.Web/Services` without adding new same-project service interfaces; UI-facing DTOs stay under `src/CookieCommerce.Web/Models`; accessibility/responsive validation and pending-state feedback are included.
- **Documentation impact**: PASS — changes to Web profile service behavior or any existing service contract require `Documentation/backend-documentation.md` updates; this plan avoids adding a new Web service interface in the same project as its implementation.

## Project Structure

### Documentation (this feature)

```text
specs/001-update-customer-profile/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── profile-display-inventory.md
├── contracts/
│   └── customer-profile-api.md
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
│       ├── Integration/
│       ├── Queries/
│       └── ReadModels/
├── CookieCommerce.CustomerDomain.Domain/
│   ├── Aggregates/CustomerAggregate.cs
│   ├── DomainEvents/
│   ├── Exceptions/
│   └── ValueObjects/
├── CookieCommerce.CustomerDomain.Infrastructure/
│   ├── Projections/CustomerProjection.cs
│   └── Security/
├── CookieCommerce.CustomerDomain.Tests/
│   ├── ApplicationTests/
│   ├── DomainTests/
│   ├── IntegrationTests/
│   └── ProjectionsTests/
├── CookieCommerce.AuthenticationManagement.Application/
│   └── Auth/ or Integration/       # verified-email consumer/use case, following existing layout
├── CookieCommerce.AuthenticationManagement.Infrastructure/
│   └── Keycloak/ or Consumers/      # login email update implementation, following existing layout
├── CookieCommerce.AuthenticationManagement.Tests/
│   ├── ConsumerTests/
│   └── IntegrationTests/            # runnable xUnit project referenced by src/CookieCommerce.slnx
├── CookieCommerce.Web/
│   ├── Components/Account/
│   ├── Models/Account/
│   ├── Services/
│   ├── Program.cs
│   └── appsettings*.json
├── CookieCommerce.Web.Tests/
│   ├── Components/Account/
│   └── Services/                    # runnable xUnit project referenced by src/CookieCommerce.slnx
├── CookieCommerce.AppHost/AppHost.cs
├── CookieCommerce.slnx              # references all runnable source and test projects
└── Documentation/backend-documentation.md
```

**Structure Decision**: Implement the write/read contract in the existing CustomerDomain five-project layout and expose authenticated dashboard profile operations through `/api/customer/me`-style endpoints so the API derives profile ownership from the signed-in customer rather than trusting route-supplied customer ids. In `CookieCommerce.Web`, use a concrete typed profile service registered with DI and remove customer-facing profile use of `FakeAccountService`/`StaticProfileList`; do not introduce a new Web service interface in the same project as its implementation. Existing `IAccountService` usage may be adapted only if it does not expand same-project interface/implementation coupling; any existing service contract change must update backend documentation. AuthenticationManagement changes should be limited to consuming verified-email events for login credential promotion and consuming `CustomerForgotten`/`CustomerDeleted` events for login identity removal/disablement and email reuse. CustomerDomain must not synchronously delete, disable, or otherwise modify AuthenticationManagement login identities. Email uniqueness checking across verified and pending emails must be available in Foundation or US1 and must run before creating any pending email change; US2 may only refine validation messages/tests for duplicate-email failures.

## Phase 0: Research Summary

See [research.md](./research.md). Key decisions:

- Use CustomerDomain as owner of profile update and pending email state.
- Model requested email changes as pending until verification succeeds.
- Publish a verified-email integration event for AuthenticationManagement after successful verification.
- Add/complete actual authenticated CustomerDomain `/api/customer/me` profile endpoints for US1 pending-email request/resend, with verification confirmation and AuthenticationManagement promotion owned by US3.
- Preserve GDPR crypto-shredding: event-sourced full name/email/gender/date of birth encrypted with customer-specific keys; decrypted read-model demographic data is projection-only and removed during deletion; customer deletion publishes an outbox-backed `CustomerForgotten`/`CustomerDeleted` integration event with no PII; AuthenticationManagement consumes it idempotently to remove/disable the login identity and release login-email uniqueness so login is not found and registration can reuse the email after processing.
- Use concrete typed Blazor services over Aspire-named `HttpClient` and remove customer-facing mock fallback for profile load/save and every inventoried website surface that shows customer profile details.
- Create runnable `src/CookieCommerce.Web.Tests` and `src/CookieCommerce.AuthenticationManagement.Tests` xUnit projects during setup, reference both from `src/CookieCommerce.slnx`, run restore before `dotnet test src --no-restore --list-tests` discovery, and forbid orphan tests outside runnable projects.
- Use the explicit external email verification delivery handoff implementation: CustomerDomain defines the contract, sender abstraction, concrete infrastructure adapter, DI registration, options/configuration, and payload containing pending recipient email, customer id, token or token reference, expiry, verification URL or route data, `CustomerEmailChange` purpose, correlation id, and idempotency/retry metadata to the existing/external delivery dependency; the delivery dependency sends only and does not decide token validity or email promotion; raw tokens and unnecessary PII must not be logged. Validate Aspire composition participation before completion.

## Phase 1: Design Summary

See [data-model.md](./data-model.md), [contracts/customer-profile-api.md](./contracts/customer-profile-api.md), [profile-display-inventory.md](./profile-display-inventory.md), and [quickstart.md](./quickstart.md).

Design artifacts define:

- Customer Profile, Customer Account Identity, Profile Update, Email Change Request, Email Verification, Customer Profile Read Model, Customer Key Record, Web Profile DTO, and Customer Forgotten/Deleted integration event, including the `Forgotten` lifecycle state that publishes a PII-free async event for AuthenticationManagement-owned login cleanup.
- Customer profile API contracts for `/api/customer/me` reading/updating of the authenticated customer's profile, requesting/resending verification, and confirming pending email verification with token creation/storage/validation owned by CustomerDomain and delivery delegated externally.
- A profile-display inventory covering dashboard/account, auth/nav, checkout, orders birthday prompt, Web mock sources, and backoffice customer read surfaces that must be reviewed or updated for immediate profile consistency.
- Verified-customer-email and forgotten/deleted-customer integration event requirements for AuthenticationManagement login credential updates and deletion cleanup, including SC-008 behavior that CustomerDomain verification does not change login credentials until AuthenticationManagement consumes the verified-email event.
- Validation, authorization, immediate website-wide profile refresh without manual page reload in the same save response/render cycle, GDPR erasure, async deletion/login not-found, post-consumer same-email re-registration, pending email, external delivery handoff, verification-token handling, login promotion, mock-removal, operational log redaction for profile update/email verification/deletion without PII or raw tokens, and Aspire composition validation scenarios.

## Post-Design Constitution Check

- **Domain boundaries**: PASS — contracts and data model keep profile/pending email ownership in CustomerDomain; Web remains a consumer; AuthenticationManagement handles verified login email updates and forgotten/deleted-customer login cleanup through event consumers.
- **Backend standards**: PASS — design calls for thin authenticated `/api/customer/me` controller endpoints, request/response DTOs, focused command/query handlers, explicit mapping, cancellation-token propagation, and domain-specific exceptions.
- **Event sourcing + CQRS**: PASS — update behavior uses aggregate methods and events such as profile updated, email change requested, and email verified/promoted; projections update read models.
- **Testing discipline**: PASS — quickstart and tasks require domain/application/projection/API/Web and AuthenticationManagement integration-event validation, with Web and AuthenticationManagement tests housed only in runnable xUnit projects referenced by `src/CookieCommerce.slnx` so `dotnet test src` discovers them.
- **Async integration**: PASS — verified email promotion and customer forgetting/deletion cleanup are shared through outbox-backed integration events; no synchronous cross-context business decision is introduced, and CustomerDomain does not make AuthenticationManagement login-identity decisions directly.
- **Privacy + operations**: PASS — data model and contracts explicitly preserve encryption, crypto-shredding, deleted-customer login not-found behavior and same-email re-registration after AuthenticationManagement event consumption, no PII in deletion events, token creation/storage/validation without raw token logging, external email delivery delegation, redacted logs, explicit operational log redaction tests for profile update/email verification/deletion, and user-safe failures.
- **Aspire + containerization**: PASS — no new container; Web must add CustomerDomain API service reference and named client registration as needed; CustomerDomain and AuthenticationManagement messaging participants must remain in the Aspire composition.
- **Frontend standards**: PASS — components remain in Account folder; backend calls are routed through concrete Web services/models rather than component direct API calls or new same-project interfaces; accessible form/error/pending states remain required.
- **Documentation impact**: PASS — plan includes `Documentation/backend-documentation.md` update if existing service contracts or profile integration behavior change.

## Complexity Tracking

No constitution violations requiring justification.

## Agent Context Update

No agent context update script is present under `.specify/scripts` in this repository, so this step was checked and skipped without changes.
