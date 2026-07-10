# Planning Context: Create Product Button MVP

## Intent
- User wants a narrow MVP for the backoffice Products page.
- For now, only the existing `Create Product` button / modal flow must work.
- Product table must show all existing fake products with fake stock values.
- After adding a product, show a spinner/loading state while saving.
- When save finishes, refresh the table and include the new product with zero stock.

## Decisions
- Treat this as a Blazor backoffice UI + existing fake service MVP, not a full ProductDomain persistence/bootstrap implementation yet.
- Use existing `IBackofficeService` and `FakeBackofficeService` because the Products page already depends on this service and static inventory data.
- Keep variants out of the primary workflow. Existing modal includes a default variant, but user explicitly says product creation only for now.
- New fake-created products should set `Stock = 0`, `Active = false` initially, no main image unless UI collects one.
- Refresh `InventoryItems` after `OnProductCreated` instead of only calling `StateHasChanged`, so the table reflects service state.

## Constraints
- Constitution requires Blazor components to route backend/data access through services, not direct API calls.
- If service interfaces change under `src/CookieCommerce.Web/Services`, update `Documentation/backend-documentation.md` in implementation.
- ProductDomain already has an API create endpoint, but Web currently uses fake backoffice services/static data for this screen.
- Current `CreateProductDto` lacks description/image fields; avoid expanding unless implementation needs it for validation or contract alignment.
- No variants beyond what current fake DTO structure requires.

## Open questions
- None blocking for this MVP plan.

## Discarded options
- Full backend persistence/bootstrap through ProductDomain: rejected for now because user narrowed scope to making the button/table behavior work immediately.
- New dedicated product API client service: rejected for this MVP because current page already uses `IBackofficeService`, and adding a real API client would broaden scope.
- Building a new visual design: rejected because requested behavior uses existing page/modal/table visual structure; only loading/spinner state needs refinement.
