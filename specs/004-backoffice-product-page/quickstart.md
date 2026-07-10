# Quickstart: Validate Backoffice Product Page Reuse

## Prerequisites

- .NET 10 SDK available.
- Repository root: `C:/Users/willi/source/Repos/CookieCommerce`.
- Use the existing branch for this scope: `feature/003-create-product`.

## Build validation

From the repository root:

```powershell
dotnet build src
```

Expected outcome:

- Build completes successfully.
- No new ProductDomain/API/Aspire projects are required for this feature.

## Run the app

From the repository root:

```powershell
dotnet run --project src/CookieCommerce.AppHost
```

Expected outcome:

- Aspire starts the CookieCommerce composition.
- The existing Web app can be opened from the Aspire dashboard or configured launch URL.

## Scenario 1: Existing products page is reused

1. Navigate to `/backoffice/products/`.
2. Confirm the page uses the existing backoffice layout and product UI:
   - Backoffice layout/sidebar/header remains recognizable.
   - Products table/list remains in the existing page location.
   - Selected product/details area remains in the existing page location.
   - Existing create product workflow remains reachable from this page.

Expected outcome:

- Product workflow is available from the existing backoffice product page.
- No new or duplicate backoffice product page is needed.
- The layout is not replaced by a redesigned screen.

## Scenario 2: Product selection remains on existing page

1. On `/backoffice/products/`, select an existing product from the products table/list.
2. If the route changes, confirm it remains under `/backoffice/products/{ActiveProductId}`.
3. Confirm details render in the existing selected-product area.

Expected outcome:

- Selection behavior stays within the existing backoffice product page.
- Current UI structure remains recognizable.

## Scenario 3: Status control is visible but non-functional

1. Locate the existing visible status radio/button/toggle control on the product/variant UI.
2. Interact with the status control if the UI permits interaction.
3. Complete or interact with currently functional product workflow fields.
4. Refresh or revisit the page.

Expected outcome:

- The status control remains visible in its existing location/style.
- The status control communicates that status changes are deferred (`aria-disabled="true"` and helper text/title) while preserving the existing switch presentation.
- No status-specific validation appears.
- No status-saved/status-changed confirmation appears.
- Product data or behavior does not change because of the status interaction.
- No ProductDomain status event, integration event, API call, or persisted status update is introduced by this feature.

## Scenario 4: Validation ignores status

1. Attempt the current product workflow with any status visual state.
2. Trigger validation for fields that are currently functional.

Expected outcome:

- Validation messages only mention functional product fields.
- Status selection is never required.
- Status does not block product workflow completion.

## Focused automated checks for implementation phase

When tasks are generated, prefer checks that prove:

- `Products.razor` remains the route/page for product workflow.
- No additional backoffice products page/route is added for the same workflow.
- Status-control interaction does not call `IBackofficeService.ChangeProductStatusAsync` or `ChangeVariantStatusAsync` for this feature scope.
- Existing create/list behavior from `specs/003-create-product` is not regressed.

Use the smallest relevant command after implementation, then run broader validation when backend/domain behavior changes:

```powershell
dotnet test src/CookieCommerce.Web.Tests
dotnet build src
```

If implementation changes ProductDomain API, persistence, projections, or integration messaging, also run:

```powershell
dotnet test src/CookieCommerce.ProductDomain.Tests
dotnet test src
```
