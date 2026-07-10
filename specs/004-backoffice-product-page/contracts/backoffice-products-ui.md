# Contract: Backoffice Products UI

## Scope

This is a user-interface contract for the existing Blazor backoffice products page. It intentionally avoids defining a new backend API contract because the feature does not require new service endpoints, commands, events, projections, or status behavior.

## Existing route contract

- Primary page route: `/backoffice/products/`
- Existing optional selected-product route: `/backoffice/products/{ActiveProductId:guid}`
- Component path: `src/CookieCommerce.Web/Components/Backoffice/ProductsArea/Products.razor`
- Layout: existing `BackofficeLayout`

## Required user-visible behavior

1. Product workflow entry points for this feature lead to the existing backoffice products page.
2. The current visual structure remains recognizable:
   - Existing backoffice layout/header/sidebar remains in use.
   - Existing products table/list area remains in use.
   - Existing selected-product/details area remains in use.
   - Existing create-product modal/page workflow remains in use where applicable.
3. No new or duplicate backoffice product page is introduced for the same workflow.
4. Existing status radio/button/toggle controls remain visible in their current location/style unless the smallest necessary accessibility clarification is required.
5. Status interaction has no functional effect:
   - No save/update call for product status.
   - No validation requirement for status.
   - No filtering/publishing/product behavior change based on status.
   - No status-related success message.
   - No status-related ProductDomain event or integration event.

## Status control interaction contract

Given the page displays a status control:

- When the user clicks or changes the control, the product workflow must still be completable without status-specific validation.
- When a product or variant is saved through currently functional fields, status must not be included as a saved behavior for this feature.
- When the page is refreshed or revisited, any status control state may return to its default/existing visual state.
- If the implementation uses a disabled or inert control, it must preserve accessibility and should avoid suggesting that a status change was saved.

## Non-contracts / explicitly out of scope

- No new product status API endpoint.
- No ProductDomain `StatusChanged` command/event/projection.
- No new status-related integration event.
- No new status filter in product lists.
- No new durable status persistence.
- No new sensitive personal information handling.

## Compatibility notes

If implementation changes an interface under `src/CookieCommerce.Web/Services/**/*.cs`, it must also update `Documentation/backend-documentation.md` according to the constitution. Preferred implementation should avoid service interface changes for this scope.
