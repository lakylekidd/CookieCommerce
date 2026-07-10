# Quickstart: Validate Create Product Button MVP

## Prerequisites

- .NET 10 SDK available.
- Repository dependencies restored.
- Run from repository root: `C:\Users\willi\source\Repos\CookieCommerce`.

## Build check

```powershell
dotnet build src
```

Expected outcome: solution builds without compiler errors.

## Run the app

```powershell
dotnet run --project src/CookieCommerce.AppHost
```

Expected outcome: Aspire starts the CookieCommerce app and the Blazor Web project is reachable.

## Scenario 1: Existing fake products display with stock

1. Open the Web app.
2. Navigate to `/backoffice/products/`.
3. Wait for table loading to complete.

Expected outcome:

- Table shows all existing fake products from `StaticInventoryList.InventoryItems`.
- Each row shows product name, reference, stock quantity, status, and image/placeholder.
- Fake products have non-empty stock quantities matching their fake inventory data.

## Scenario 2: Create product shows spinner and refreshes table

1. On `/backoffice/products/`, click `New Product` / `Create Product`.
2. Enter a product name and internal reference.
3. Submit the form.

Expected outcome:

- A spinning/loading indicator appears while save is in progress.
- Duplicate submit is prevented while saving.
- When save completes, the modal closes or returns to an idle state.
- The table refreshes without a browser reload.
- The new product row appears in the table.
- The new product row shows `0 Qty` stock.
- If the new product has no image, the tiger placeholder is shown.

## Scenario 3: Validation prevents incomplete product create

1. Open the create modal.
2. Leave required product fields empty.
3. Submit the form.

Expected outcome:

- No new row is added to the table.
- The user sees validation guidance for missing fields.
- The spinner is not left running.

## Focused automated checks to add during implementation

Run the smallest useful test command after implementation, for example:

```powershell
dotnet test src/CookieCommerce.ProductDomain.Tests
```

If implementation only changes Web fake services/components and there are no existing component tests, at minimum run:

```powershell
dotnet build src
```

Expected outcome: command completes successfully with zero errors.
