# Implementation Plan: Backoffice Create Product Button MVP

**Branch**: `feature/003-create-product` | **Date**: 2026-07-10 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/003-create-product/spec.md`, plus user scope update: "for now i only want the button create product to work. I also want the table on the page that shows the products to display all the existing products with fake stocks. Once a product is added, there should be a spinning wheel and once its done the table should update the include the new product with zero stock."

## Summary

Implement the narrowed MVP on the Blazor backoffice Products page: make the existing create-product button/modal successfully add a product through the current `IBackofficeService` fake-data path, show a spinner while saving, and refresh the inventory table so the newly created product appears with `0` stock alongside all existing fake products and fake stock values.

This plan intentionally does not implement full ProductDomain persistence/bootstrap yet. The current UI screen is already wired to `FakeBackofficeService` and `StaticInventoryList`, which is the shortest path to the requested behavior.

## Technical Context

**Language/Version**: .NET 10 / C# 14, Blazor Interactive Server components.

**Primary Dependencies**: Blazor, ASP.NET Core components/forms, existing `IBackofficeService`, existing fake product data in `StaticInventoryList`. ProductDomain dependencies (Marten, MediatR, MassTransit, Aspire) remain relevant to the repository but are not the primary implementation path for this narrowed MVP.

**Storage**: In-memory fake product list: `src/CookieCommerce.Web/Services/Products/StaticInventoryList.cs`. Durable Marten/PostgreSQL product storage is out of scope for this immediate MVP.

**Testing**: xUnit/FluentAssertions where automated tests are added. At minimum validate with `dotnet build src`; use existing test projects if implementation touches ProductDomain behavior.

**Target Platform**: Aspire-orchestrated CookieCommerce web app, specifically `src/CookieCommerce.Web` Blazor backoffice UI.

**Project Type**: Blazor Web UI + UI service fake-data implementation. No new bounded context or runtime service.

**Performance Goals**: Product create should complete after the existing simulated service latency and update the table without requiring a browser reload. No high-throughput backend goal for this fake-data MVP.

**Constraints**: Components must use typed services under `src/CookieCommerce.Web/Services`; no direct backend API calls from Razor components. UI must preserve accessibility, visible loading feedback, and validation guidance. New products must show `Stock = 0`.

**Scale/Scope**: One backoffice screen (`Products.razor`), one create modal, one fake service, and the table components already used by that screen.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- **Domain boundaries**: PASS. The narrowed MVP is UI/fake-service behavior inside `src/CookieCommerce.Web`. ProductDomain ownership is acknowledged, but durable domain creation/bootstrap is intentionally deferred for this immediate UI behavior.
- **Backend standards**: PASS. No controller/application/domain changes are planned for this MVP. Existing backend standards remain unaffected.
- **Event sourcing + CQRS**: PASS with scope note. No durable product state changes are introduced in this MVP. If later implementing real ProductDomain persistence, product creation must continue through event-sourced ProductDomain commands/projections.
- **Testing discipline**: PASS. Plan calls for validation of the fake service/create flow and at minimum `dotnet build src`; ProductDomain tests are only required if ProductDomain behavior changes.
- **Async integration**: PASS. No cross-domain communication is introduced.
- **Privacy + operations**: PASS. Fake product data and newly created product fields must not contain customer/staff PII. No secrets/config changes.
- **Aspire + containerization**: PASS. No new runtime service/container is introduced. Existing Aspire composition remains the run path.
- **Frontend standards**: PASS. Work stays in `Components/Backoffice/ProductsArea`, uses `IBackofficeService`, preserves component-local state, and adds accessible spinner/validation behavior.
- **Documentation impact**: PASS with condition. If `IBackofficeService` signatures change, implementation must update `Documentation/backend-documentation.md`. The preferred MVP path does not require an interface signature change.

## Project Structure

### Documentation (this feature)

```text
specs/003-create-product/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── backoffice-products-ui.md
└── tasks.md             # Created later by /speckit.tasks, not by this plan command
```

### Source Code (repository root)

```text
src/
├── CookieCommerce.Web/
│   ├── Components/Backoffice/ProductsArea/
│   │   ├── Products.razor
│   │   ├── InventoryTable.razor
│   │   ├── InventoryTableItem.razor
│   │   └── CreateProductModalArea/
│   │       └── CreateNewProductModal.razor
│   ├── Models/Products/
│   │   └── CreateProductDto.cs
│   └── Services/
│       ├── IBackofficeService.cs
│       ├── FakeBackofficeService.cs
│       └── Products/
│           └── StaticInventoryList.cs
└── CookieCommerce.ProductDomain.*      # Existing real product domain remains out of scope unless later scope expands
```

**Structure Decision**: Use the existing `CookieCommerce.Web` backoffice feature folder and fake backoffice service. Do not add a new ProductDomain API/client slice for this narrowed MVP.

## Phase 0: Research Summary

See [research.md](./research.md).

Resolved decisions:

- Use the current Blazor backoffice fake-data path.
- Keep existing fake products/stocks in `StaticInventoryList.InventoryItems`.
- Append created products to the same list with `Stock = 0`.
- Show save spinner in the create modal/button while `CreateProductAsync` is running.
- Refresh the table from the service after successful create.

## Phase 1: Design Summary

See [data-model.md](./data-model.md), [contracts/backoffice-products-ui.md](./contracts/backoffice-products-ui.md), and [quickstart.md](./quickstart.md).

Implementation should preserve this user-visible contract:

1. Products table displays all existing fake products with fake stock.
2. Create Product button opens a usable product creation flow.
3. Submit shows a spinner/loading state and prevents duplicate submits.
4. Successful create appends a product with zero stock.
5. Parent page refreshes the table and shows the new row without manual reload.

## Agent Context Update

No `.specify` agent-context update script exists in this repository (`.specify/scripts/powershell/` contains setup/check scripts only), so no agent context file was updated by this plan command.

## Post-Design Constitution Check

- **Domain boundaries**: PASS. Design limits changes to the Web fake-data slice and records ProductDomain durable persistence as future work, not hidden scope.
- **Backend standards**: PASS. No backend/API changes required by the selected design.
- **Event sourcing + CQRS**: PASS. No new durable event-sourced state in this MVP.
- **Testing discipline**: PASS. Quickstart defines build/manual validation and notes automated test expectations if implementation touches ProductDomain.
- **Async integration**: PASS. No cross-domain integration introduced.
- **Privacy + operations**: PASS. No PII/secrets/operational changes.
- **Aspire + containerization**: PASS. Existing AppHost run path remains valid; no container changes.
- **Frontend standards**: PASS. Design uses Blazor component-local saving/loading state, service access, validation, and accessible spinner/status behavior.
- **Documentation impact**: PASS conditional. No interface change is expected; update backend documentation if implementation changes `IBackofficeService`.

## Complexity Tracking

No constitution violations requiring justification.
