# Contract: Backoffice Products Create Button MVP

## Scope

This contract defines the expected behavior of the backoffice Products page for the narrowed MVP requested by the user.

## Page

- Route: `/backoffice/products/`
- Route with active product: `/backoffice/products/{ActiveProductId:guid}`
- Component: `src/CookieCommerce.Web/Components/Backoffice/ProductsArea/Products.razor`

## Data access

The page must use `IBackofficeService`; components must not directly call ProductDomain APIs.

Relevant service members:

```csharp
Task<ICollection<ProductDto>> GetInventoryItemsAsync();
Task<ProductDto> GetInventoryItemAsync(Guid productId);
Task CreateProductAsync(CreateProductDto createProductDto);
```

If the `IBackofficeService` interface signature changes, update `Documentation/backend-documentation.md` in the same implementation change.

## Initial table behavior

Given the Products page loads, when inventory loading completes, then the table displays all products returned by `GetInventoryItemsAsync()`.

Each row must display:

- product name
- product reference
- stock quantity
- live/draft status
- image or existing tiger placeholder when no image exists

Existing fake products must retain their fake stock values from `StaticInventoryList`.

## Create behavior

Given a backoffice user clicks `Create Product`, when the modal opens, then the user can submit the minimum product fields required by the MVP.

Given the user submits valid product data, then:

1. The create submit action shows a spinning/loading indicator.
2. Submit/cancel actions are disabled or otherwise guarded against duplicate submits while saving.
3. `IBackofficeService.CreateProductAsync(...)` is called once.
4. A new `ProductDto` is added to the inventory source.
5. The new product has `Stock = 0`.
6. The Products page refreshes the table from `GetInventoryItemsAsync()`.
7. The new product appears in the table without a page reload.

## Validation and failure behavior

Given required product fields are missing, when the user submits, then the product is not created and clear validation guidance is shown.

Given create fails unexpectedly, when the task completes with an error, then the modal remains usable, entered data is preserved where practical, the spinner stops, and the user sees a plain-language retry message.

## Out of scope for this MVP

- Real ProductDomain persistence from the Web UI.
- Durable backend bootstrap of fake product records.
- Product variants beyond what current fake DTO/UI requires.
- Inventory/stock management beyond setting newly created products to zero stock.
