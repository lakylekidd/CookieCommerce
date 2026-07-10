# Phase 1 Data Model: Create Product Button MVP

## ProductDto

Existing UI inventory/product row model used by `src/CookieCommerce.Web/Components/Backoffice/ProductsArea/*`.

| Field | Type | Notes |
|---|---|---|
| `ProductId` | `Guid` | Unique product identifier. Generated on fake create. |
| `ProductReference` | `string` | Internal product reference shown in the table. Required for create MVP. |
| `ProductName` | `string` | Product display name shown in table/detail. Required for create MVP. |
| `ProductMainImage` | `ProductImageDto?` | Existing fake products use cookie images; newly created MVP products may be null and use the tiger placeholder. |
| `Stock` | `int` | Existing fake products use fake stock. Newly created products must be `0`. |
| `Active` | `bool` | Existing fake products are live. New products should default to draft/inactive unless explicitly activated later. |
| `IsLimitedEdition` | `bool` | Unchanged by this MVP. |
| `IsFeatured` | `bool` | Unchanged by this MVP. |
| `Variants` | `ICollection<ProductVariantDto>` | Product variants are not the focus for this MVP; retain only what current UI/service requires. |

### Validation rules

- `ProductName` must be non-empty and visible in the table after save.
- `ProductReference` must be non-empty and visible in the table after save.
- `Stock` for newly created products must be exactly `0`.
- MVP-created products must not include customer/staff PII.

### State transitions

```text
Create modal opened
  -> user enters product data
  -> submit clicked
  -> IsSaving = true; submit/cancel disabled; spinner visible
  -> FakeBackofficeService.CreateProductAsync appends ProductDto with Stock = 0
  -> Products page RefreshInventory reloads table rows
  -> modal closes and IsSaving = false
```

## CreateProductDto

Existing UI create command model in `src/CookieCommerce.Web/Models/Products/CreateProductDto.cs`.

| Field | Type | Notes |
|---|---|---|
| `ProductReference` | `string` | Required for fake ProductDto reference. |
| `ProductName` | `string` | Required for fake ProductDto name. |
| `Variants` | `ICollection<CreateProductVariantDto>` | Existing modal currently provides a default variant. For this MVP, do not add variant-management scope beyond what the modal already requires. |

### Validation rules

- Required field guidance should be shown before calling the service.
- If the existing variant editor remains visible, retained variant validation should not block a product-only happy path unexpectedly.

## StaticInventoryList

Existing fake product bootstrap source in `src/CookieCommerce.Web/Services/Products/StaticInventoryList.cs`.

| Member | Type | Notes |
|---|---|---|
| `InventoryItems` | `List<ProductDto>` | Source of table rows for existing fake products and newly created fake products. |

### Idempotency/duplicates

- For this MVP, existing static products are already initialized once by static list construction.
- Fake product create should append one product per successful submit.
- Full idempotent backend bootstrap remains out of scope for this narrowed request.
