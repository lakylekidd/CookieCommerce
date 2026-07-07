# Phase 0 Research: Update Customer Profile

## Decision: CustomerDomain owns profile update state

**Rationale**: The specification identifies the customer profile as customer-owned personal details, and the repository already contains `CookieCommerce.CustomerDomain.*` projects with `CustomerAggregate`, `CustomerReadModel`, crypto-shredding services, and Customer API endpoints. Keeping the slice in CustomerDomain preserves bounded-context ownership and keeps profile business rules out of Web and AuthenticationManagement.

**Alternatives considered**:
- Put update behavior in `CookieCommerce.Web`: rejected because Web must not own business rules or persistence.
- Put all profile behavior in AuthenticationManagement: rejected because AuthenticationManagement owns sign-in identity, not customer profile state.

## Decision: Model email changes as pending until verification succeeds, with US1 stopping before promotion

**Rationale**: The clarified requirement states that when a customer requests an email change, the new email must be stored as pending and the current verified email remains both the profile email and login credential until CustomerDomain verification succeeds; after CustomerDomain verification, the profile email can promote, but the login credential remains the old verified email until AuthenticationManagement consumes the verified-email event. US1 is complete when editable profile data saves, pending email request/resend works, and the old verified email remains active. US3 owns verification confirmation, verified-email integration, and AuthenticationManagement login credential promotion. This avoids account lockout, prevents takeover through unverified addresses, and gives tests a clear state transition: requested → pending verification → verified profile email → AuthenticationManagement-consumed login promotion.

**Alternatives considered**:
- Replace the profile email immediately and keep only login on the old email: rejected because profile display would show an unverified identity as authoritative.
- Replace both profile and login immediately: rejected because it weakens account security and contradicts the clarification.
- Require same-session verification before saving: rejected because it creates brittle UX and prevents normal out-of-band email verification.

## Decision: CustomerDomain emits a verified-email integration event for AuthenticationManagement

**Rationale**: The verified customer email is the sign-in credential, but AuthenticationManagement owns the login identity. The constitution forbids synchronous cross-bounded-context business decisions, so CustomerDomain verification only promotes the CustomerDomain profile email and publishes an outbox-backed event; AuthenticationManagement updates its login credential only after consuming that event. The consumer must be idempotent and retry-safe, and tests must prove old-email login remains valid until consumption and new-email login is required only after consumption.

**Alternatives considered**:
- CustomerDomain calls AuthenticationManagement synchronously during profile update: rejected by asynchronous cross-domain integration rules.
- Web calls both APIs to coordinate the email change: rejected because Web would orchestrate business consistency.
- AuthenticationManagement updates on pending email request: rejected because login must not switch until verification succeeds.

## Decision: Complete actual authenticated CustomerDomain profile and email-verification endpoints and use them from Web

**Rationale**: The dashboard currently contains `FakeAccountService`, `StaticProfileList`, and static profile data. The spec requires actual implemented endpoints and no mock-data fallback for customer-facing profile load/save or any website surface that shows customer name, email, gender, or date of birth. Dashboard-facing profile endpoints should use authenticated `/api/customer/me` routes so CustomerDomain derives the customer id from the signed-in session and does not rely on caller-supplied ids for ordinary customer flows. Existing CustomerDomain API has `GET /api/customer/{id}` and create/delete/address endpoints, but no complete authenticated profile update or pending email verification contract; those missing capabilities are in scope.

**Alternatives considered**:
- Continue using `FakeAccountService` for profile updates: rejected by specification and SC-007.
- Keep mock fallback when API fails: rejected because it can display or save fake data as real when the live service failed.
- Use `/api/customer/{customerId}` as the dashboard profile contract: rejected for customer-facing flows because it increases cross-customer access risk; existing id routes may remain for internal/admin/deletion paths but must enforce ownership/policy.
- Implement only frontend service wiring without backend changes: rejected because current API does not expose a complete profile/email-verification contract.

## Decision: Use focused command/event/projection paths

**Rationale**: Constitution requires event-sourced state changes through aggregates and CQRS read models. Non-email profile updates should use a command such as `UpdateCustomerProfileCommand`; email changes should use separate commands such as `RequestCustomerEmailChangeCommand` and `VerifyCustomerEmailChangeCommand` so validation, verification token handling, and login promotion are explicit. Controllers remain thin and map request/response DTOs only.

**Alternatives considered**:
- Directly update `CustomerReadModel`: rejected because durable state changes must go through aggregate events.
- Reuse `CustomerCreated` by recreating the stream: rejected because it corrupts audit history and lifecycle semantics.
- Combine profile update and verification into one command: rejected because verification is a separate lifecycle step and must be independently testable.

## Decision: Preserve GDPR crypto-shredding exactly as documented

**Rationale**: `Documentation/GDPR-Compliance.md` states that event-stream personal data is encrypted before events/aggregate state, each customer has a unique encryption key, deletion removes the key and read models, and deletion events contain no personal data. Profile update and pending/verified email events must use the same model. Pending email is PII and must be encrypted if it enters immutable history.

**Alternatives considered**:
- Store updated or pending email in plaintext events: rejected because email is event-sourced PII.
- Delete or rewrite events for erasure: rejected because event immutability is a core architecture constraint.
- Store gender/date of birth only in read models and not in immutable events: rejected by the clarified requirement for this feature. Gender and date of birth are profile PII and must use the same encrypted immutable CustomerDomain event storage model as full name and email, with decrypted projection copies removed during GDPR deletion.

## Decision: Customer deletion publishes an async forgotten/deleted-customer integration event

**Rationale**: CustomerDomain owns customer profile lifecycle and GDPR crypto-shredding, but AuthenticationManagement owns login identities and login-email uniqueness. On deletion/forgetting, CustomerDomain must delete the customer key/read models, write PII-free deletion records, and publish an outbox-backed `CustomerForgotten`/`CustomerDeleted` integration event. AuthenticationManagement must consume that event idempotently and then remove or disable the login identity and release login-email uniqueness so login returns not found and the former email can be used for a new registration after processing. This keeps deletion eventually consistent across bounded contexts without allowing CustomerDomain to make login-identity business decisions directly.

**Alternatives considered**:
- CustomerDomain directly disables or deletes the AuthenticationManagement identity: rejected because CustomerDomain must not perform login-identity business decisions directly.
- CustomerDomain directly releases AuthenticationManagement login-email uniqueness: rejected because that uniqueness policy belongs to AuthenticationManagement.
- Synchronous CustomerDomain-to-AuthenticationManagement call during deletion: rejected by asynchronous cross-domain integration rules and outbox requirements.
- Disable login but keep the deleted email reserved after consumer processing: rejected because it prevents registration with the same email after the deletion event is processed.
- Return an account-disabled/authentication-failed message after consumer processing: rejected because the required behavior is not found.
- Preserve plaintext deleted email in CustomerDomain for future duplicate checks: rejected because it violates GDPR crypto-shredding semantics.

## Decision: Validate email format and uniqueness before creating a pending email change

**Rationale**: The spec requires requested email addresses to be valid and unique across customer accounts before creating a pending change. The foundational/US1 application path should provide a read-model uniqueness check that excludes the current customer and considers both verified and pending emails to avoid duplicate pending claims; this check must execute before any pending email state or token is created, and US2 may only refine validation messages/tests for this already-existing behavior. If AuthenticationManagement maintains a separate login uniqueness index, it should reconcile from verified-email integration events and from forgotten/deleted-customer events, rejecting conflicting verified-email updates idempotently and releasing deleted identities only in its own consumer.

**Alternatives considered**:
- Allow duplicate pending emails and resolve later: rejected because it creates race conditions and poor UX.
- Validate uniqueness only in UI: rejected because server-side validation is required for correctness and security.

## Decision: Web uses typed service calls over Aspire service discovery

**Rationale**: `src/CookieCommerce.Web/Program.cs` already registers named clients for service integrations. Customer profile integration should register a CustomerDomain named client using the Aspire service name from `src/CookieCommerce.AppHost/AppHost.cs` (`customer-api`) and route UI calls through a service in `src/CookieCommerce.Web/Services`.

**Alternatives considered**:
- Components call API endpoints directly: rejected by frontend constitution standards.
- Hard-code localhost URLs in components: rejected because Aspire service discovery/configuration should own service endpoints.

## Decision: Refresh all website profile displays immediately after a successful save

**Rationale**: The clarification requires updated profile information wherever customer name, email, gender, or date of birth is shown on the website, visible without manual page reload within the same successful save response/render cycle, and no sync-pending or stale state is acceptable after a successful save. `EditProfileModal.razor` and `PersonalDetailsCard.razor` remain the primary edit/display location. Planning now includes `profile-display-inventory.md`, initially identifying customer-facing dashboard/account, auth/nav, checkout, birthday-prompt, mock profile source, and backoffice customer read surfaces to verify before implementation changes those surfaces.

**Alternatives considered**:
- Limit refresh to Dashboard Overview/Personal Details only: rejected because the clarification applies website-wide to displayed customer profile fields.
- Allow eventual projection lag with an "updating" message: rejected because no sync-pending state is acceptable.
- Redesign unrelated dashboard pages: rejected as unnecessary unless they display customer profile fields.

## Decision: Keep profile UI focused while exposing pending email state

**Rationale**: `EditProfileModal.razor` and `PersonalDetailsCard.razor` already provide the dashboard edit location. The implementation should preserve this user flow while adding live loading, validation errors, failure states, success feedback, immediate profile-display refresh in the same save response/render cycle, and pending-email messaging. The verified email remains displayed as the account/login email; pending email is shown as pending verification until promoted.

**Alternatives considered**:
- Redesign the dashboard profile flow entirely: rejected as unnecessary scope expansion.
- Hide pending email state from the customer: rejected because it would make verification and login behavior confusing.

## Decision: Use a concrete Web profile service instead of adding a same-project interface

**Rationale**: The constitution says interfaces and implementations must not reside in the same project. To avoid expanding an existing smell and to resolve the analyze finding, this feature should register a concrete typed `CustomerProfileService` under `src/CookieCommerce.Web/Services` and inject/use it directly from components or a shared state service. Existing `IAccountService` can remain for unrelated account behavior, but this feature should not add `ICustomerProfileService` in the Web project or expand an interface located beside its implementation.

**Alternatives considered**:
- Add `ICustomerProfileService` beside `CustomerProfileService` in `CookieCommerce.Web`: rejected by constitution.
- Expand `IAccountService` for the new profile flow: rejected unless a broader refactor first moves the interface to an approved separate project.
- Let Razor components call APIs directly: rejected by frontend constitution standards.

## Decision: CustomerDomain owns verification tokens; external dependency handles email delivery through an explicit handoff contract

**Rationale**: The clarification keeps token creation, storage, and validation in scope while delegating actual email delivery to existing/external notification infrastructure. CustomerDomain must create a token or token reference, store only safe/hashed references as appropriate, validate submitted tokens, avoid raw token logging/events, and hand off pending recipient email, customer id, token/reference, expiry, verification URL or route data, email-change purpose, correlation id, and retry/idempotency metadata through a concrete sender/adapter registered in DI. The adapter is the only production handoff path from CustomerDomain to the configured external delivery dependency; it sends/renders nothing itself unless the existing dependency contract requires a simple client wrapper.

**Alternatives considered**:
- Build full email delivery/templates/sending in this feature: rejected as new scope not required by the clarification.
- Treat token creation and validation as external: rejected because acceptance tests must verify CustomerDomain pending-email promotion behavior.
- Use manual/test-only token capture for production behavior: rejected because production needs a real handoff to the existing/external delivery path.

## Decision: Operational log safety requires explicit test coverage

**Rationale**: Profile update, email verification, and customer deletion/forgetting all touch PII or token-adjacent flows. The design must include a concrete test that captures operational logs from these flows and asserts they do not contain first name, last name, email address, gender, date of birth, customer encryption keys, or raw verification tokens. This makes the privacy/observability requirement executable rather than relying on implementation review alone.

**Alternatives considered**:
- Rely only on code review of logging statements: rejected because future log template changes could reintroduce leakage without failing tests.
- Test only user-facing messages: rejected because operational logs are a separate leakage surface.
- Redact only raw tokens but allow emails in logs: rejected because the requirement forbids unnecessary PII in operational logs.

## Decision: Aspire composition validation is a required polish gate

**Rationale**: The clarified requirement states affected services must still participate in the Aspire composition before completion. CustomerDomain API, AuthenticationManagement API, RabbitMQ, Keycloak/PostgreSQL dependencies, and Web must remain registered/startable through `src/CookieCommerce.AppHost/AppHost.cs`; profile service discovery and email-handoff configuration must not rely on hard-coded local URLs.

**Alternatives considered**:
- Rely only on `dotnet build`/`dotnet test`: rejected because service discovery and composition wiring failures can pass compile/test but fail at runtime.
- Validate only the Web project: rejected because profile update spans CustomerDomain, AuthenticationManagement, RabbitMQ, and Web.

## Decision: Documentation update is required if Web profile integration behavior or existing service contracts change

**Rationale**: Constitution explicitly requires `Documentation/backend-documentation.md` updates when `src/CookieCommerce.Web/Services/**/*.cs` interfaces change. Even without adding a new interface, profile integration behavior and service registration changes should be documented so developers understand live CustomerDomain profile dependencies and the lack of mock fallback.

**Alternatives considered**:
- Skip documentation for internal Web services: rejected by constitution.
## Decision: Add Web and AuthenticationManagement test-project foundations before story tests

**Rationale**: Feature tasks already place Web tests under `src/CookieCommerce.Web.Tests` and AuthenticationManagement tests under `src/CookieCommerce.AuthenticationManagement.Tests`, but those projects do not currently exist in the solution. Creating runnable xUnit projects with FluentAssertions and Moq during setup, referencing them from `src/CookieCommerce.slnx`, running restore first, and then proving discovery with `dotnet test src --no-restore --list-tests` prevents orphan test files that are never executed by the required quality gate. This resolves analyze C1 and aligns with the constitution requirement that tests be runnable with `dotnet test src`.

**Alternatives considered**:
- Leave test project creation implicit in the first story test task: rejected because early test files could be orphaned outside a runnable project.
- Put Web or AuthenticationManagement tests into existing CustomerDomain tests: rejected because tests should stay with the bounded context or UI surface they validate.
- Skip solution references and run project-specific test commands only: rejected because the required broad gate is `dotnet test src`.
