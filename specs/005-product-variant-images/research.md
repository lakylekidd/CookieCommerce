# Research: Product Variant Images

## Decision: Store image content outside domain events and keep ProductMediaAsset references in ProductDomain

**Rationale**: Product and variant images are business media, but binary content does not belong in event history or text fields. The ProductDomain should own the product/media association and persist only stable metadata/reference values in events and read models: media asset id, owner scope, storage key or URL, original filename, content type, size, checksum, alt text, and audit timestamps. Image bytes should be written by an infrastructure media-store abstraction before or during the command workflow and referenced by the aggregate after validation succeeds.

**Alternatives considered**:
- Embed base64 image bytes in product events: rejected because event streams become large, immutable history retains unwanted media, and projections/read models become inefficient.
- Store only public URLs from the UI: rejected because validation, ownership, replacement, and removal cannot be enforced reliably by ProductDomain.
- Put image content directly in PostgreSQL/Marten documents: rejected for MVP because object/file media storage is simpler to serve and replace while Marten remains focused on event/read state.

## Decision: Use a configurable product-media store with local filesystem/volume for development and a blob-compatible seam for production

**Rationale**: The repository currently uses static image paths under the Web project and does not expose a dedicated media service. The implementation should introduce an `IProductMediaStore` in ProductDomain Application/Infrastructure with configuration supplied through Aspire/appsettings/environment. Development and bootstrap can use a durable local filesystem path mounted/configured by AppHost; production can swap to blob/object storage without changing domain events or UI contracts because events store only keys/references.

**Alternatives considered**:
- Add an unrelated standalone media bounded context now: rejected because the requested behavior is ProductDomain-owned product media and does not need cross-domain media workflows.
- Keep using `src/CookieCommerce.Web/wwwroot/fake-images` for uploaded media: rejected because uploads must be durable, replaceable, and not tied to source-controlled fake assets.
- Hard-code an absolute upload path: rejected by Aspire/configuration rules and container portability.

## Decision: Validate images at the API/use-case boundary before domain mutation

**Rationale**: Unsupported, unreadable, oversized, or suspicious images must be rejected with clear validation errors before product events are appended. Boundary validation should check declared and detected content type, extension, maximum bytes, image decodability/dimensions where practical, and media-store write success. Domain invariants should still enforce legal asset ownership and one-main-image cardinality.

**Alternatives considered**:
- Validate only in Blazor: rejected because API and bootstrap paths would bypass protection.
- Validate only after event append with asynchronous repair: rejected because invalid images could appear in read models and event history.

## Decision: Model variants as ProductAggregate-owned child entities changed through event-sourced commands

**Rationale**: Variants belong to exactly one product and duplicate prevention is scoped within a product. ProductAggregate should expose methods to add/update/remove variants, attach/remove variant photos, and set/replace the product main image. Events should include enough data for projections to rebuild product details/list views without loading image content.

**Alternatives considered**:
- A separate Variant aggregate: rejected for MVP because duplicate validation, ownership, and product page editing are product-scoped and do not require independent variant lifecycle/concurrency yet.
- Store variants only in Web static DTOs: rejected because the feature requires saved/reopened product behavior and bootstrap data for real ProductDomain state.

## Decision: Backoffice UI remains in existing ProductsArea and uses typed service methods with multipart-capable requests

**Rationale**: The feature spec requires the existing backoffice product page. Razor components should stay under `src/CookieCommerce.Web/Components/Backoffice/ProductsArea`, keep state local or in a focused scoped view-model service, and access backend behavior only through typed services under `src/CookieCommerce.Web/Services`. Upload interactions can use `InputFile` and typed service calls that submit multipart form data or a pre-upload request, then bind returned media references to product/variant form state.

**Alternatives considered**:
- Add a separate variant management page: rejected by the existing-page requirement.
- Call ProductDomain endpoints directly from Razor components: rejected by frontend standards requiring typed services.

## Decision: Bootstrap defaults come from existing static inventory/variant data and fail fast on missing image files

**Rationale**: Static defaults already define product main images and variant images in `StaticInventoryList`. Bootstrap should map every static inventory product to ProductDomain seed commands, including all variants and media references/copies. A validation step should confirm referenced files exist and report the product reference, variant name, and missing path if not.

**Alternatives considered**:
- Seed products without images when files are missing: rejected because success criteria require no broken image references.
- Duplicate a second defaults file manually: rejected because the spec names existing static variants as the authoritative defaults.

## Decision: No cross-domain integration events are required for this feature

**Rationale**: Product variants and product media are owned by ProductDomain and surfaced in Backoffice Web. Existing Product snapshot publication may need to include variant/media data if current consumers depend on product snapshots, but no new business decision crosses bounded contexts for the requested backoffice workflow.

**Alternatives considered**:
- Publish media-created events to all domains: rejected because no other bounded context has a stated need for media lifecycle decisions.
