# Phase 1 Data Model: Backoffice Product Page Reuse

This feature does not introduce new durable product state. It documents the entities already involved so implementation can preserve current behavior and avoid accidental status persistence.

## Entity: Product

**Description**: A merchandise item managed by backoffice users through the existing backoffice products page.

**Existing UI model paths**:
- `src/CookieCommerce.Web/Models/Products/ProductDto.cs`
- `src/CookieCommerce.Web/Models/Products/CreateProductDto.cs`
- `src/CookieCommerce.Web/Models/Products/ProductVariantDto.cs`
- Existing fake data under `src/CookieCommerce.Web/Services/Products/StaticInventoryList.cs`

**Fields in current workflow**:
- `ProductId`: Product identifier for selecting/viewing a product.
- `ProductReference`: Internal reference/SKU-style value displayed/entered by backoffice users.
- `ProductName`: Product display/management name.
- `ProductMainImage`: Optional product image details where already supported.
- `Variants`: One or more package/price variants.

**Relationships**:
- A Product has many Product Variants.
- A Product may have an image.
- A Product is displayed and selected from the existing backoffice products table.

**Validation rules for this feature**:
- Reuse validation already present for functional product fields.
- Do not add product status validation.
- Completion and validation messages must refer only to functional product fields/actions.

**State transitions**:
- Product selection may change the selected item displayed on the existing page.
- Product creation/editing may follow existing workflow behavior.
- Product status must not transition in this feature.

## Entity: Product Variant

**Description**: A sellable/configurable variant of a product shown on the existing product detail/table UI.

**Existing UI model paths**:
- `src/CookieCommerce.Web/Models/Products/ProductVariantDto.cs`
- `src/CookieCommerce.Web/Models/Products/CreateProductVariantDto.cs`

**Fields in current workflow**:
- `Id`: Variant identifier.
- `Name`: Package/variant name.
- `Price`: Sale price.
- `CostPrice`: Cost summary displayed by the UI.
- `CurrencySymbol`: Currency display.
- `Stock`: Existing/fake stock amount displayed by the UI.
- `Skus`: SKU/ingredient associations where already supported.
- `Active`: Existing visual active/draft/status-like field that must not become functional due to this feature.

**Relationships**:
- A Product Variant belongs to one Product.
- A Product Variant may have SKU rows/ingredients.

**Validation rules for this feature**:
- Preserve validation for existing functional variant fields.
- Do not require `Active` or any status/radio/toggle value.

**State transitions**:
- Functional variant fields may continue to behave as already implemented/specified.
- Status/active/draft selection must not save, publish, filter, validate, or otherwise affect behavior in this feature.

## Entity: Product Status

**Description**: A visible page control representing status intent. Its behavior is explicitly deferred.

**Fields**:
- Visible selected/default state only.
- No new persisted field, event, command, read-model field, API request field, or validation requirement is introduced by this feature.

**Relationships**:
- Visually associated with Product or Product Variant controls on the existing backoffice products page.
- Must not drive Product or Variant behavior for this feature.

**Validation rules**:
- Must never be required to complete the current product workflow.
- Must not show status-related validation errors.
- Must not show status-saved/status-changed confirmation messages.

**State transitions**:
- User interaction may change only local visual state if the existing UI allows it.
- No durable or behavioral transition is allowed.
- Refreshing or returning to the page may show the default/existing visual state.

## Entity: Backoffice User

**Description**: Authorized staff user managing products through the existing backoffice products page.

**Fields**:
- No new fields introduced.
- No new sensitive personal information introduced.

**Relationships**:
- Uses the existing backoffice products route/page.

**Validation rules**:
- Existing authorization/access behavior remains unchanged.
- No new PII handling or logging is required.
