# Feature Specification: Product Variant Images

**Feature Branch**: `master`

**Created**: 2026-07-10

**Status**: Draft

**Input**: User description: "I would like to keep working on the backoffice product page by allowing the creation of variants. Along with this feature I would like to be able to add variant photos. Maybe you can recommend the best way to store images once added to the variant. i would like also to be able to upload a photo on the product level. There should only be one photo there. This is the main image of the product. I want to use the exisitng web UI. Also update the bootstrap project to include variants for all products, along with images. You can find these defaults in the static variants."

## Clarifications

### Session 2026-07-10

- Q: Where should product name and main image edits be available after a product is selected? → A: Edit product name and main image in the selected-product area before the variant table; save with the normal product update flow.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Create product variants from the existing backoffice page (Priority: P1)

As a backoffice user managing products, I want to create variants from the existing backoffice product page, so each product can represent its available options without leaving the familiar workflow.

**Why this priority**: Variant creation is the core business capability requested. Without it, product and variant images have no useful variant-level destination.

**Independent Test**: Can be tested by opening the existing backoffice product page, entering valid variant information for a product, saving it, and confirming the product shows the newly created variants when viewed again.

**Acceptance Scenarios**:

1. **Given** an authorized backoffice user is creating or editing a product, **When** they add valid variant details and save the product, **Then** the variant is associated with that product and visible in the product management view.
2. **Given** a product can have more than one option, **When** the user adds multiple valid variants before saving, **Then** all entered variants are saved and displayed for that product.
3. **Given** a user enters incomplete or duplicate variant information, **When** they try to save, **Then** the system prevents invalid variant creation and explains what must be corrected.

---

### User Story 2 - Add photos to product variants (Priority: P2)

As a backoffice user, I want to add photos to product variants, so customers and staff can distinguish visual differences such as flavor, color, package, or size-specific presentation.

**Why this priority**: Variant photos are specifically requested and provide the visual detail needed to make variants useful in product presentation.

**Independent Test**: Can be tested by adding a photo to a variant, saving the product, returning to the product page, and confirming the photo remains associated with the correct variant.

**Acceptance Scenarios**:

1. **Given** a product variant is being created or edited, **When** the user uploads a supported image for that variant and saves, **Then** the image is retained and displayed with that variant.
2. **Given** a product has multiple variants, **When** different photos are added to different variants, **Then** each photo remains linked only to its intended variant.
3. **Given** a user uploads an unsupported, oversized, or unreadable image, **When** they attempt to use it, **Then** the system rejects the image with a clear validation message and preserves the rest of the product form where possible.

---

### User Story 3 - Add one main image at the product level (Priority: P3)

As a backoffice user, I want to upload one main image for a product, so the product has a default visual representation separate from variant-specific photos.

**Why this priority**: The product-level image is important for product display, but it is less central than the ability to create variants and attach variant-specific photos.

**Independent Test**: Can be tested by uploading a main product image, saving, returning to the product page, and confirming exactly one main image is present for the product.

**Acceptance Scenarios**:

1. **Given** an authorized backoffice user is creating a product, **When** they include a supported main image before saving, **Then** the created product stores that image as its single product-level main image.
2. **Given** a product has no main image, **When** the user uploads a supported image from the selected-product area before the variant table and saves the product update, **Then** that image becomes the product's main image.
3. **Given** a product already has a main image, **When** the user uploads a different main image from the selected-product area before the variant table and saves the product update, **Then** the new image replaces the previous main image rather than adding a second product-level image.
4. **Given** a product has variant photos, **When** the user changes the main product image, **Then** the variant photos remain unchanged.

---

### User Story 4 - Seed bootstrap products with variants and images (Priority: P4)

As a developer or demo user starting the application with bootstrap data, I want all bootstrapped products to include variants and images based on existing static variant defaults, so the backoffice and storefront have realistic sample data immediately.

**Why this priority**: Bootstrap data supports demonstrations and local validation, but it depends on the product and variant image behavior being defined first.

**Independent Test**: Can be tested by starting from the bootstrap data set and confirming every seeded product has variants and associated images that match the available static variant defaults.

**Acceptance Scenarios**:

1. **Given** the application is populated from bootstrap defaults, **When** products are loaded, **Then** every product has at least one variant.
2. **Given** static variant defaults include image references, **When** bootstrap data is created, **Then** those images are associated with the matching variants.
3. **Given** a bootstrapped product is displayed in the existing backoffice product page, **When** a user reviews it, **Then** its variants, variant photos, and single main image are visible where applicable.

### Edge Cases

- If a product is saved without variants, the system should clearly communicate whether variants are required for that product and must not silently create misleading placeholder variants.
- If a variant image upload fails after other product details are entered, the user should be able to correct the image issue without re-entering all product and variant data.
- If a product main image is replaced, the old image must no longer be presented as the product's main image.
- If a product name or main image update fails from the selected-product area, the UI should preserve the entered product name and selected image state where practical so the user can correct and retry the normal product update flow.
- If a variant is removed or replaced, its photos must not appear on another variant by mistake.
- If a product or variant image contains personal or sensitive information accidentally, normal media removal and product update workflows should allow backoffice users to remove it.
- If bootstrap defaults reference a missing image, bootstrap should report the missing media clearly rather than creating products with broken image references.
- If images take time to become available after upload, the backoffice should show a clear pending or retry state rather than implying that the image was lost.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST allow authorized backoffice users to create one or more variants for a product from the existing backoffice product page.
- **FR-002**: The system MUST allow authorized backoffice users to review existing variants for a product from the existing backoffice product page.
- **FR-003**: The system MUST validate variant entries before saving, including required variant fields and duplicate prevention within the same product.
- **FR-004**: The system MUST allow authorized backoffice users to add one or more photos to each product variant.
- **FR-005**: The system MUST keep each variant photo associated with the correct variant and product.
- **FR-006**: The system MUST allow authorized backoffice users to upload exactly one product-level main image per product during product creation and from the selected-product area before the variant table.
- **FR-007**: The system MUST replace the existing product-level main image when a different product-level image is saved through the normal product update flow.
- **FR-008**: The system MUST keep product-level images separate from variant-level photos so changing one does not unintentionally change the other.
- **FR-009**: The system MUST reject unsupported, unreadable, or oversized images with user-facing validation messages.
- **FR-010**: The system MUST preserve product and variant form data when image validation fails, where practical, so users can correct the issue without restarting the workflow.
- **FR-011**: The system MUST make saved product variants, variant photos, and the product main image visible when the product is opened again in the backoffice.
- **FR-011a**: The system MUST let authorized backoffice users edit the selected product's name in the selected-product area before the variant table and save it through the normal product update flow.
- **FR-012**: The system MUST update bootstrap data so every bootstrapped product includes variants based on the existing static variant defaults.
- **FR-013**: The system MUST update bootstrap data so bootstrapped products and variants include available image defaults from the static variant data.
- **FR-014**: The system MUST report missing or invalid bootstrap image defaults clearly during bootstrap validation or startup.
- **FR-015**: The owning bounded context is ProductDomain, with the existing backoffice product page as the affected user surface.
- **FR-016**: The system MUST treat uploaded product and variant images as product media assets linked to products or variants, with image content managed separately from business text fields and event history.
- **FR-017**: The system MUST avoid introducing new personal information requirements; product and variant images are business media and should be removable if inappropriate content is uploaded.

### Key Entities *(include if feature involves data)*

- **Product**: A merchandise item managed by backoffice users. It may have one product-level main image and one or more variants.
- **Product Variant**: A purchasable or displayable option of a product, such as a flavor, size, color, package, or other distinguishing option. It belongs to exactly one product.
- **Product Main Image**: The single default image representing a product as a whole. It is separate from variant photos.
- **Variant Photo**: An image associated with one specific product variant to represent that variant visually.
- **Product Media Asset**: A managed image record or reference used by product and variant images, including enough information to display, validate, replace, or remove the image.
- **Static Variant Defaults**: Existing default variant data used as the source for bootstrap variants and their default images.
- **Backoffice User**: An authorized staff user who manages products, variants, and images through the backoffice product page.

### Architecture & Compliance Notes *(mandatory when applicable)*

- **Bounded Context**: ProductDomain owns product, variant, and product media behavior because variants and product images are product-management concerns.
- **Backend/API Impact**: Product management capabilities must support creating variants, associating variant photos, setting or replacing the single product main image, and exposing these values to the existing backoffice UI.
- **State Changes**: Product state changes include variant creation or update, variant photo association, and product main image assignment or replacement.
- **Read Models/Projections**: Product read views used by the backoffice must include variants, variant photo information, and the product main image.
- **Integration Events**: No cross-domain integration events are required unless existing product publication workflows need product, variant, or image changes. Any asynchronous processing must be safe for retries and delayed delivery.
- **PII/Data Protection**: No new PII is intentionally collected. Product media may contain accidental sensitive content and must remain removable through product management or media cleanup workflows.
- **Frontend/UI Impact**: The existing backoffice product page remains the user-facing surface. Product creation must include a main-image upload control, and the selected-product area before the variant table must include controls to edit the product name and add or replace the single main image through the normal product update flow. The page must add variant creation and image upload capabilities without replacing the established product UI.
- **Aspire/Containerization Impact**: Runtime configuration may need to account for durable media storage; no new unrelated runtime surface is expected.
- **Service Interface Documentation**: `Documentation/backend-documentation.md` must change if product service interfaces used by the web UI are added, removed, or altered.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100% of tested product creation or edit flows allow an authorized backoffice user to add at least one valid variant without leaving the existing backoffice product page.
- **SC-002**: 100% of saved variants retain their correct product association and display correctly when the product is reopened.
- **SC-003**: 100% of accepted variant image uploads remain linked to the intended variant after save, page refresh, and product reload.
- **SC-004**: 100% of products have no more than one product-level main image after repeated uploads and replacements.
- **SC-005**: At least 95% of common valid product and variant image uploads complete without requiring users to re-enter product or variant details.
- **SC-006**: 100% of bootstrapped products include variants, and all image defaults available in the static variant data are represented without broken image references.
- **SC-007**: In acceptance testing, backoffice users can complete the combined product, variant, and image workflow in under 5 minutes for a product with three variants.
- **SC-008**: 100% of tested selected-product views expose product name editing and main-image add-or-replace controls before the variant table.

## Assumptions

- The target users are authorized backoffice staff who already have access to product management.
- The existing backoffice product page and UI remain the baseline; a full visual redesign or separate variant-management page is out of scope.
- Product name and main image updates after product selection use the normal product update flow from the selected-product area before the variant table.
- A product may have multiple variants; each variant may have one or more photos.
- A product has exactly one product-level main image at most; replacing the image is allowed, but storing multiple product-level main images is out of scope.
- Static variant defaults are the authoritative source for initial bootstrap variant values and default image references.
- Accepted image types and size limits will follow common web-commerce expectations and should be communicated in the UI.
- Recommended media approach: uploaded images should be stored as durable product media assets and linked to the product or variant by reference, rather than embedded directly inside product text fields or permanent business history.
- No new customer-facing purchase behavior is required by this feature unless existing product displays already consume product and variant image data.
