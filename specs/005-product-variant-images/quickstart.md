# Quickstart: Validate Product Variant Images

This guide defines end-to-end validation scenarios for the implementation phase. It does not prescribe implementation code.

## Prerequisites

- .NET 10 SDK available.
- Repository dependencies restored.
- ProductDomain API, Web, PostgreSQL/Marten, RabbitMQ, and other configured dependencies runnable through Aspire.
- Product media storage configured through Aspire/appsettings/environment. Development may use a durable local media folder/volume.

## Build and test commands

From repository root:

```powershell
dotnet build src
dotnet test src/CookieCommerce.ProductDomain.Tests
dotnet test src/CookieCommerce.Web.Tests
```

For broad validation before merge:

```powershell
dotnet test src
```

If ProductDomain API containerization is added or changed, also build the affected image and confirm the service starts in Aspire:

```powershell
dotnet run --project src/CookieCommerce.AppHost
```

Expected result: AppHost starts the system without missing service/media configuration errors.

## Scenario 1: Create product variants on existing backoffice page

1. Start the app through Aspire.
2. Open the existing backoffice product page (`/backoffice/products/`).
3. Create or edit a product.
4. Add three valid variants with distinct names/prices.
5. Save the product.
6. Reopen the product.

Expected results:
- The workflow never leaves the existing backoffice product page.
- All variants are visible after reload.
- Variant duplicates or missing required fields are rejected with user-facing messages.

## Scenario 2: Add photos to variants

1. Open a product with multiple variants.
2. Upload a supported image to variant A.
3. Upload a different supported image to variant B.
4. Save and reload the product.

Expected results:
- Variant A displays only its photo(s).
- Variant B displays only its photo(s).
- Photos remain correctly linked after save, refresh, and product reload.

## Scenario 3: Reject invalid variant image without losing form data

1. Start editing a product and enter/update product and variant fields.
2. Upload an unsupported file, oversized image, or unreadable image for one variant.

Expected results:
- The upload is rejected with a clear validation message near the affected variant photo control.
- Product and variant text/price fields remain populated where practical.
- Other valid variant images are not reassigned or removed.

## Scenario 4: Set and replace one product main image

1. Open a product without a main image or remove the existing main image.
2. Upload a supported product-level main image.
3. Save and reload the product.
4. Upload a different supported product-level main image.
5. Save and reload again.

Expected results:
- The product has one current main image after each save.
- The second upload replaces the first product-level main image association.
- Variant photos remain unchanged.

## Scenario 5: Remove inappropriate media

1. Open a product with a main image and variant photos.
2. Remove the product main image.
3. Remove one variant photo.
4. Save and reload.

Expected results:
- Removed media no longer appears in current product/backoffice read models.
- Removing the main image does not remove variant photos.
- Removing a variant photo does not remove the main image or photos from other variants.

## Scenario 6: Bootstrap products with static variant defaults and images

1. Start from an empty development database/media store.
2. Run the bootstrap path used by local development/demo startup.
3. Open the backoffice products page.

Expected results:
- Every bootstrapped product has at least one variant.
- Product main images and variant photos from static defaults are visible where references exist.
- No broken image references appear.

## Scenario 7: Bootstrap reports missing image defaults

1. Temporarily configure or simulate a static default image path that does not exist.
2. Run bootstrap validation/startup.

Expected results:
- Bootstrap reports the missing product reference, variant name when applicable, and image path.
- The failure is clear enough for a developer to fix the static default or media file.
- Bootstrap does not silently create products with broken image references.

## Documentation check

If implementation changes any `src/CookieCommerce.Web/Services/**/*.cs` interface (for example `IBackofficeService`), verify `Documentation/backend-documentation.md` is updated in the same change.

## Implementation notes

- Product variants, product main images, and variant photos are now ProductDomain-owned behavior exposed through `IBackofficeService`; the existing `/backoffice/products/` surface remains the operator workflow.
- Product media bytes are stored through the configured ProductDomain media store. Events and read models store metadata and URLs only, not image bytes.
- Development media storage is wired from AppHost through `ProductMedia__StorageRootPath` and `ProductMedia__RequestPath`; default appsettings provide local fallback values.
- Web upload controls perform client-side size/content-type checks for faster feedback, but ProductDomain API validation remains authoritative for unsupported, oversized, unreadable, or mis-scoped images.
- Product media store logging records product/variant ids, role, byte length, and a hash of the storage key; it does not log image contents, absolute storage paths, or raw uploaded filenames.

## Phase 7 validation record

- `dotnet build src`: PASS on 2026-07-10 with existing package vulnerability/nullability warnings.
- `docker build -f src/CookieCommerce.ProductDomain.Api/Dockerfile src -t cookiecommerce-productdomain-api:phase7`: PASS on 2026-07-10 with existing package warnings.
- `dotnet run --project src/CookieCommerce.AppHost`: PASS startup/build validation on 2026-07-10; command was stopped after the Aspire dashboard began listening.
- `dotnet test src/CookieCommerce.ProductDomain.Tests`: PASS on 2026-07-10.
- `dotnet test src/CookieCommerce.Web.Tests`: PASS on 2026-07-10.
- `dotnet test src --no-build`: PASS on 2026-07-10.
- `dotnet format src/CookieCommerce.slnx --verify-no-changes`: PASS on 2026-07-10. Running bare `dotnet format` from the repository root does not discover the `.slnx` workspace, so the solution path was supplied explicitly.
- Quickstart scenarios 1-7 require a browser session against the running Aspire stack. In this agent run they are intentionally deferred for manual browser execution; no scenario-specific product/media issues were observed from static review or automated validation.
- Manual execution should record any browser or bootstrap findings below this section before merge.
