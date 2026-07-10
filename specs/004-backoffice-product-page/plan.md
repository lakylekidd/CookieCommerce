# Implementation Plan: Backoffice Product Page Reuse and Status Deferral

**Branch**: `feature/003-create-product` | **Date**: 2026-07-10 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/004-backoffice-product-page/spec.md`: reuse the existing backoffice product page and UI; keep the status radio/button visible as-is but non-functional for now.

## Summary

Constrain the product workflow to the existing Blazor backoffice products page (`/backoffice/products/`) and current ProductsArea UI. Implementation should preserve the established layout and controls, avoid introducing a parallel page or redesigned workflow, and make the visible status radio/button/toggle inert for this feature: it must not save, validate, publish, filter, or otherwise affect product behavior.

This scope is primarily a Web UI contract/guardrail on top of the current create-product MVP. It should not add ProductDomain status behavior, integration events, projections, or new runtime topology.

## Technical Context

**Language/Version**: .NET 10 / C# 14, Blazor Interactive Server components.

**Primary Dependencies**: Blazor, ASP.NET Core components/forms, existing `IBackofficeService`, existing `FakeBackofficeService`, existing product DTOs in `src/CookieCommerce.Web/Models/Products`. Repository-level ProductDomain dependencies (Marten, MediatR, MassTransit, Aspire) remain relevant but are not the selected implementation path for status deferral.

**Storage**: No new durable storage. Existing/fake product data may continue through `src/CookieCommerce.Web/Services/Products/StaticInventoryList.cs`; product status must not be newly persisted or treated as functional state.

**Testing**: xUnit and FluentAssertions. Prefer focused `CookieCommerce.Web.Tests` coverage or component/service checks for route reuse and status non-effect. Run `dotnet build src`; run ProductDomain tests only if implementation unexpectedly changes ProductDomain behavior.

**Target Platform**: Aspire-orchestrated CookieCommerce Web app, specifically the existing `src/CookieCommerce.Web` Blazor backoffice UI.

**Project Type**: Blazor Web UI guardrail/change. No new bounded context, API, infrastructure project, or runtime service.

**Performance Goals**: Existing page interactions should remain responsive with no added network or persistence work for status. Status interaction should not trigger status save latency.

**Constraints**: Preserve existing page layout/styling/control placement unless the smallest necessary change is needed to keep status non-functional and accessible. Components must use typed services under `src/CookieCommerce.Web/Services`; no direct backend API calls from Razor components. Status must not be required, validated, saved, filtered, published, or confirmed.

**Scale/Scope**: One existing backoffice products page and its supporting ProductsArea components, especially visible status controls in selected product/variant UI. No duplicate page and no status backend slice.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- **Domain boundaries**: PASS. ProductDomain owns product behavior, but this feature only constrains the existing `CookieCommerce.Web` backoffice surface and explicitly avoids new status domain behavior.
- **Backend standards**: PASS. No controller/application/domain changes are required. If later tasks discover backend changes are needed, controllers must remain thin and handlers async/focused.
- **Event sourcing + CQRS**: PASS. No new durable Product status state is introduced. Status-related events/projections are explicitly out of scope.
- **Testing discipline**: PASS. Plan requires focused validation for existing page reuse and status non-effect; ProductDomain tests are required only if ProductDomain behavior changes.
- **Async integration**: PASS. No cross-domain communication is introduced. No status-related integration events are required.
- **Privacy + operations**: PASS. No new PII, secrets, logging, health, or ProblemDetails behavior is introduced.
- **Aspire + containerization**: PASS. No new runtime surface/container is introduced; existing `CookieCommerce.Web` Dockerfile/AppHost path remains the runtime surface.
- **Frontend standards**: PASS. Work stays in `Components/Backoffice/ProductsArea`, uses typed services, preserves accessibility and existing UI, and keeps state local.
- **Documentation impact**: PASS with condition. Preferred path does not change `src/CookieCommerce.Web/Services/**/*.cs` interfaces. If implementation changes such an interface, update `Documentation/backend-documentation.md` in the same change.

Any failed gate MUST be listed in Complexity Tracking with a justification, the simpler alternative rejected, and the migration or mitigation path.

## Project Structure

### Documentation (this feature)

```text
specs/004-backoffice-product-page/
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
│   │   ├── SelectedInventoryItem.razor
│   │   ├── SelectedInventoryVariant.razor
│   │   └── CreateProductModalArea/
│   │       └── CreateNewProductModal.razor
│   ├── Models/Products/
│   │   ├── CreateProductDto.cs
│   │   ├── ProductDto.cs
│   │   └── ProductVariantDto.cs
│   └── Services/
│       ├── IBackofficeService.cs
│       ├── FakeBackofficeService.cs
│       └── Products/
│           └── StaticInventoryList.cs
├── CookieCommerce.Web.Tests/        # Preferred test project for UI/service guardrails
└── CookieCommerce.ProductDomain.*   # Existing real product domain; no status behavior changes planned
```

**Structure Decision**: Use the existing `CookieCommerce.Web` backoffice products feature folder and current service abstractions. Do not add a new Product page, ProductDomain status slice, API endpoint, event, projection, or runtime service for this feature.

## Phase 0: Research Summary

See [research.md](./research.md).

Resolved decisions:

- Reuse the current Blazor backoffice products route and components.
- Keep status controls visually present while excluding status from behavior.
- Treat implementation as a Web UI/fake-service scope unless existing ProductDomain integration is already in use.
- Validate with page-level behavior and focused component/service checks.
- Add no PII, integration events, Aspire topology, or container changes.

## Phase 1: Design Summary

See [data-model.md](./data-model.md), [contracts/backoffice-products-ui.md](./contracts/backoffice-products-ui.md), and [quickstart.md](./quickstart.md).

Implementation should preserve this user-visible contract:

1. Product workflow entry points lead to `/backoffice/products/` or the existing optional selected-product route under `/backoffice/products/{ActiveProductId}`.
2. The existing ProductsArea layout, visual style, and controls remain recognizable.
3. No duplicate page or redesigned product workflow is introduced.
4. The existing status radio/button/toggle remains visible.
5. Status interaction has no effect on saved product data, validation, filtering, publication, events, or other product behavior.

## Agent Context Update

No `.specify` agent-context update script exists in this repository (`.specify/scripts/powershell/` contains setup/check scripts only), so no agent context file was updated by this plan command.

## Post-Design Constitution Check

- **Domain boundaries**: PASS. Design explicitly keeps ProductDomain as the owner while avoiding new ProductDomain status behavior for this UI-only clarification.
- **Backend standards**: PASS. No backend/API changes are required by the selected design.
- **Event sourcing + CQRS**: PASS. No status commands/events/projections/read-model changes are introduced.
- **Testing discipline**: PASS. Quickstart defines focused validation and when to run Web/ProductDomain tests.
- **Async integration**: PASS. No cross-domain integration or status event publication is introduced.
- **Privacy + operations**: PASS. No new PII, secrets, observability, or operational changes.
- **Aspire + containerization**: PASS. Existing Web runtime/AppHost path remains valid; no container changes.
- **Frontend standards**: PASS. Design uses existing Blazor components, typed services, local UI state, accessibility-preserving controls, and no direct backend API calls.
- **Documentation impact**: PASS conditional. No service interface change is expected; update `Documentation/backend-documentation.md` if implementation changes `src/CookieCommerce.Web/Services/**/*.cs` interfaces.

## Complexity Tracking

No constitution violations requiring justification.
