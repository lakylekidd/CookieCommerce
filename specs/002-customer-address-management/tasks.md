# Tasks: Customer Address Management

**Input**: Design documents from `/specs/002-customer-address-management/`

**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/customer-address-api.md`, `quickstart.md`

**Tests**: Required by the feature specification and constitution for domain behavior, Marten projections, API contracts, and Web service integration. Write tests before or alongside implementation and verify they fail for the intended reason when practical.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel because it touches different files and does not depend on incomplete tasks in the same phase.
- **[Story]**: User story label for story phases only (`[US1]`, `[US2]`, `[US3]`).
- Every task includes an exact file path.

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Confirm existing seams and add shared address contract scaffolding without changing behavior.

- [ ] T001 Audit current fake and CustomerDomain address flows in `src/CookieCommerce.Web/Services/FakeAccountService.cs`, `src/CookieCommerce.CustomerDomain.Api/Controllers/CustomerController.cs`, and `src/CookieCommerce.CustomerDomain.Domain/Aggregates/CustomerAggregate.cs`
- [ ] T002 [P] Add stable address identity to the Web address DTO in `src/CookieCommerce.Web/Models/Addresses/AddressBookItemDto.cs`
- [ ] T003 [P] Create CustomerDomain address API response DTO in `src/CookieCommerce.CustomerDomain.Api/Models/Customers/CustomerAddressResponse.cs`
- [ ] T004 [P] Create CustomerDomain address mapping extension skeleton in `src/CookieCommerce.CustomerDomain.Api/Models/Customers/CustomerAddressMappings.cs`
- [ ] T005 [P] Create Web CustomerDomain address client model skeletons in `src/CookieCommerce.Web/Models/Addresses/CustomerAddressApiModels.cs`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core address model, validation, encryption, projection, and service seams that all user stories depend on.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

- [ ] T006 Update CustomerDomain `Address` value object with address id, icon, label, first name, last name, street, house number, house-number addition, city, postal code, province, country, phone, default flag, and created timestamp in `src/CookieCommerce.CustomerDomain.Domain/ValueObjects/Address.cs`
- [ ] T007 [P] Add address-specific domain exceptions for validation, duplicate label, and missing address id behavior in `src/CookieCommerce.CustomerDomain.Domain/Exceptions/CustomerAddressExceptions.cs`
- [ ] T008 Update `CustomerAddressAdded` to store encrypted address PII fields and non-PII metadata instead of plaintext value-object payloads in `src/CookieCommerce.CustomerDomain.Domain/DomainEvents/CustomerAddressAdded.cs`
- [ ] T009 Update `CustomerAddressRemoved` to remove by address id and include deletion timestamp metadata in `src/CookieCommerce.CustomerDomain.Domain/DomainEvents/CustomerAddressRemoved.cs`
- [ ] T010 Add address encryption/decryption helpers using existing customer key infrastructure in `src/CookieCommerce.CustomerDomain.Application/Customers/Profile/CustomerAddressEncryption.cs`
- [ ] T011 Add reusable address request validation for required fields, trimming, length bounds, duplicate labels, and phone/postal-code presence in `src/CookieCommerce.CustomerDomain.Application/Customers/Profile/CustomerAddressValidator.cs`
- [ ] T012 Update `CustomerAddressReadModel` with the full customer-facing address shape and default-address fields in `src/CookieCommerce.CustomerDomain.Application/Customers/ReadModels/CustomerAddressReadModel.cs`
- [ ] T013 Update `CustomerProjection` to decrypt address-added events, apply deterministic indexes, maintain default-address state, and delete address read models on customer erasure in `src/CookieCommerce.CustomerDomain.Infrastructure/Projections/CustomerProjection.cs`

**Checkpoint**: Foundation ready — user story implementation can now begin.

---

## Phase 3: User Story 1 - View saved addresses (Priority: P1) 🎯 MVP

**Goal**: Authenticated customers can view only their own active saved addresses in profile and checkout, with empty-state behavior when none exist.

**Independent Test**: Sign in as a customer with existing saved addresses and confirm profile or checkout lists only that customer's current addresses; sign in as a customer with no addresses and confirm an empty state with no fake/static fallback.

### Tests for User Story 1 ⚠️

- [ ] T014 [P] [US1] Add aggregate tests for replaying address-added events and enforcing one default active address in `src/CookieCommerce.CustomerDomain.Tests/DomainTests/CustomerAggregate_AddressView_Tests.cs`
- [ ] T015 [P] [US1] Add projection tests for decrypted address read models, empty address books, ownership isolation, and erasure removal in `src/CookieCommerce.CustomerDomain.Tests/ProjectionsTests/CustomerAddressProjectionTests.cs`
- [ ] T016 [P] [US1] Add current-customer address API contract tests for `GET /api/customer/me/addresses` and `GET /api/customer/me/addresses/default` in `src/CookieCommerce.CustomerDomain.Tests/IntegrationTests/CurrentCustomerAddressApiTests.cs`
- [ ] T017 [P] [US1] Add Web service tests proving address reads call CustomerDomain and do not fall back to fake address data in `src/CookieCommerce.Web.Tests/Services/CustomerAddressServiceTests.cs`

### Implementation for User Story 1

- [ ] T018 [US1] Update `CustomerAggregate` address add/replay behavior to preserve active addresses, address ids, and default state from events in `src/CookieCommerce.CustomerDomain.Domain/Aggregates/CustomerAggregate.cs`
- [ ] T019 [US1] Create current-customer address query messages for all addresses and default address in `src/CookieCommerce.CustomerDomain.Application/Customers/Queries/GetCurrentCustomerAddressesQuery.cs`
- [ ] T020 [US1] Implement query handlers that read `CustomerReadModel`, return only active addresses for the resolved customer, and handle missing/forgotten customers in `src/CookieCommerce.CustomerDomain.Application/Customers/Handlers/GetCurrentCustomerAddressesQueryHandler.cs`
- [ ] T021 [US1] Implement `GET /api/customer/me/addresses` and `GET /api/customer/me/addresses/default` thin controller actions in `src/CookieCommerce.CustomerDomain.Api/Controllers/CustomerController.cs`
- [ ] T022 [US1] Complete address response mappings from read models to API DTOs in `src/CookieCommerce.CustomerDomain.Api/Models/Customers/CustomerAddressMappings.cs`
- [ ] T023 [US1] Create a live `IAccountService` implementation or adapter for CustomerDomain-backed address reads while leaving non-address fake behavior unchanged in `src/CookieCommerce.Web/Services/CustomerDomainAccountService.cs`, and update `Documentation/backend-documentation.md` for the changed Web service read behavior
- [ ] T024 [US1] Register the address-backed account service/decorator with the existing `customer-domain-api` HttpClient in `src/CookieCommerce.Web/Program.cs`
- [ ] T025 [US1] Update account address and default-shipping components to handle empty/error states without fake fallback in `src/CookieCommerce.Web/Components/Account/Addresses.razor` and `src/CookieCommerce.Web/Components/Account/DefaultShippingCard.razor`
- [ ] T026 [US1] Update checkout address selection to consume live address ids, preserve deterministic index display, and show empty/error states in `src/CookieCommerce.Web/Components/Storefront/Checkout/ContactAndShipping.razor`
- [ ] T027 [US1] Create a dedicated dev-bootstrap library project for fake-user and fake-address bootstrap orchestration in `src/CookieCommerce.DevBootstrap/CookieCommerce.DevBootstrap.csproj` and wire it into the solution
- [ ] T028 [US1] Move fake-user bootstrap orchestration out of Keycloak code into the dev-bootstrap library, coordinating identity creation/sync and CustomerDomain address seeding in `src/CookieCommerce.DevBootstrap/DevelopmentBootstrapService.cs`
- [ ] T029 [US1] Limit Keycloak bootstrap code to identity user creation/synchronization adapters with no address ownership or CustomerDomain orchestration in `src/CookieCommerce.AuthenticationManagement.Infrastructure/Keycloak/KeycloakBootstrapService.cs`

**Checkpoint**: User Story 1 is fully functional and testable independently.

---

## Phase 4: User Story 2 - Add a saved address (Priority: P2)

**Goal**: Authenticated customers can submit a complete valid address, receive validation feedback for invalid input, and see the saved address in subsequent profile and checkout reads.

**Independent Test**: Sign in as a customer, submit a valid new address, and confirm it appears in that customer's address list after save; submit invalid fields and confirm no address-book mutation.

### Tests for User Story 2 ⚠️

- [ ] T030 [P] [US2] Add aggregate tests for first-address default, requested default replacing previous default, duplicate labels, and validation failures in `src/CookieCommerce.CustomerDomain.Tests/DomainTests/CustomerAggregate_AddAddress_Tests.cs`
- [ ] T031 [P] [US2] Add handler tests for encrypting address PII, rejecting invalid requests without appending events, and saving valid address events in `src/CookieCommerce.CustomerDomain.Tests/ApplicationTests/AddCustomerAddressHandlerTests.cs`
- [ ] T032 [P] [US2] Add API contract tests for `POST /api/customer/me/addresses` success, validation problem, conflict, unauthorized, and not-found outcomes in `src/CookieCommerce.CustomerDomain.Tests/IntegrationTests/AddCurrentCustomerAddressApiTests.cs`
- [ ] T033 [P] [US2] Add Web service tests for `AddAddressAsync` mapping, field errors, conflict handling, refresh after success, and no fake mutation in `src/CookieCommerce.Web.Tests/Services/CustomerAddressAddServiceTests.cs`

### Implementation for User Story 2

- [ ] T034 [US2] Replace `AddCustomerAddress` command with the full address request shape, generated address id, created timestamp, default flag, and cancellation-safe naming in `src/CookieCommerce.CustomerDomain.Application/Customers/Commands/AddCustomerAddress.cs`
- [ ] T035 [US2] Update `AddCustomerAddressHandler` to validate input, load the customer stream, encrypt address PII using the customer key, append address-added events, and save with `CancellationToken` in `src/CookieCommerce.CustomerDomain.Application/Customers/Handlers/AddCustomerAddressHandler.cs`
- [ ] T036 [US2] Update `CustomerAggregate.AddAddress` to enforce required domain invariants, duplicate-label rejection, first-address default, and only-one-default behavior in `src/CookieCommerce.CustomerDomain.Domain/Aggregates/CustomerAggregate.cs`
- [ ] T037 [US2] Expand `AddCustomerAddressRequest` with icon, recipient, house-number, province, phone, and `isDefault` fields using request DTO suffix conventions in `src/CookieCommerce.CustomerDomain.Api/Models/Customers/AddCustomerAddressRequest.cs`
- [ ] T038 [US2] Add request-to-command and result-to-response mappings for address creation in `src/CookieCommerce.CustomerDomain.Api/Models/Customers/CustomerAddressMappings.cs`
- [ ] T039 [US2] Implement `POST /api/customer/me/addresses` with `201 Created`, validation, conflict, unauthorized, and not-found outcomes in `src/CookieCommerce.CustomerDomain.Api/Controllers/CustomerController.cs`
- [ ] T040 [US2] Update domain exception middleware to map address validation and duplicate-label exceptions to user-safe ProblemDetails without PII in `src/CookieCommerce.CustomerDomain.Api/Middleware/DomainExceptionMiddleware.cs`
- [ ] T041 [US2] Implement Web `AddAddressAsync` to call CustomerDomain, map validation errors into `ResultDto.FieldErrors`, refresh address state, avoid fake profile mutation in `src/CookieCommerce.Web/Services/CustomerDomainAccountService.cs`, and update `Documentation/backend-documentation.md` for the changed Web service add-address behavior
- [ ] T042 [US2] Update add-address modal validation display so field-level errors from CustomerDomain are visible and no PII is logged in `src/CookieCommerce.Web/Components/Account/AddAddressModal.razor`
- [ ] T043 [US2] Refresh checkout/account address lists immediately after successful adds in `src/CookieCommerce.Web/Components/Storefront/Checkout/ContactAndShipping.razor` and `src/CookieCommerce.Web/Components/Account/Addresses.razor`

**Checkpoint**: User Stories 1 and 2 both work independently.

---

## Phase 5: User Story 3 - Delete a saved address (Priority: P3)

**Goal**: Authenticated customers can delete their own active addresses; deleting defaults preserves valid default state and deleting missing/foreign addresses is safe.

**Independent Test**: Sign in as a customer with at least two saved addresses, delete one address, and confirm it no longer appears while remaining addresses and default state are correct.

### Tests for User Story 3 ⚠️

- [ ] T044 [P] [US3] Add aggregate tests for deleting non-default, deleting default with promotion, deleting only address, and repeated/missing deletes in `src/CookieCommerce.CustomerDomain.Tests/DomainTests/CustomerAggregate_DeleteAddress_Tests.cs`
- [ ] T045 [P] [US3] Add handler/projection tests for remove events, default promotion in read models, and no changes for missing address ids in `src/CookieCommerce.CustomerDomain.Tests/ApplicationTests/RemoveCustomerAddressHandlerTests.cs`
- [ ] T046 [P] [US3] Add API contract tests for `DELETE /api/customer/me/addresses/{addressId}` success, malformed id, not found, unauthorized, and ownership isolation in `src/CookieCommerce.CustomerDomain.Tests/IntegrationTests/DeleteCurrentCustomerAddressApiTests.cs`
- [ ] T047 [P] [US3] Add Web service tests for `DeleteAddressAsync` using stable address ids, refresh after success, and no fake mutation in `src/CookieCommerce.Web.Tests/Services/CustomerAddressDeleteServiceTests.cs`

### Implementation for User Story 3

- [ ] T048 [US3] Replace `RemoveCustomerAddress` label-based command with address-id-based deletion metadata in `src/CookieCommerce.CustomerDomain.Application/Customers/Commands/RemoveCustomerAddress.cs`
- [ ] T049 [US3] Update `RemoveCustomerAddressHandler` to load the current customer stream, remove only owned active addresses by id, append remove events, and return not-found outcomes safely in `src/CookieCommerce.CustomerDomain.Application/Customers/Handlers/RemoveCustomerAddressHandler.cs`
- [ ] T050 [US3] Update `CustomerAggregate.RemoveAddress` to remove by address id, promote the oldest remaining active address when deleting the default, and leave zero defaults when empty in `src/CookieCommerce.CustomerDomain.Domain/Aggregates/CustomerAggregate.cs`
- [ ] T051 [US3] Implement `DELETE /api/customer/me/addresses/{addressId}` and retire customer-facing use of legacy label/id routes in `src/CookieCommerce.CustomerDomain.Api/Controllers/CustomerController.cs`
- [ ] T052 [US3] Update `IAccountService` and Web `DeleteAddressAsync` compatibility behavior to use stable ids in `src/CookieCommerce.Web/Services/IAccountService.cs` and `src/CookieCommerce.Web/Services/CustomerDomainAccountService.cs`, and update `Documentation/backend-documentation.md` for the changed Web service delete-address behavior
- [ ] T053 [US3] Update `SetAsPrimaryAsync` compatibility behavior to clearly return unsupported for explicit set-primary outside scope in `src/CookieCommerce.Web/Services/IAccountService.cs` and `src/CookieCommerce.Web/Services/CustomerDomainAccountService.cs`, and document the unsupported behavior in `Documentation/backend-documentation.md`
- [ ] T054 [US3] Update address cards to pass stable address ids for delete actions and refresh after deletion in `src/CookieCommerce.Web/Components/Account/AddressCard.razor`
- [ ] T055 [US3] Update checkout selected-address clearing after deleting a selected/default address in `src/CookieCommerce.Web/Components/Storefront/Checkout/ContactAndShipping.razor`

**Checkpoint**: All user stories are independently functional.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Validation, documentation, privacy hardening, and repo-wide quality gates.

- [ ] T056 [P] Update feature quickstart notes with any final endpoint response choices, especially no-default behavior, in `specs/002-customer-address-management/quickstart.md`
- [ ] T057 Audit static/fake address references to ensure customer-facing account and checkout flows no longer read fake addresses in `src/CookieCommerce.Web/Services/FakeAccountService.cs` and `src/CookieCommerce.Web/Services/StaticData/StaticProfileList.cs`
- [ ] T058 Verify PII handling by ensuring address event payloads are encrypted, logs omit address values, and erasure deletes decrypted projections in `src/CookieCommerce.CustomerDomain.Domain/DomainEvents/CustomerAddressAdded.cs` and `src/CookieCommerce.CustomerDomain.Infrastructure/Projections/CustomerProjection.cs`
- [ ] T059 Run focused CustomerDomain and Web tests for address behavior with `dotnet test src/CookieCommerce.CustomerDomain.Tests/CookieCommerce.CustomerDomain.Tests.csproj` and `dotnet test src/CookieCommerce.Web.Tests/CookieCommerce.Web.Tests.csproj`
- [ ] T060 Run full build and test validation with `dotnet build src` and `dotnet test src`
- [ ] T061 Run formatting validation with `dotnet format src --verify-no-changes`
- [ ] T062 Verify Aspire composition still starts Web and CustomerDomain using `dotnet run --project src/CookieCommerce.AppHost`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies; start immediately.
- **Foundational (Phase 2)**: Depends on Setup; blocks all user stories.
- **User Stories (Phase 3+)**: Depend on Foundational completion.
- **Polish (Phase 6)**: Depends on all desired user stories being complete.

### User Story Dependencies

- **User Story 1 (P1)**: Starts after Foundational; MVP scope and no dependency on US2/US3.
- **User Story 2 (P2)**: Starts after Foundational; depends conceptually on the read model/API shape from US1 for post-add visibility.
- **User Story 3 (P3)**: Starts after Foundational; depends on stable address ids and default-state model from US1/US2.

### Within Each User Story

- Tests should be written before or alongside implementation.
- Domain model and events before handlers.
- Handlers before API endpoints.
- API contracts before Web service integration.
- Web service integration before component updates.
- Story must pass its independent test before moving to the next priority story.

### Parallel Opportunities

- Setup skeleton tasks T002–T005 can run in parallel.
- Foundational exception, event, validation, and read-model tasks T007–T012 can run in parallel after T006 is understood, then T013 integrates them.
- Test tasks at the start of each user story can run in parallel.
- API DTO/mapping work can run in parallel with Web client model work when contracts are stable.
- Different user stories can be staffed in parallel after Phase 2, but sequential P1 → P2 → P3 is lower risk because add/delete rely on the read model and stable ids.

---

## Parallel Example: User Story 1

```bash
Task: "Add aggregate tests in src/CookieCommerce.CustomerDomain.Tests/DomainTests/CustomerAggregateAddressViewTests.cs"
Task: "Add projection tests in src/CookieCommerce.CustomerDomain.Tests/ProjectionsTests/CustomerAddressProjectionTests.cs"
Task: "Add API contract tests in src/CookieCommerce.CustomerDomain.Tests/IntegrationTests/CurrentCustomerAddressApiTests.cs"
Task: "Add Web service tests in src/CookieCommerce.Web.Tests/Services/CustomerAddressServiceTests.cs"
```

## Parallel Example: User Story 2

```bash
Task: "Add aggregate add-address tests in src/CookieCommerce.CustomerDomain.Tests/DomainTests/CustomerAggregateAddAddressTests.cs"
Task: "Add handler add-address tests in src/CookieCommerce.CustomerDomain.Tests/ApplicationTests/AddCustomerAddressHandlerTests.cs"
Task: "Add API add-address tests in src/CookieCommerce.CustomerDomain.Tests/IntegrationTests/AddCurrentCustomerAddressApiTests.cs"
Task: "Add Web add-address tests in src/CookieCommerce.Web.Tests/Services/CustomerAddressAddServiceTests.cs"
```

## Parallel Example: User Story 3

```bash
Task: "Add aggregate delete-address tests in src/CookieCommerce.CustomerDomain.Tests/DomainTests/CustomerAggregateDeleteAddressTests.cs"
Task: "Add handler/projection delete tests in src/CookieCommerce.CustomerDomain.Tests/ApplicationTests/RemoveCustomerAddressHandlerTests.cs"
Task: "Add API delete-address tests in src/CookieCommerce.CustomerDomain.Tests/IntegrationTests/DeleteCurrentCustomerAddressApiTests.cs"
Task: "Add Web delete-address tests in src/CookieCommerce.Web.Tests/Services/CustomerAddressDeleteServiceTests.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup.
2. Complete Phase 2: Foundational.
3. Complete Phase 3: User Story 1.
4. Stop and validate `GET /api/customer/me/addresses`, account address book, checkout address selector, empty-state behavior, and no fake fallback.
5. Demo/deploy MVP if view-only address management is acceptable.

### Incremental Delivery

1. Setup + Foundation: shared full address shape, encrypted events, read model, and Web DTO/client seams.
2. US1: read live addresses from CustomerDomain in account and checkout.
3. US2: add valid addresses with field-level validation and default rules.
4. US3: delete addresses with default promotion and safe not-found behavior.
5. Polish: documentation, privacy audit, formatting, full tests, and Aspire validation.

### Parallel Team Strategy

1. Team completes Setup + Foundational work together.
2. After Phase 2, split by story tests and slices:
   - Developer A: US1 read/query/API/Web view slice.
   - Developer B: US2 add/validation/encryption slice.
   - Developer C: US3 delete/default-promotion slice.
3. Re-integrate through `CustomerProjection`, `CustomerDomainAccountService`, and full `dotnet test src` validation.

---

## Notes

- Do not store plaintext address PII in immutable CustomerDomain events.
- Do not add synchronous runtime calls between bounded contexts for business decisions.
- Keep controllers thin; business/default-address rules belong in domain/application layers.
- Keep Web components using `src/CookieCommerce.Web/Services` instead of direct backend calls.
- Preserve existing fake account behavior for favorites, orders, tracking, and reviews unless an address task explicitly changes it.
- Stop and resolve any task that requires exposing internal aggregate/event types through API or Web DTOs.
