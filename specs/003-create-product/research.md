# Phase 0 Research: Create Product Button MVP

## Decision: Deliver the current request as a Blazor backoffice fake-data MVP

**Rationale**: The user narrowed the scope: only the `Create Product` button needs to work for now, the existing Products table must show all fake products with fake stocks, and after creation the table must refresh with the new product at zero stock. The current backoffice Products page already uses `IBackofficeService` and `FakeBackofficeService`, backed by `StaticInventoryList.InventoryItems`, so this is the smallest coherent slice.

**Alternatives considered**:
- Full ProductDomain persistence/bootstrap through Marten and ProductDomain API. Rejected for this immediate request because it expands scope beyond making the button/table behavior work now.
- New product API client in the Web project. Rejected for this MVP because the screen already routes data access through `IBackofficeService` and fake backoffice data.

## Decision: Keep table data in `StaticInventoryList.InventoryItems` for this phase

**Rationale**: Existing fake products already live in `src/CookieCommerce.Web/Services/Products/StaticInventoryList.cs` with stock values, active status, references, images, and variants. Binding the Products table to `GetInventoryItemsAsync()` displays all existing fake products with fake stocks.

**Alternatives considered**:
- Duplicate static product data into another mock source. Rejected because it risks divergence.
- Bootstrap ProductDomain events for fake products. Rejected for the MVP; can be planned later when real backend persistence is required.

## Decision: On successful create, append a fake product with `Stock = 0` and refresh inventory

**Rationale**: User specifically requested that after creation the table updates and includes the new product with zero stock. `FakeBackofficeService.CreateProductAsync()` should create a `ProductDto` with a new ID, provided name/reference, no main image unless supplied, inactive/draft state, and `Stock = 0`. The page should refresh the `InventoryItems` collection after `OnProductCreated` rather than only calling `StateHasChanged`.

**Alternatives considered**:
- Optimistically mutate the page collection only. Rejected because service state would not be the source of truth and detail views could become inconsistent.
- Require stock entry in the create form. Rejected because user asked for new products to appear with zero stock.

## Decision: Show spinner/loading state during create submission

**Rationale**: Existing fake service methods simulate latency with `Task.Delay`. The modal submit handler should expose an `IsSaving` state, disable cancel/submit while saving, and render a small spinner with accessible status text. After save, invoke the parent callback to refresh the table and close/reset the modal.

**Alternatives considered**:
- Use only the table skeleton after saving. Rejected because user asked for a spinning wheel during create.
- Global loading overlay. Rejected because the create action is localized to the modal/button.

## Decision: Validate at the UI/service boundary for the fake MVP

**Rationale**: The constitution requires forms to provide validation feedback and data access to go through typed services. Add or use validation on the create DTO/form for required product fields used by this MVP: product name and internal reference, plus any retained default variant price rule if the current modal keeps the default variant control.

**Alternatives considered**:
- Rely on backend/domain validation only. Rejected because this MVP is fake-service-backed and must still provide immediate user guidance.
