# Contract: Existing Backoffice Product Page Variant/Image Workflow

## Surface

- The user-facing surface remains the existing backoffice product page and ProductsArea components under `src/CookieCommerce.Web/Components/Backoffice/ProductsArea`.
- Do not create a separate variant-management page for this feature.
- Product and variant behavior must be accessed through typed services under `src/CookieCommerce.Web/Services`; Razor components must not call backend APIs directly.

## Product-level main image behavior

- The product form displays one main image slot.
- If no main image exists, the slot shows an empty/upload state.
- If a main image exists, the slot shows a preview, filename/alt text where available, and replace/remove affordances.
- Uploading a new main image replaces the existing product-level main image association; it does not add a second product main image.
- Changing/removing the product main image does not alter variant photos.

## Variant behavior

- The product page allows adding one or more variants before saving.
- Each variant row/card has required fields for variant name and price.
- Duplicate variant entries within the same product are blocked with a row-specific message.
- Existing variants are visible when the product is reopened.
- Removing a variant removes that variant from the current product view and prevents its photos from appearing under another variant.

## Variant photo behavior

- Each variant supports one or more photo uploads.
- Each uploaded photo preview is displayed only within its owning variant row/card.
- Invalid image uploads show validation messages near the affected upload control.
- Product and other variant fields are preserved where practical after an image validation failure.

## Image validation display

The UI must communicate:
- Accepted file types.
- Maximum file size.
- File-specific validation errors for unsupported, oversized, unreadable, or failed uploads.
- Pending/retry state if upload/storage is asynchronous or slow.

## Accessibility and responsive behavior

- Upload controls have visible labels.
- Image previews have meaningful alt text or staff-editable alt text.
- Add/remove/replace controls are keyboard reachable.
- Validation messages are associated with their inputs.
- Layout remains usable on mobile and desktop widths and does not rely on hover-only controls.

## Bootstrap visibility

When bootstrap defaults are loaded, the existing backoffice product page displays:
- Every product with at least one variant.
- Each available product main image.
- Each available variant photo under the correct variant.
- Clear startup/bootstrap failure reporting if a default image path is missing.
