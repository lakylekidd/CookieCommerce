# Tasks: Backoffice Product Page Reuse and Status Deferral

**Input**: Design documents from `/specs/004-backoffice-product-page/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/backoffice-products-ui.md, quickstart.md

**Tests**: Tests are REQUIRED for this behavioral UI guardrail. Use focused `CookieCommerce.Web.Tests` coverage for route/page reuse and status non-effect. Do not add ProductDomain tests unless implementation changes ProductDomain behavior.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2)
- Include exact file paths in descriptions

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Confirm the existing Web UI surface and identify any duplicate product pages before story work begins.

- [X] T001 Inspect current product routes in `src/CookieCommerce.Web/Components/Backoffice/ProductsArea/Products.razor` and confirm `/backoffice/products/` plus `/backoffice/products/{ActiveProductId:guid}` remain the target routes
- [X] T002 [P] Search `src/CookieCommerce.Web/Components` for other backoffice product page routes and record that no duplicate workflow page should be introduced
- [X] T003 [P] Inspect current product workflow components in `src/CookieCommerce.Web/Components/Backoffice/ProductsArea/` to identify status controls and product entry points before editing

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Establish test seams and guardrails that MUST be in place before user story implementation.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

- [X] T004 Add Web test support for product page/component guardrails in `src/CookieCommerce.Web.Tests/Components/Backoffice/ProductsArea/Products_RouteReuse_Tests.cs`
- [X] T005 [P] Configure Moq-based `IBackofficeService` mocks for status-call detection in the relevant `src/CookieCommerce.Web.Tests/Components/Backoffice/ProductsArea/*_Tests.cs` files; do not add a custom recording test double
- [X] T006 [P] Verify no service interface changes are needed in `src/CookieCommerce.Web/Services/IBackofficeService.cs`; if an interface change becomes unavoidable, update `Documentation/backend-documentation.md`

**Checkpoint**: Foundation ready - user story implementation can now begin.

---

## Phase 3: User Story 1 - Use existing backoffice product page for product work (Priority: P1) 🎯 MVP

**Goal**: Product workflow remains on the existing backoffice products page with the current recognizable UI and no duplicate page.

**Independent Test**: Open `/backoffice/products/` and confirm the product workflow, products table/list, selected product area, and create-product workflow are available in the existing page layout.

### Tests for User Story 1 ⚠️

> **NOTE: Write required tests first when practical; ensure they fail for the intended reason before implementation.**

- [X] T007 [P] [US1] Add route guardrail tests for `/backoffice/products/` and `/backoffice/products/{ActiveProductId:guid}` in `src/CookieCommerce.Web.Tests/Components/Backoffice/ProductsArea/Products_RouteReuse_Tests.cs`
- [X] T008 [P] [US1] Add component structure tests for products table, selected product area, and create modal presence in `src/CookieCommerce.Web.Tests/Components/Backoffice/ProductsArea/Products_ComponentStructure_Tests.cs`

### Implementation for User Story 1

- [X] T009 [US1] Preserve existing routes, `BackofficeLayout`, `InventoryTable`, `SelectedInventoryItem`, and `CreateNewProductModal` usage in `src/CookieCommerce.Web/Components/Backoffice/ProductsArea/Products.razor`
- [X] T010 [US1] Remove or avoid any duplicate product workflow route/page found under `src/CookieCommerce.Web/Components` while keeping product work on `src/CookieCommerce.Web/Components/Backoffice/ProductsArea/Products.razor`
- [X] T011 [US1] Verify create-product callbacks still refresh the existing products page in `src/CookieCommerce.Web/Components/Backoffice/ProductsArea/Products.razor`

**Checkpoint**: User Story 1 should be fully functional and testable independently.

---

## Phase 4: User Story 2 - Keep product status control non-functional for now (Priority: P2)

**Goal**: Existing status radio/button/toggle remains visible in place but does not save, validate, filter, publish, or otherwise affect product behavior.

**Independent Test**: Interact with the visible status control, complete or refresh the product workflow, and confirm no status-specific save call, validation message, success message, or product behavior change occurs.

### Tests for User Story 2 ⚠️

- [X] T012 [P] [US2] Add a component test proving variant status control remains rendered in `src/CookieCommerce.Web.Tests/Components/Backoffice/ProductsArea/SelectedInventoryVariant_StatusRendering_Tests.cs`
- [X] T013 [P] [US2] Add a component test using Moq verification to prove status interaction does not call `IBackofficeService.ChangeVariantStatusAsync` in `src/CookieCommerce.Web.Tests/Components/Backoffice/ProductsArea/SelectedInventoryVariant_StatusInteraction_Tests.cs`
- [X] T014 [P] [US2] Add a service-level regression test proving `FakeBackofficeService.CreateProductAsync` does not derive saved status from status UI input in `src/CookieCommerce.Web.Tests/Services/FakeBackofficeService_CreateProductAsync_Tests.cs`

### Implementation for User Story 2

- [X] T015 [US2] Make the visible variant status checkbox/toggle inert without changing its location or visual style in `src/CookieCommerce.Web/Components/Backoffice/ProductsArea/SelectedInventoryVariant.razor`
- [X] T016 [US2] Remove the status save call from `ToggleActive` or its replacement in `src/CookieCommerce.Web/Components/Backoffice/ProductsArea/SelectedInventoryVariant.razor`
- [X] T017 [US2] Ensure status interaction does not show status-saved confirmations or status-specific validation in `src/CookieCommerce.Web/Components/Backoffice/ProductsArea/SelectedInventoryVariant.razor`
- [X] T018 [US2] Confirm product creation does not add status fields or status validation in `src/CookieCommerce.Web/Components/Backoffice/ProductsArea/CreateProductModalArea/CreateNewProductModal.razor`
- [X] T019 [US2] Leave `IBackofficeService.ChangeProductStatusAsync` and `IBackofficeService.ChangeVariantStatusAsync` unused by this feature in `src/CookieCommerce.Web/Services/IBackofficeService.cs` without expanding the service contract

**Checkpoint**: User Stories 1 and 2 should both work independently.

---

## Phase 5: Polish & Cross-Cutting Concerns

**Purpose**: Validate feature behavior and clean up cross-cutting concerns.

- [X] T020 [P] Review accessibility of the inert status control in `src/CookieCommerce.Web/Components/Backoffice/ProductsArea/SelectedInventoryVariant.razor`
- [X] T021 [P] Update feature notes if implementation details changed in `specs/004-backoffice-product-page/quickstart.md`
- [X] T022 Run focused Web tests with `dotnet test src/CookieCommerce.Web.Tests` for `src/CookieCommerce.Web.Tests/CookieCommerce.Web.Tests.csproj`
- [X] T023 Run build validation with `dotnet build src` for `src/`
- [X] T024 Run formatting validation with `dotnet format --verify-no-changes` before merge
- [X] T025 Manually execute quickstart scenarios for existing page reuse and status non-effect from `specs/004-backoffice-product-page/quickstart.md`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3+)**: Depend on Foundational phase completion
- **Polish (Phase 5)**: Depends on all desired user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational - no dependency on User Story 2
- **User Story 2 (P2)**: Can start after Foundational - can be implemented independently, but should be verified against the existing page preserved by User Story 1

### Within Each User Story

- Tests should be written before or alongside implementation and should fail for the intended reason when test-first work is practical
- Keep UI behavior in existing `ProductsArea` components
- Do not introduce ProductDomain status commands, events, projections, API endpoints, or integration events
- Do not update `Documentation/backend-documentation.md` unless a `src/CookieCommerce.Web/Services/**/*.cs` interface changes

## Parallel Opportunities

- T002 and T003 can run in parallel during setup
- T005 and T006 can run in parallel after T004 is started
- T007 and T008 can run in parallel for User Story 1 tests
- T012, T013, and T014 can run in parallel for User Story 2 tests
- T020 and T021 can run in parallel during polish

## Parallel Example: User Story 1

```bash
# Launch independent User Story 1 guardrail test tasks together:
Task: "Add route guardrail tests for `/backoffice/products/` and `/backoffice/products/{ActiveProductId:guid}` in `src/CookieCommerce.Web.Tests/Components/Backoffice/ProductsArea/Products_RouteReuse_Tests.cs`"
Task: "Add component structure tests for products table, selected product area, and create modal presence in `src/CookieCommerce.Web.Tests/Components/Backoffice/ProductsArea/Products_ComponentStructure_Tests.cs`"
```

## Parallel Example: User Story 2

```bash
# Launch independent User Story 2 tests together:
Task: "Add a component test proving variant status control remains rendered in `src/CookieCommerce.Web.Tests/Components/Backoffice/ProductsArea/SelectedInventoryVariant_StatusRendering_Tests.cs`"
Task: "Add a component test using Moq verification to prove status interaction does not call `IBackofficeService.ChangeVariantStatusAsync` in `src/CookieCommerce.Web.Tests/Components/Backoffice/ProductsArea/SelectedInventoryVariant_StatusInteraction_Tests.cs`"
Task: "Add a service-level regression test proving `FakeBackofficeService.CreateProductAsync` does not derive saved status from status UI input in `src/CookieCommerce.Web.Tests/Services/FakeBackofficeService_CreateProductAsync_Tests.cs`"
```

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational
3. Complete Phase 3: User Story 1
4. **STOP and VALIDATE**: Open `/backoffice/products/` and run User Story 1 tests
5. Demo existing-page workflow reuse before status deferral changes

### Incremental Delivery

1. Complete Setup + Foundational → guardrails ready
2. Add User Story 1 → product work remains on existing page → test independently
3. Add User Story 2 → status remains visible but inert → test independently
4. Complete Polish → run quickstart, focused Web tests, build, and formatting validation

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together
2. Developer A completes User Story 1 page reuse tests and UI guardrails
3. Developer B completes User Story 2 status non-effect tests and UI changes
4. Integrate by running `dotnet test src/CookieCommerce.Web.Tests`, `dotnet build src`, and `dotnet format --verify-no-changes`

## Notes

- [P] tasks = different files, no dependencies on incomplete tasks
- [US1] and [US2] labels map tasks to user stories in `specs/004-backoffice-product-page/spec.md`
- Keep status visually present and recognizable; do not hide or remove it unless the smallest accessibility-preserving inert behavior requires a minor markup adjustment
- Status must not save, update, validate, filter, publish, or affect ProductDomain behavior in this feature
- Avoid adding new backoffice product pages, backend status endpoints, ProductDomain status events, projections, integration events, Aspire topology, or container changes
