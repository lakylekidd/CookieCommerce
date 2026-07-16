# Contract: Product Variants and Product Media

This contract describes the externally visible interfaces needed by the Web backoffice and bootstrap paths. Exact route names may follow existing `ProductController` conventions, but request/response DTOs must keep `*Request`/`*Response` suffixes in the API layer and commands/queries must keep `*Command`/`*Query` suffixes in Application.

## Read products for backoffice

**Interface**: `GET /api/product` and `GET /api/product/{id}` extend existing product responses.

**Response fields added to product details/list where applicable**:
- `mainImage`: nullable product media summary.
- `variants`: collection of variant summaries/details.

```json
{
  "id": "602f6345-fd5c-4a1d-a2a5-fd345f36e7f8",
  "name": "Chocolate Chunk",
  "description": "...",
  "price": 1.99,
  "isActive": true,
  "mainImage": {
    "id": "350a9ef0-650c-4043-82a1-859f6b858377",
    "url": "/media/products/602f.../main/top-tr.webp",
    "altText": "Chocolate Chunk cookie",
    "contentType": "image/webp"
  },
  "variants": [
    {
      "id": "7f57f243-eae2-4d5b-8b63-507fc2c164fe",
      "name": "Single Cookie",
      "price": 1.99,
      "currencySymbol": "€",
      "isActive": true,
      "photos": [
        {
          "id": "b97ebd19-f141-47ea-b87f-4f85bb878326",
          "url": "/media/products/602f.../variants/7f57.../single.webp",
          "altText": "Single Chocolate Chunk cookie",
          "contentType": "image/webp"
        }
      ]
    }
  ]
}
```

## Create/update product with variants

**Interface**: Existing create/update product operations must accept variant details either inline or through focused variant commands.

**Minimum request shape**:

```json
{
  "name": "Chocolate Chunk",
  "description": "Large chocolate chunk cookie",
  "price": 1.99,
  "mainImageAssetId": "350a9ef0-650c-4043-82a1-859f6b858377",
  "variants": [
    {
      "id": "7f57f243-eae2-4d5b-8b63-507fc2c164fe",
      "name": "Single Cookie",
      "price": 1.99,
      "currencySymbol": "€",
      "photoAssetIds": ["b97ebd19-f141-47ea-b87f-4f85bb878326"]
    }
  ]
}
```

**Rules**:
- Duplicate variant names within a product return validation errors before events are appended.
- Missing required variant fields return validation errors associated with the variant row/index.
- A product can have at most one current `mainImageAssetId`.
- Variant photo assets must target a variant belonging to the product.

## Upload product main image

**Interface**: `POST /api/product/{productId}/media/main-image` or equivalent typed service operation.

**Encoding**: `multipart/form-data`.

**Parts**:
- `file`: required image file.
- `altText`: optional string.

**Success response**: `201 Created` or `200 OK` with `ProductMediaAssetResponse`.

```json
{
  "id": "350a9ef0-650c-4043-82a1-859f6b858377",
  "productId": "602f6345-fd5c-4a1d-a2a5-fd345f36e7f8",
  "variantId": null,
  "role": "MainImage",
  "url": "/media/products/602f.../main/top-tr.webp",
  "originalFileName": "top-tr.png",
  "contentType": "image/png",
  "lengthBytes": 384200,
  "altText": "Chocolate Chunk cookie"
}
```

**Validation failures**:
- `400 ValidationProblem` for unsupported content type, oversized file, unreadable image, missing file, or invalid alt text.
- Response should keep field names stable for Web validation display (`file`, `altText`).

## Upload variant photo

**Interface**: `POST /api/product/{productId}/variants/{variantId}/photos` or equivalent typed service operation.

**Encoding**: `multipart/form-data`.

**Parts**:
- `file`: required image file.
- `altText`: optional string.
- `sortOrder`: optional integer.

**Success response**: `201 Created` with `ProductMediaAssetResponse` using role `VariantPhoto`.

**Validation failures**:
- `404` if product or variant does not exist.
- `400 ValidationProblem` if variant does not belong to product, file invalid, or image rules fail.

## Remove media

**Interfaces**:
- `DELETE /api/product/{productId}/media/main-image/{mediaAssetId}` clears current main image if it matches.
- `DELETE /api/product/{productId}/variants/{variantId}/photos/{mediaAssetId}` removes a variant photo association.

**Rules**:
- Removing product main image must not remove variant photos.
- Removing variant photos must not affect product main image or other variants.
- Physical media deletion may be immediate or cleanup-driven, but removed media must disappear from current product read models.

## Web typed service contract

`src/CookieCommerce.Web/Services` should expose typed methods rather than direct component HTTP calls. If `IBackofficeService` changes, update `Documentation/backend-documentation.md` in the same implementation change.

Required capabilities:
- Get product details including variants/images.
- Create/update product variants from existing product page.
- Upload/replace product main image.
- Upload/remove variant photos.
- Surface validation errors without clearing unrelated form state.
