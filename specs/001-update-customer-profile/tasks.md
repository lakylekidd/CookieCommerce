# Tasks: Update Customer Profile

**Input**: Design documents from `/specs/001-update-customer-profile/`

**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/customer-profile-api.md`, `quickstart.md`, `profile-display-inventory.md`, `.specify/memory/constitution.md`

**Tests**: Tests are REQUIRED by the feature specification and constitution for domain behavior, validation, projections, API contracts, Web integration, messaging, and cross-domain consumers. Create runnable test projects before adding test files.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1, US2, US3)
- Every task includes concrete file paths.

## Path Conventions

- **CustomerDomain**: `src/CookieCommerce.CustomerDomain.Api/`, `src/CookieCommerce.CustomerDomain.Application/`, `src/CookieCommerce.CustomerDomain.Domain/`, `src/CookieCommerce.CustomerDomain.Infrastructure/`, `src/CookieCommerce.CustomerDomain.Tests/`
- **AuthenticationManagement**: `src/CookieCommerce.AuthenticationManagement.Application/`, `src/CookieCommerce.AuthenticationManagement.Infrastructure/`, `src/CookieCommerce.AuthenticationManagement.Tests/`
- **Blazor UI**: `src/CookieCommerce.Web/`, `src/CookieCommerce.Web.Tests/`
- **Shared platform**: `src/CookieCommerce.CookieMassTransit/`, `src/CookieCommerce.ServiceDefaults/`, `src/CookieCommerce.AppHost/`
- **Documentation/specs**: `Documentation/backend-documentation.md`, `specs/001-update-customer-profile/`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Prepare runnable test projects and solution wiring before story work so no feature tests are orphaned.

- [X] T001 Create xUnit test project with FluentAssertions and Moq in `src/CookieCommerce.Web.Tests/CookieCommerce.Web.Tests.csproj`
- [X] T002 Create xUnit test project with FluentAssertions and Moq in `src/CookieCommerce.AuthenticationManagement.Tests/CookieCommerce.AuthenticationManagement.Tests.csproj`
- [X] T003 Add `src/CookieCommerce.Web.Tests/CookieCommerce.Web.Tests.csproj` to `src/CookieCommerce.slnx`
- [X] T004 Add `src/CookieCommerce.AuthenticationManagement.Tests/CookieCommerce.AuthenticationManagement.Tests.csproj` to `src/CookieCommerce.slnx`
- [X] T005 [P] Add initial test folders in `src/CookieCommerce.Web.Tests/Services/`, `src/CookieCommerce.Web.Tests/Components/Account/`, `src/CookieCommerce.AuthenticationManagement.Tests/ConsumerTests/`, and `src/CookieCommerce.AuthenticationManagement.Tests/IntegrationTests/`
- [X] T006 Restore packages with `dotnet restore src` and then verify test discovery with `dotnet test src --no-restore --list-tests` using `src/CookieCommerce.slnx`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Establish shared profile, PII, messaging, and configuration foundations that all user stories depend on.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

- [X] T007 Define profile API request/response DTO records for `/api/customer/me` contracts in `src/CookieCommerce.CustomerDomain.Api/Models/Customers/CustomerProfileDtos.cs`
- [X] T008 Define application profile command/query records and result models in `src/CookieCommerce.CustomerDomain.Application/Customers/Profile/CustomerProfileMessages.cs`
- [X] T009 Define CustomerDomain profile event classes for encrypted first name, last name, verified email, pending email, gender, and date of birth in `src/CookieCommerce.CustomerDomain.Domain/DomainEvents/CustomerProfileEvents.cs`
- [X] T010 Define profile domain value objects/enums for supported gender and date-of-birth rules in `src/CookieCommerce.CustomerDomain.Domain/ValueObjects/CustomerProfileValueObjects.cs`
- [X] T011 Update `CustomerReadModel` with verified email, pending email, email verification status, gender, and date of birth projection fields in `src/CookieCommerce.CustomerDomain.Application/Customers/ReadModels/CustomerReadModel.cs`
- [X] T012 Define PII encryption/decryption helper contract usage for profile event payloads in `src/CookieCommerce.CustomerDomain.Application/Customers/Profile/CustomerProfileEncryption.cs`
- [X] T013 Define verified-email and customer-forgotten integration event contracts in `src/CookieCommerce.CustomerDomain.Application/Customers/Integration/CustomerProfileIntegrationEvents.cs`
- [X] T014 Define email verification token store abstraction and token reference model in `src/CookieCommerce.CustomerDomain.Application/Customers/Integration/EmailVerificationTokenStore.cs`
- [X] T015 Define external email verification delivery handoff payload and sender contract in `src/CookieCommerce.CustomerDomain.Application/Customers/Integration/EmailVerificationDelivery.cs`
- [X] T016 Define email delivery options for verification URL/route, expiry, retry, and external dependency settings in `src/CookieCommerce.CustomerDomain.Infrastructure/Configuration/EmailVerificationDeliveryOptions.cs`
- [X] T017 Add CustomerDomain exception types for profile validation, duplicate email, missing profile, invalid token, expired token, and unauthorized ownership in `src/CookieCommerce.CustomerDomain.Domain/Exceptions/CustomerProfileExceptions.cs`
- [X] T018 Define and wire the CustomerDomain email uniqueness checker contract/skeleton for verified and pending emails, excluding the current customer, and update dependency-injection skeleton for profile handlers, token storage, delivery adapter, projection, uniqueness checker, and options in `src/CookieCommerce.CustomerDomain.Application/Customers/Profile/CustomerEmailUniquenessService.cs` and `src/CookieCommerce.CustomerDomain.Infrastructure/DependencyInjection.cs`

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel.

---

## Phase 3: User Story 1 - Update personal details from dashboard (Priority: P1) 🎯 MVP

**Goal**: Authenticated customers can load live profile data, edit first name, last name, gender, date of birth, request a pending email change, resend verification, and continue seeing/signing in with the old verified email while pending verification exists.

**Independent Test**: Sign in, open dashboard update profile, verify data comes from CustomerDomain not static/mocks, save valid first name/last name/gender/date of birth plus a new email, confirm non-email fields update immediately, confirm `email` remains the old verified value, confirm `pendingEmail` is shown, and confirm verification delivery handoff occurred without login credential promotion.

### Tests for User Story 1 ⚠️

- [X] T019 [P] [US1] Add aggregate tests for encrypted profile updates and pending-email state in `src/CookieCommerce.CustomerDomain.Tests/DomainTests/CustomerAggregate_UpdateProfile_Tests.cs`
- [X] T020 [P] [US1] Add aggregate tests proving gender and date of birth are emitted as encrypted immutable profile event fields in `src/CookieCommerce.CustomerDomain.Tests/DomainTests/CustomerAggregate_ProfilePiiEncryption_Tests.cs`
- [X] T021 [P] [US1] Add projection tests for decrypted profile fields, pending email, gender, and date of birth in `src/CookieCommerce.CustomerDomain.Tests/ProjectionsTests/CustomerProjection_ProfileUpdate_Tests.cs`
- [X] T022 [P] [US1] Add handler tests for `GetCurrentCustomerProfileQuery` and `UpdateCustomerProfileCommand` in `src/CookieCommerce.CustomerDomain.Tests/ApplicationTests/CustomerProfileHandlerTests.cs`
- [X] T023 [P] [US1] Add email delivery handoff tests for payload fields, idempotency key, expiry, recipient, purpose, and token redaction in `src/CookieCommerce.CustomerDomain.Tests/ApplicationTests/EmailVerificationDeliveryHandoffTests.cs`
- [X] T024 [P] [US1] Add CustomerDomain API contract tests for `GET /api/customer/me`, `PATCH /api/customer/me/profile`, and `POST /api/customer/me/email-verification` in `src/CookieCommerce.CustomerDomain.Tests/IntegrationTests/CustomerProfileApiTests.cs`
- [X] T025 [P] [US1] Add Web service tests for live load/save/resend mapping and no mock fallback in `src/CookieCommerce.Web.Tests/Services/CustomerProfileServiceTests.cs`
- [X] T026 [P] [US1] Add Web component tests for profile display, edit modal, pending email message, and immediate refreshed state visible without manual page reload in the same save response/render cycle in `src/CookieCommerce.Web.Tests/Components/Account/ProfileComponentsTests.cs`

### Implementation for User Story 1

- [X] T027 [P] [US1] Implement profile update and pending-email aggregate methods in `src/CookieCommerce.CustomerDomain.Domain/Aggregates/CustomerAggregate.cs`
- [X] T028 [P] [US1] Implement encryption mapping for first name, last name, verified email, pending email, gender, and date of birth events in `src/CookieCommerce.CustomerDomain.Application/Customers/Profile/CustomerProfileEncryption.cs`
- [X] T029 [US1] Implement `GetCurrentCustomerProfileQueryHandler` in `src/CookieCommerce.CustomerDomain.Application/Customers/Handlers/GetCurrentCustomerProfileQueryHandler.cs`
- [X] T030 [US1] Implement `UpdateCustomerProfileCommandHandler` with non-email updates, verified/pending email uniqueness checked before any pending email is created, token creation, and delivery handoff in `src/CookieCommerce.CustomerDomain.Application/Customers/Handlers/UpdateCustomerProfileCommandHandler.cs`
- [X] T031 [US1] Implement `ResendCustomerEmailVerificationCommandHandler` with current pending-email validation and retry-safe delivery handoff in `src/CookieCommerce.CustomerDomain.Application/Customers/Handlers/ResendCustomerEmailVerificationCommandHandler.cs`
- [X] T032 [US1] Implement token reference persistence with hashed/reference-only storage in `src/CookieCommerce.CustomerDomain.Infrastructure/Security/EmailVerificationTokenStore.cs`
- [X] T033 [US1] Implement concrete external email verification sender/adapter in `src/CookieCommerce.CustomerDomain.Infrastructure/Integration/ExternalEmailVerificationSender.cs`
- [X] T034 [US1] Register the external email verification sender/adapter, options, and token store in `src/CookieCommerce.CustomerDomain.Infrastructure/DependencyInjection.cs`
- [X] T035 [US1] Add email verification delivery configuration placeholders for URL/route, expiry, retry, and external dependency endpoint in `src/CookieCommerce.CustomerDomain.Api/appsettings.json`
- [X] T036 [US1] Update CustomerDomain profile projection for encrypted event decryption and removable read-model fields in `src/CookieCommerce.CustomerDomain.Infrastructure/Projections/CustomerProjection.cs`
- [X] T037 [US1] Implement thin authenticated `GET /api/customer/me`, `PATCH /api/customer/me/profile`, and `POST /api/customer/me/email-verification` endpoints in `src/CookieCommerce.CustomerDomain.Api/Controllers/CustomerController.cs`
- [X] T038 [US1] Add API DTO mapping extensions for profile responses, validation problems, and pending-email status in `src/CookieCommerce.CustomerDomain.Api/Models/Customers/CustomerProfileMappings.cs`
- [X] T039 [US1] Update CustomerDomain ProblemDetails/domain exception mapping for profile errors in `src/CookieCommerce.CustomerDomain.Api/Program.cs`
- [X] T040 [US1] Add UI-facing profile DTOs and result/validation models in `src/CookieCommerce.Web/Models/Account/UserProfileDto.cs`
- [X] T041 [US1] Implement concrete live CustomerDomain profile service using Aspire-named `HttpClient` in `src/CookieCommerce.Web/Services/CustomerProfileService.cs`
- [X] T042 [US1] Register CustomerDomain API named client and concrete `CustomerProfileService` in `src/CookieCommerce.Web/Program.cs`
- [X] T043 [US1] Add shared profile state for immediate refresh after saves so updated profile values are visible without manual page reload during the same save response/render cycle in `src/CookieCommerce.Web/Services/CustomerProfileState.cs`
- [X] T044 [US1] Replace profile load/save in `src/CookieCommerce.Web/Components/Account/PersonalDetailsCard.razor` with `CustomerProfileService` and `CustomerProfileState`
- [X] T045 [US1] Replace edit form submission in `src/CookieCommerce.Web/Components/Account/EditProfileModal.razor` with live service calls, allowed gender values, pending-email status, and user-safe success/failure messages
- [X] T046 [US1] Update account overview loading and live-service failure state in `src/CookieCommerce.Web/Components/Account/Overview.razor`
- [X] T047 [US1] Update birthday prompt source to use live profile `HasBirthdateAsync` behavior in `src/CookieCommerce.Web/Components/Account/Orders/Orders.razor`
- [X] T048 [US1] Remove customer-facing profile dependencies on fake/static profile sources in `src/CookieCommerce.Web/Services/FakeAccountService.cs`
- [X] T049 [US1] Ensure static profile data is not used for customer-facing profile load/save/display in `src/CookieCommerce.Web/Services/StaticData/StaticProfileList.cs`
- [X] T050 [US1] Update customer-facing profile display inventory with implementation decisions and remaining exclusions in `specs/001-update-customer-profile/profile-display-inventory.md`
- [X] T051 [US1] Run MVP verification for US1 with `dotnet test src --filter CustomerProfile` using `src/CookieCommerce.slnx`

**Checkpoint**: User Story 1 is fully functional and independently testable as the MVP.

---

## Phase 4: User Story 2 - Prevent invalid profile updates (Priority: P2)

**Goal**: Invalid names, invalid or duplicate emails, unsupported gender values, and invalid dates of birth are rejected with clear field-specific feedback and no saved profile changes; email uniqueness implementation already exists from Foundation/US1, so US2 only refines validation messages and tests around that behavior.

**Independent Test**: Attempt invalid names, invalid email format, duplicate verified email, duplicate pending email, unsupported gender, and future date of birth; verify field-level API/UI feedback and unchanged saved profile state.

### Tests for User Story 2 ⚠️

- [ ] T052 [P] [US2] Add domain validation tests for required names, supported gender, and past date of birth in `src/CookieCommerce.CustomerDomain.Tests/DomainTests/CustomerAggregate_ProfileValidation_Tests.cs`
- [ ] T053 [P] [US2] Add application validation-message tests for email format and pre-existing duplicate verified/pending email failures in `src/CookieCommerce.CustomerDomain.Tests/ApplicationTests/UpdateCustomerProfileValidationTests.cs`
- [ ] T054 [P] [US2] Add API validation problem contract tests for invalid profile updates in `src/CookieCommerce.CustomerDomain.Tests/IntegrationTests/CustomerProfileValidationApiTests.cs`
- [ ] T055 [P] [US2] Add Web validation mapping tests for field-level messages and unchanged form state in `src/CookieCommerce.Web.Tests/Services/CustomerProfileValidationTests.cs`

### Implementation for User Story 2

- [ ] T056 [US2] Implement boundary validation for required names, email format, supported gender, and past date of birth in `src/CookieCommerce.CustomerDomain.Application/Customers/Profile/CustomerProfileValidator.cs`
- [ ] T057 [US2] Refine duplicate-email validation messages and field-level error mapping using the Foundation/US1 uniqueness checker in `src/CookieCommerce.CustomerDomain.Application/Customers/Profile/CustomerEmailUniquenessService.cs`
- [ ] T058 [US2] Wire validation and duplicate email failures into `UpdateCustomerProfileCommandHandler` without appending events in `src/CookieCommerce.CustomerDomain.Application/Customers/Handlers/UpdateCustomerProfileCommandHandler.cs`
- [ ] T059 [US2] Return field-level validation and conflict responses from CustomerDomain API mappings in `src/CookieCommerce.CustomerDomain.Api/Models/Customers/CustomerProfileMappings.cs`
- [ ] T060 [US2] Display field-specific validation feedback in the profile edit form in `src/CookieCommerce.Web/Components/Account/EditProfileModal.razor`
- [ ] T061 [US2] Add user-safe service failure and validation-result mapping in `src/CookieCommerce.Web/Services/CustomerProfileService.cs`
- [ ] T062 [US2] Run validation verification with `dotnet test src --filter ProfileValidation` using `src/CookieCommerce.slnx`

**Checkpoint**: User Stories 1 and 2 both work independently without invalid updates changing saved state.

---

## Phase 5: User Story 3 - Protect account identity and privacy (Priority: P3)

**Goal**: Profile operations are owner-only, unauthenticated users are blocked, verification promotes pending email only in CustomerDomain first, AuthenticationManagement updates login only after consuming the verified-email event, and deletion/forgetting cleanup remains async/idempotent and GDPR-safe.

**Independent Test**: Verify unauthenticated access is rejected, customers cannot read/update another profile, confirmation messages do not leak PII, email verification promotes CustomerDomain profile email but not login until AuthenticationManagement consumer processes the event, deletion publishes a PII-free forgotten event, and AuthenticationManagement consumption makes login not found and releases same-email registration.

### Tests for User Story 3 ⚠️

- [ ] T063 [P] [US3] Add CustomerDomain authorization tests for unauthenticated and cross-customer profile access in `src/CookieCommerce.CustomerDomain.Tests/IntegrationTests/CustomerProfileAuthorizationTests.cs`
- [ ] T064 [P] [US3] Add email verification token success, invalid, expired, and mismatched-token tests in `src/CookieCommerce.CustomerDomain.Tests/ApplicationTests/VerifyCustomerEmailChangeTests.cs`
- [ ] T065 [P] [US3] Add projection tests for verified email promotion clearing pending email/token reference in `src/CookieCommerce.CustomerDomain.Tests/ProjectionsTests/CustomerProjection_EmailVerification_Tests.cs`
- [ ] T066 [P] [US3] Add outbox integration-event tests for verified-email and forgotten/deleted-customer publication in `src/CookieCommerce.CustomerDomain.Tests/IntegrationTests/CustomerProfileOutboxTests.cs`
- [ ] T067 [P] [US3] Add AuthenticationManagement verified-email consumer idempotency tests in `src/CookieCommerce.AuthenticationManagement.Tests/ConsumerTests/VerifiedCustomerEmailChangedConsumerTests.cs`
- [ ] T068 [P] [US3] Add AuthenticationManagement forgotten/deleted-customer consumer idempotency and email-reuse tests in `src/CookieCommerce.AuthenticationManagement.Tests/ConsumerTests/CustomerForgottenConsumerTests.cs`
- [ ] T069 [P] [US3] Add GDPR crypto-shredding tests for full name, verified email, pending email, gender, and date of birth in immutable events and read models in `src/CookieCommerce.CustomerDomain.Tests/IntegrationTests/CustomerProfileGdprErasureTests.cs`
- [ ] T070 [P] [US3] Add operational logging redaction tests proving profile update, email verification, and customer deletion/forgetting logs contain no first name, last name, email address, date of birth, gender, or raw verification tokens in `src/CookieCommerce.CustomerDomain.Tests/IntegrationTests/CustomerProfileOperationalLoggingTests.cs`

### Implementation for User Story 3

- [ ] T071 [US3] Enforce authenticated `/me` ownership resolution in CustomerDomain profile handlers in `src/CookieCommerce.CustomerDomain.Application/Customers/Profile/CurrentCustomerResolver.cs`
- [ ] T072 [US3] Implement `VerifyCustomerEmailChangeCommandHandler` with token validation, expiry checks, pending-email promotion, and verified-email event enqueueing in `src/CookieCommerce.CustomerDomain.Application/Customers/Handlers/VerifyCustomerEmailChangeCommandHandler.cs`
- [ ] T073 [US3] Implement `POST /api/customer/me/email-verification/confirm` endpoint in `src/CookieCommerce.CustomerDomain.Api/Controllers/CustomerController.cs`
- [ ] T074 [US3] Update aggregate verification behavior to clear pending email/token reference and preserve old login boundary in `src/CookieCommerce.CustomerDomain.Domain/Aggregates/CustomerAggregate.cs`
- [ ] T075 [US3] Publish verified customer email changed integration events through the CustomerDomain outbox in `src/CookieCommerce.CustomerDomain.Infrastructure/Integration/CustomerProfileOutboxPublisher.cs`
- [ ] T076 [US3] Publish PII-free customer forgotten/deleted integration events through the CustomerDomain outbox in `src/CookieCommerce.CustomerDomain.Infrastructure/Integration/CustomerForgottenOutboxPublisher.cs`
- [ ] T077 [US3] Update CustomerDomain deletion flow to crypto-shred profile PII and remove read-model copies without touching AuthenticationManagement login state in `src/CookieCommerce.CustomerDomain.Application/Customers/Handlers/DeleteCustomerCommandHandler.cs`
- [ ] T078 [US3] Implement AuthenticationManagement verified-email consumer application use case in `src/CookieCommerce.AuthenticationManagement.Application/Auth/ConsumeVerifiedCustomerEmailChangedCommandHandler.cs`
- [ ] T079 [US3] Implement AuthenticationManagement forgotten/deleted-customer cleanup use case in `src/CookieCommerce.AuthenticationManagement.Application/Auth/ConsumeCustomerForgottenCommandHandler.cs`
- [ ] T080 [US3] Implement MassTransit consumers and idempotency guards for verified-email and forgotten/deleted-customer events in `src/CookieCommerce.AuthenticationManagement.Infrastructure/Consumers/CustomerProfileIntegrationConsumers.cs`
- [ ] T081 [US3] Register AuthenticationManagement consumers, idempotency storage, and message mappings in `src/CookieCommerce.AuthenticationManagement.Infrastructure/DependencyInjection.cs`
- [ ] T082 [US3] Add Web email verification confirmation route and user-safe result display in `src/CookieCommerce.Web/Components/Account/EmailVerificationConfirm.razor`
- [ ] T083 [US3] Refresh verified profile email after successful confirmation while leaving login credential messaging tied to AuthenticationManagement consumption in `src/CookieCommerce.Web/Services/CustomerProfileState.cs`
- [ ] T084 [US3] Run identity/privacy verification with `dotnet test src --filter "Authorization|EmailVerification|CustomerForgotten|Gdpr|OperationalLogging"` using `src/CookieCommerce.slnx`

**Checkpoint**: All user stories are independently functional and cross-domain identity/privacy behavior is verified.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Final quality gates, documentation, and Aspire validation across the feature.

- [ ] T085 [P] Update backend/profile integration documentation in `Documentation/backend-documentation.md`
- [ ] T086 [P] Update quickstart validation notes for email delivery handoff and test projects in `specs/001-update-customer-profile/quickstart.md`
- [ ] T087 [P] Verify affected service registration and service discovery in `src/CookieCommerce.AppHost/AppHost.cs`
- [ ] T088 Verify CustomerDomain API container configuration still builds using `src/CookieCommerce.CustomerDomain.Api/Dockerfile`
- [ ] T089 Verify AuthenticationManagement API container configuration still builds using `src/CookieCommerce.AuthenticationManagement.Api/Dockerfile`
- [ ] T090 Verify Web container/configuration still builds and has no hard-coded profile API localhost dependency using `src/CookieCommerce.Web/Dockerfile`
- [ ] T091 Run formatting for changed code with `dotnet format src/CookieCommerce.slnx` using `src/CookieCommerce.slnx`
- [ ] T092 Run full build with `dotnet build src` using `src/CookieCommerce.slnx`
- [ ] T093 Run full regression suite with `dotnet test src` using `src/CookieCommerce.slnx`
- [ ] T094 Validate Aspire composition by starting `dotnet run --project src/CookieCommerce.AppHost` and confirming CustomerDomain API, AuthenticationManagement API, RabbitMQ, Keycloak, PostgreSQL, and Web are healthy in `src/CookieCommerce.AppHost/AppHost.cs`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies; start immediately.
- **Foundational (Phase 2)**: Depends on Setup completion; blocks all user stories.
- **User Story 1 (Phase 3)**: Depends on Foundational; MVP scope.
- **User Story 2 (Phase 4)**: Depends on US1 profile update path because US2 refines validation and field-level feedback for the live update flow; final verification must include US1 regression.
- **User Story 3 (Phase 5)**: Depends on US1 pending-email state for verification confirmation and login-promotion behavior; AuthenticationManagement consumer scaffolding can start after event contracts exist, but story completion waits for US1.
- **Polish (Phase 6)**: Depends on desired user stories being complete.

### User Story Dependencies

- **US1 (P1)**: Can start after Phase 2; no dependency on US2 or US3. US1 owns the first runnable email uniqueness check before pending email creation.
- **US2 (P2)**: Starts after US1's live profile update/pending-email path exists; strengthens the same flow and must not break US1. US2 may refine validation messages/tests but must not introduce the first email uniqueness implementation.
- **US3 (P3)**: Starts after US1's pending-email contract exists for verification confirmation; AuthenticationManagement consumer work may be prepared after Phase 2 event contracts, but end-to-end US3 completion depends on US1.

### Within Each User Story

- Tests should be written before or alongside implementation and should fail for the intended reason when practical.
- Domain/value objects/events before application handlers.
- Application handlers before API endpoints.
- API contracts before Web service/component integration.
- Messaging contracts before consumers.
- Story complete before moving to the next priority if working sequentially.

### Parallel Opportunities

- T001 and T002 can be implemented by separate agents; T003 and T004 follow their respective projects.
- T007 through T018 can be split by layer once naming is agreed: API DTOs, domain events/value objects, application messages, infrastructure options, email uniqueness skeleton, and exceptions are separate files.
- US1 test tasks T019 through T026 can run in parallel across CustomerDomain and Web test projects.
- US1 implementation tasks T027, T028, T032, T033, T040, and T043 can run in parallel after foundations.
- US2 test tasks T052 through T055 can run in parallel after US1 establishes the live update path they validate.
- US3 test tasks T063 through T070 can run in parallel after US1 establishes pending-email state, with AuthenticationManagement consumer tests able to start from the Phase 2 event contracts.
- US3 AuthenticationManagement consumer implementation T078 through T081 can run in parallel with CustomerDomain verification endpoint work T072 through T076 once event contracts are stable.
- Polish documentation T085/T086 and AppHost/container checks T087 through T090 can run in parallel before final `dotnet build src` and `dotnet test src`.

---

## Parallel Example: User Story 1

```bash
# CustomerDomain tests in parallel
Task: "T019 Add aggregate tests in src/CookieCommerce.CustomerDomain.Tests/DomainTests/CustomerAggregate_UpdateProfile_Tests.cs"
Task: "T021 Add projection tests in src/CookieCommerce.CustomerDomain.Tests/ProjectionsTests/CustomerProjection_ProfileUpdate_Tests.cs"
Task: "T023 Add email delivery handoff tests in src/CookieCommerce.CustomerDomain.Tests/ApplicationTests/EmailVerificationDeliveryHandoffTests.cs"
Task: "T024 Add API contract tests in src/CookieCommerce.CustomerDomain.Tests/IntegrationTests/CustomerProfileApiTests.cs"

# Web tests in parallel
Task: "T025 Add service tests in src/CookieCommerce.Web.Tests/Services/CustomerProfileServiceTests.cs"
Task: "T026 Add component tests in src/CookieCommerce.Web.Tests/Components/Account/ProfileComponentsTests.cs"

# Independent implementation work after tests/foundations
Task: "T027 Implement aggregate methods in src/CookieCommerce.CustomerDomain.Domain/Aggregates/CustomerAggregate.cs"
Task: "T033 Implement sender adapter in src/CookieCommerce.CustomerDomain.Infrastructure/Integration/ExternalEmailVerificationSender.cs"
Task: "T041 Implement Web service in src/CookieCommerce.Web/Services/CustomerProfileService.cs"
```

## Parallel Example: User Story 2

```bash
Task: "T052 Add domain validation tests in src/CookieCommerce.CustomerDomain.Tests/DomainTests/CustomerAggregate_ProfileValidation_Tests.cs"
Task: "T053 Add application validation tests in src/CookieCommerce.CustomerDomain.Tests/ApplicationTests/UpdateCustomerProfileValidationTests.cs"
Task: "T054 Add API validation tests in src/CookieCommerce.CustomerDomain.Tests/IntegrationTests/CustomerProfileValidationApiTests.cs"
Task: "T055 Add Web validation tests in src/CookieCommerce.Web.Tests/Services/CustomerProfileValidationTests.cs"
```

## Parallel Example: User Story 3

```bash
Task: "T064 Add token verification tests in src/CookieCommerce.CustomerDomain.Tests/ApplicationTests/VerifyCustomerEmailChangeTests.cs"
Task: "T066 Add outbox tests in src/CookieCommerce.CustomerDomain.Tests/IntegrationTests/CustomerProfileOutboxTests.cs"
Task: "T067 Add AuthenticationManagement verified-email consumer tests in src/CookieCommerce.AuthenticationManagement.Tests/ConsumerTests/VerifiedCustomerEmailChangedConsumerTests.cs"
Task: "T068 Add AuthenticationManagement forgotten-customer consumer tests in src/CookieCommerce.AuthenticationManagement.Tests/ConsumerTests/CustomerForgottenConsumerTests.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1 setup so Web and AuthenticationManagement tests are discoverable by `dotnet test src`.
2. Complete Phase 2 foundations, especially encrypted profile event storage for gender/date of birth, the email delivery handoff contracts, and the email uniqueness checker required before pending email creation.
3. Complete Phase 3 US1: live profile load/save, pending email request/resend, concrete delivery sender/adapter, DI registration, and Web mock removal.
4. **STOP and VALIDATE**: Run T051 and manually verify quickstart sections 4, 5, and 6.
5. Demo MVP if ready; old verified email remains the active profile/login email while pending email awaits US3 verification.

### Incremental Delivery

1. Setup + Foundational → runnable test foundations and stable contracts.
2. US1 → customer can update valid profile details and request/resend pending email verification.
3. US2 → invalid updates are safely rejected with field feedback and no state changes.
4. US3 → identity/privacy hardening, verification confirmation, async login promotion, and forgotten-customer cleanup.
5. Polish → documentation, container, Aspire, build, and full regression gates.

### Parallel Team Strategy

1. Team completes Setup + Foundational together.
2. After Phase 2:
   - Developer A: CustomerDomain US1 aggregate/handlers/projections/API.
   - Developer B: Web US1 service/components/mock removal.
   - Developer C: US2 validation tests/implementation.
   - Developer D: US3 AuthenticationManagement consumers and outbox tests.
3. Integrate at story checkpoints with targeted tests before full regression.

---

## Notes

- Gender and date of birth MUST follow the selected storage model: encrypted immutable CustomerDomain event storage using the customer-specific key, decrypted removable projection fields, and read-model deletion during GDPR erasure.
- CustomerDomain MUST provide the concrete email verification delivery handoff path: application contract, payload, infrastructure sender/adapter, DI registration, options, safe token/reference handling, correlation id, idempotency key, expiry, recipient pending email, purpose, and no raw-token/unnecessary-PII logging.
- US1 stops at pending email request/resend and old verified email remaining active; US3 owns token confirmation, verified profile email promotion, integration-event publication, and AuthenticationManagement login credential promotion.
- Do not introduce synchronous runtime HTTP business decisions between CustomerDomain and AuthenticationManagement.
- Do not add a same-project Web service interface for `CustomerProfileService`.
- Do not leave feature tests outside runnable test projects referenced by `src/CookieCommerce.slnx`.
- Do not use `FakeAccountService`, `StaticProfileList`, or mock fallbacks for customer-facing profile load/save/display.
