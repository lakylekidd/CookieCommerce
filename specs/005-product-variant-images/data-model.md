# Data Model: Product Variant Images

## Product

**Owner**: ProductDomain aggregate root.

**Fields**:
- `Id` (`Guid`): aggregate/product id.
- `Name` (`string`): existing product display name, required.
- `Description` (`string`): existing product description, required.
- `Price` (`decimal`): existing base price, positive.
- `IsActive` (`bool`): existing active flag.
- `MainImage` (`ProductMediaAsset?`): optional single product-level main image.
- `Variants` (`IReadOnlyCollection<ProductVariant>`): product-owned variants.

**Relationships**:
- Owns zero or more `ProductVariant` records.
- References zero or one `ProductMediaAsset` as `MainImage`.

**Validation rules**:
- Product name and description are required.
- Price must be positive.
- At most one product main image may be current at a time.
- Replacing `MainImage` must not change any variant photos.

**State transitions**:
- Created by `ProductCreated`.
- Main image set/replaced by a product-media event such as `ProductMainImageSet`.
- Variant collection changed by variant events.
- Deactivated by existing `ProductDeactivated`.

## ProductVariant

**Owner**: ProductAggregate child entity/value record.

**Fields**:
- `Id` (`Guid`): stable variant id, preferably `Guid.CreateVersion7()` for new variants.
- `Name` (`string`): user-visible variant name, required.
- `Sku` or `Reference` (`string?`): optional stable variant code if existing inventory needs it.
- `Price` (`decimal`): variant price, positive.
- `CurrencySymbol` (`string`): defaults to `€` for current UI data.
- `IsActive` (`bool`): variant active status if existing UI continues to expose it.
- `Photos` (`IReadOnlyCollection<ProductMediaAsset>`): zero or more variant-specific photos.

**Relationships**:
- Belongs to exactly one `Product`.
- References zero or more `ProductMediaAsset` records as photos.

**Validation rules**:
- Variant name is required.
- Variant price must be positive.
- Duplicate variants within the same product are rejected. Initial duplicate key should be normalized variant name; include SKU/reference if introduced.
- Variant photo asset owner must match the product and target variant.
- Removing a variant removes/hides that variant's photo associations from product read models.

**State transitions**:
- Added by `ProductVariantAdded`.
- Updated by `ProductVariantUpdated`/existing rename-price commands if split.
- Removed by `ProductVariantRemoved`.
- Photos added/removed by `VariantPhotoAdded` and `VariantPhotoRemoved`.

## ProductMainImage

**Type**: Product-level specialization of `ProductMediaAsset`.

**Fields**:
- Same metadata as `ProductMediaAsset`.
- `Role`: `MainImage`.
- `ProductId`: required.
- `VariantId`: null.

**Validation rules**:
- Exactly one current main image at most per product.
- Supported image content type and size only.
- Replacement supersedes the old main-image association; old content may be deleted or retained only as non-current cleanup/audit metadata outside user-facing read models.

## VariantPhoto

**Type**: Variant-level specialization of `ProductMediaAsset`.

**Fields**:
- Same metadata as `ProductMediaAsset`.
- `Role`: `VariantPhoto`.
- `ProductId`: required.
- `VariantId`: required and must exist on product.
- `SortOrder` (`int`): optional display order for multiple photos.

**Validation rules**:
- The target variant must belong to the target product.
- Multiple photos are allowed for a variant.
- Photos must not appear on a different variant after variant updates/removal.

## ProductMediaAsset

**Owner**: ProductDomain metadata/reference; content stored by configured media store.

**Fields**:
- `Id` (`Guid`): media asset id.
- `ProductId` (`Guid`): owning product.
- `VariantId` (`Guid?`): target variant for variant photos, null for product main image.
- `Role` (`ProductMediaRole`): `MainImage` or `VariantPhoto`.
- `StorageKey` (`string`): stable internal key in configured media store.
- `Url` (`string`): display URL/path exposed to Web read models.
- `OriginalFileName` (`string`): sanitized original filename for staff context.
- `ContentType` (`string`): accepted image media type.
- `LengthBytes` (`long`): validated byte length.
- `Sha256` (`string`): checksum for integrity/dedup diagnostics.
- `AltText` (`string?`): accessible description, optional but recommended.
- `CreatedAt` (`DateTimeOffset`): supplied by application boundary.
- `IsCurrent` (`bool`): true for current associations in read models.

**Validation rules**:
- `ContentType` must be accepted (`image/jpeg`, `image/png`, `image/webp` recommended; GIF only if explicitly accepted later).
- `LengthBytes` must be within configured maximum.
- File must be decodable as an image before association.
- `StorageKey` and `Url` must not expose developer-specific absolute paths or secrets.

## StaticVariantDefaults

**Source**: Existing static inventory/variant defaults, especially `src/CookieCommerce.Web/Services/Products/StaticInventoryList.cs`.

**Fields used for bootstrap**:
- Product id/reference/name.
- Product main image URL/path.
- Variant id/name/price/currency/active flag.
- Variant image URL/path collection.
- Existing SKU composition where needed by inventory UI.

**Validation rules**:
- Every bootstrapped product must have at least one variant.
- Every referenced default image path must resolve to an existing image asset before bootstrap completes.
- Missing media reports must include product reference, variant name when applicable, and the missing path.

## BackofficeProductForm

**Scope**: UI/view model, not an event-sourced entity.

**Fields**:
- Existing product fields.
- `MainImageUpload`/`MainImagePreview`.
- `Variants[]` with variant fields and photo upload/preview state.
- Validation messages per product field, variant row, and image input.

**Validation rules**:
- Preserve entered product/variant fields when image validation fails where practical.
- Display accepted file types and maximum file size near upload controls.
- Use labels, alt text inputs or sensible generated alt text, keyboard-reachable add/remove controls, and responsive layout.
