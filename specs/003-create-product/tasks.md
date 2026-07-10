# Tasks: Backoffice Create Product Button MVP

**Input**: Design documents from `/specs/003-create-product/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/backoffice-products-ui.md, quickstart.md

**Tests**: Include focused Web service tests for behavioral changes where practical. Component spinner behavior is validated by quickstart/manual UI because this project does not currently include a Blazor component test framework dependency.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1, US2)
- Include exact file paths in descriptions

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Confirm the existing Web/fake-data slice and keep scope narrow before implementation.

- [ ] T001 Inspect current Products page, create modal, fake backoffice service, and static inventory source in src/CookieCommerce.Web/Components/Backoffice/ProductsArea/Products.razor, src/CookieCommerce.Web/Components/Backoffice/ProductsArea/CreateProductModalArea/CreateNewProductModal.razor, src/CookieCommerce.Web/Services/FakeBackofficeService.cs, and src/CookieCommerce.Web/Services/Products/StaticInventoryList.cs
- [ ] T002 [P] Inspect existing Web test conventions in src/CookieCommerce.Web.Tests/CookieCommerce.Web.Tests.csproj and src/CookieCommerce.Web.Tests/Services/

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Establish shared validation/data assumptions before story work.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

- [ ] T003 Add required-field validation attributes for ProductName and ProductReference in src/CookieCommerce.Web/Models/Products/CreateProductDto.cs
- [ ] T004 [P] Confirm existing fake products expose non-empty ProductName, ProductReference, Stock, Active, and image/placeholder-compatible data in src/CookieCommerce.Web/Services/Products/StaticInventoryList.cs

**Checkpoint**: Foundation ready - user story implementation can now begin.

---

## Phase 3: User Story 1 - Create a Product from Backoffice (Priority: P1) 🎯 MVP

**Goal**: A backoffice user can create a product from the Products page, see a spinner while saving, and see the new product appear in the table with zero stock.

**Independent Test**: Navigate to `/backoffice/products/`, click `New Product`, enter valid product name/reference, submit, observe spinner during save, and confirm the new row appears without browser reload with `0 Qty`.

### Tests for User Story 1

- [ ] T005 [P] [US1] Add FakeBackofficeService create-product test that appends one ProductDto with ProductName, ProductReference, Active false, and Stock 0 in src/CookieCommerce.Web.Tests/Services/FakeBackofficeServiceCreateProductTests.cs
- [ ] T006 [P] [US1] Add CreateProductDto validation test for missing ProductName and ProductReference in src/CookieCommerce.Web.Tests/Services/CreateProductDtoValidationTests.cs

### Implementation for User Story 1

- [ ] T007 [US1] Update CreateProductAsync to append a new ProductDto with generated ProductId, provided ProductName/ProductReference, Stock 0, Active false, no main image, and retained default variants in src/CookieCommerce.Web/Services/FakeBackofficeService.cs
- [ ] T008 [US1] Replace OnProductCreated="StateHasChanged" with an async callback that awaits RefreshInventory after successful create in src/CookieCommerce.Web/Components/Backoffice/ProductsArea/Products.razor
- [ ] T009 [US1] Add DataAnnotationsValidator, ValidationSummary or field-level ValidationMessage components, and accessible labels/errors to the create form in src/CookieCommerce.Web/Components/Backoffice/ProductsArea/CreateProductModalArea/CreateNewProductModal.razor
- [ ] T010 [US1] Add IsSaving state, guarded duplicate-submit logic, disabled close/submit controls during save, and a visible spinner with status text in src/CookieCommerce.Web/Components/Backoffice/ProductsArea/CreateProductModalArea/CreateNewProductModal.razor
- [ ] T011 [US1] Add failure handling that preserves entered CreateProductDto data, stops IsSaving, and shows a plain-language retry message in src/CookieCommerce.Web/Components/Backoffice/ProductsArea/CreateProductModalArea/CreateNewProductModal.razor

**Checkpoint**: User Story 1 is functional and independently testable.

---

## Phase 4: User Story 2 - Display Existing Fake Products with Fake Stocks (Priority: P2)

**Goal**: The Products table displays all existing fake products with their fake stock values and continues to display newly created products consistently.

**Independent Test**: Navigate to `/backoffice/products/` in a fresh run and confirm every item from `StaticInventoryList.InventoryItems` appears with product name, reference, stock quantity, status, and image/placeholder.

### Tests for User Story 2

- [ ] T012 [P] [US2] Add FakeBackofficeService inventory-list test that GetInventoryItemsAsync returns all StaticInventoryList.InventoryItems with non-empty names/references and stock values in src/CookieCommerce.Web.Tests/Services/FakeBackofficeServiceInventoryTests.cs

### Implementation for User Story 2

- [ ] T013 [US2] Ensure InventoryTable renders a clear empty state only when InventoryItems is empty and does not hide populated fake products after loading in src/CookieCommerce.Web/Components/Backoffice/ProductsArea/InventoryTable.razor
- [ ] T014 [US2] Ensure InventoryTableItem displays zero stock correctly with the low-stock indicator, product reference, active/draft status, and tiger placeholder for null ProductMainImage in src/CookieCommerce.Web/Components/Backoffice/ProductsArea/InventoryTableItem.razor

**Checkpoint**: User Stories 1 and 2 both work independently.

---

## Phase 5: Polish & Cross-Cutting Concerns

**Purpose**: Validate the MVP and update docs only if implementation scope requires it.

- [ ] T015 [P] Update Documentation/backend-documentation.md only if IBackofficeService signatures changed in src/CookieCommerce.Web/Services/IBackofficeService.cs
- [ ] T016 Run Web tests with `dotnet test src/CookieCommerce.Web.Tests` from C:\Users\willi\source\Repos\CookieCommerce and fix failures in src/CookieCommerce.Web.Tests/
- [ ] T017 Run build with `dotnet build src` from C:\Users\willi\source\Repos\CookieCommerce and fix compile errors in src/CookieCommerce.Web/
- [ ] T018 Validate quickstart scenarios for existing fake products, create spinner, zero-stock new row, and validation errors using specs/003-create-product/quickstart.md

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately.
- **Foundational (Phase 2)**: Depends on Setup completion - blocks all user stories.
- **User Story 1 (Phase 3)**: Depends on Foundational - MVP scope.
- **User Story 2 (Phase 4)**: Depends on Foundational; can run after or alongside US1 except for validating newly created rows.
- **Polish (Phase 5)**: Depends on desired user stories being complete.

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational - no dependency on US2.
- **User Story 2 (P2)**: Can start after Foundational - table display is independently testable with existing static inventory; final consistency check benefits from US1.

### Within Each User Story

- Tests should be written before or alongside implementation when practical.
- Model validation before form validation.
- Service behavior before page refresh integration.
- UI spinner/error handling before quickstart validation.

---

## Parallel Opportunities

- T002 and T004 are independent inspection/confirmation tasks.
- T005 and T006 can run in parallel because they create different test files.
- T012 can run in parallel with US1 tests after Foundational is complete.
- T013 and T014 affect different components and can run in parallel after table contract is understood.
- T015 can run in parallel with validation tasks if and only if `IBackofficeService` signatures changed.

---

## Parallel Example: User Story 1

```bash
# Launch tests together:
Task: "Add FakeBackofficeService create-product test in src/CookieCommerce.Web.Tests/Services/FakeBackofficeServiceCreateProductTests.cs"
Task: "Add CreateProductDto validation test in src/CookieCommerce.Web.Tests/Services/CreateProductDtoValidationTests.cs"

# Then implement service and UI in sequence:
Task: "Update CreateProductAsync in src/CookieCommerce.Web/Services/FakeBackofficeService.cs"
Task: "Refresh inventory after create in src/CookieCommerce.Web/Components/Backoffice/ProductsArea/Products.razor"
Task: "Add validation/spinner/failure handling in src/CookieCommerce.Web/Components/Backoffice/ProductsArea/CreateProductModalArea/CreateNewProductModal.razor"
```

## Parallel Example: User Story 2

```bash
# After Foundational, table component work can split:
Task: "Add FakeBackofficeService inventory-list test in src/CookieCommerce.Web.Tests/Services/FakeBackofficeServiceInventoryTests.cs"
Task: "Ensure InventoryTable renders loaded fake products in src/CookieCommerce.Web/Components/Backoffice/ProductsArea/InventoryTable.razor"
Task: "Ensure InventoryTableItem renders zero stock and placeholders in src/CookieCommerce.Web/Components/Backoffice/ProductsArea/InventoryTableItem.razor"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup.
2. Complete Phase 2: Foundational.
3. Complete Phase 3: User Story 1.
4. Stop and validate `/backoffice/products/` create flow: spinner appears, table refreshes, new row has `0 Qty`.
5. Run `dotnet test src/CookieCommerce.Web.Tests` and `dotnet build src`.

### Incremental Delivery

1. Deliver US1 create flow as the MVP.
2. Deliver US2 table consistency checks for existing fake products and zero-stock created products.
3. Complete polish validation through `specs/003-create-product/quickstart.md`.

### Notes

- Do not add real ProductDomain API persistence in this task set.
- Do not add product variants beyond retaining the current default fake DTO shape.
- Do not call backend APIs directly from Razor components; continue using `IBackofficeService`.
- Update `Documentation/backend-documentation.md` only if `IBackofficeService` changes.
- Preserve accessibility: labels, validation feedback, disabled state, and spinner status text.
