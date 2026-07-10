# Phase 0 Research: Backoffice Product Page Reuse

## Decision: Reuse the existing Blazor backoffice products route and components

**Rationale**: The requested behavior is explicitly a scope clarification: product work must happen on the current backoffice product page and current UI. The repository already has this user surface at `src/CookieCommerce.Web/Components/Backoffice/ProductsArea/Products.razor`, with supporting components under `Components/Backoffice/ProductsArea/` and a create-product modal under `CreateProductModalArea/`. Reusing these files satisfies FR-001 through FR-003 without introducing a duplicate page or redesign.

**Alternatives considered**:
- Add a new backoffice product page: rejected because FR-003 forbids a separate page or parallel UI.
- Replace the current UI with a redesigned product workflow: rejected because FR-002 requires preserving existing layout, styling, and control placement.
- Move product work directly into ProductDomain API screens: rejected for this scope because the user-facing surface must remain the existing Blazor backoffice product page.

## Decision: Keep status controls visually present but exclude them from current behavior

**Rationale**: The existing page includes visible active/draft-style status controls in the products area. This feature requires those controls to remain visible while status behavior is deferred. Implementation should not call status-changing service methods from user interaction for this scope, should not persist status choices, and should not validate or show success messages for status.

**Alternatives considered**:
- Remove or hide the status control: rejected because FR-004 requires the existing status radio/button control to remain visible.
- Fully implement status behavior now: rejected because FR-005 explicitly says status must not save, update, validate, filter, publish, or otherwise affect product behavior in this feature.
- Disable the control visually: acceptable only if it does not alter the existing recognizable UI more than necessary. Preferred path is to preserve the current visual placement/style and ignore interaction or make it non-submitting/non-saving without implying support.

## Decision: Treat this as a Web UI/fake-service scope unless implementation discovers existing ProductDomain integration already in use

**Rationale**: The current products page is wired to `IBackofficeService` and `FakeBackofficeService`, with product DTOs in `src/CookieCommerce.Web/Models/Products`. The earlier create-product MVP in `specs/003-create-product` also selected this fake-data path. This feature is a clarification on page reuse and status deferral rather than a new backend capability.

**Alternatives considered**:
- Add ProductDomain API commands for status deferral: rejected because no new status behavior is required.
- Add new ProductDomain persistence/projections: rejected because the current feature does not require durable status or a new product read model beyond existing workflow behavior.

## Decision: Validate through page-level behavior and focused component/service checks

**Rationale**: Constitution testing rules require useful coverage for behavioral changes. For this scope, the critical behavior is user-facing: route/page reuse and status non-effect. Validation should prove no duplicate route/page was introduced, existing product workflow remains on `/backoffice/products/`, and status interaction does not call status-changing methods or affect saved product data.

**Alternatives considered**:
- ProductDomain aggregate tests for status: rejected because status behavior is out of scope and should not be implemented.
- Broad end-to-end infrastructure tests: rejected as unnecessary unless implementation changes ProductDomain API, persistence, projections, or cross-domain messaging.

## Decision: No new PII, integration events, Aspire topology, or container changes

**Rationale**: The feature only constrains an existing product UI and defers product status behavior. It does not require customer/staff personal data, new integration events, runtime services, secrets, or infrastructure topology changes.

**Alternatives considered**:
- Add status-related integration events or projections: rejected because FR-005 and architecture notes say status behavior is not in scope.
- Add a new runtime/service for product page work: rejected because the existing Web runtime and page are the selected user surface.
