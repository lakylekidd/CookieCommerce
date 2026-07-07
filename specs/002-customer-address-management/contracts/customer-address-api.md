# Contract: Customer Address API

This contract describes the CustomerDomain customer-facing address-book operations required to replace fake Web address behavior. Existing endpoints may be reused only if they satisfy the same ownership, validation, privacy, and response behavior.

## Security and ownership

- All endpoints require an authenticated customer session.
- Customer-facing address endpoints derive the customer id from the authenticated context; clients must not supply another customer's id for these operations.
- Responses and validation errors must not expose encrypted payloads, customer keys, raw event data, or another customer's address data.
- Address values are personal data; only approved customer-facing responses return clear text.

## Address response shape

```json
{
  "id": "018f2f5c-6f65-7bc2-9bc3-534db0455c2a",
  "index": 1,
  "icon": "home",
  "label": "Home",
  "firstName": "Jamie",
  "lastName": "Baker",
  "street": "Cookie Lane",
  "houseNumber": "12",
  "houseNumberAddition": "A",
  "city": "Baker City",
  "postalCode": "1234 AB",
  "province": "North Brabant",
  "country": "Netherlands",
  "phone": "+31 612345678",
  "isDefault": true
}
```

`id` is the preferred stable identifier. `index` remains available for existing Web address-selection components during migration.

## GET `/api/customer/me/addresses`

Returns active addresses for the authenticated customer's address book.

### Success response: `200 OK`

```json
[
  {
    "id": "018f2f5c-6f65-7bc2-9bc3-534db0455c2a",
    "index": 1,
    "icon": "home",
    "label": "Home",
    "firstName": "Jamie",
    "lastName": "Baker",
    "street": "Cookie Lane",
    "houseNumber": "12",
    "houseNumberAddition": null,
    "city": "Baker City",
    "postalCode": "1234 AB",
    "province": "North Brabant",
    "country": "Netherlands",
    "phone": "+31 612345678",
    "isDefault": true
  }
]
```

If the customer has no active saved addresses, return `200 OK` with an empty array.

### Error responses

| Status | Meaning |
|---:|---|
| `401 Unauthorized` | Visitor is not signed in. |
| `403 Forbidden` | Authenticated identity cannot be resolved to a customer profile or policy forbids the request. |
| `404 Not Found` | Customer profile does not exist or has been forgotten. |

## POST `/api/customer/me/addresses`

Adds a valid address to the authenticated customer's address book.

### Request body

```json
{
  "icon": "home",
  "label": "Home",
  "firstName": "Jamie",
  "lastName": "Baker",
  "street": "Cookie Lane",
  "houseNumber": "12",
  "houseNumberAddition": null,
  "city": "Baker City",
  "postalCode": "1234 AB",
  "province": "North Brabant",
  "country": "Netherlands",
  "phone": "+31 612345678",
  "isDefault": true
}
```

### Field rules

| Field | Required | Rules |
|---|---:|---|
| `icon` | No | Optional display hint. |
| `label` | Yes | Non-empty after trimming; unique among active addresses for this customer. |
| `firstName` | Yes | Non-empty after trimming. |
| `lastName` | Yes | Non-empty after trimming. |
| `street` | Yes | Non-empty after trimming. |
| `houseNumber` | Yes | Non-empty after trimming. |
| `houseNumberAddition` | No | Trim when supplied. |
| `city` | Yes | Non-empty after trimming. |
| `postalCode` | Yes | Non-empty after trimming. |
| `province` | Yes | Non-empty for the current Web address shape. |
| `country` | Yes | Non-empty. |
| `phone` | Yes | Non-empty; must be acceptable for customer contact/delivery. |
| `isDefault` | No | If true, this address becomes the only default. If omitted/false and this is the first address, the new address still becomes default. |

### Success response: `201 Created`

Returns the newly saved address as it appears in the customer's address book after default rules are applied.

```json
{
  "id": "018f2f5c-6f65-7bc2-9bc3-534db0455c2a",
  "index": 1,
  "icon": "home",
  "label": "Home",
  "firstName": "Jamie",
  "lastName": "Baker",
  "street": "Cookie Lane",
  "houseNumber": "12",
  "houseNumberAddition": null,
  "city": "Baker City",
  "postalCode": "1234 AB",
  "province": "North Brabant",
  "country": "Netherlands",
  "phone": "+31 612345678",
  "isDefault": true
}
```

### Error responses

| Status | Meaning |
|---:|---|
| `400 Bad Request` | Request body cannot be read. |
| `401 Unauthorized` | Visitor is not signed in. |
| `403 Forbidden` | Authenticated identity cannot be resolved to a customer profile or policy forbids the request. |
| `404 Not Found` | Customer profile does not exist or has been forgotten. |
| `409 Conflict` | An active address with the same label already exists for this customer. |
| `422 Unprocessable Entity` or validation problem | Field-level validation failed. |

### Validation problem shape

```json
{
  "title": "One or more validation errors occurred.",
  "status": 422,
  "errors": {
    "label": ["Label is required."],
    "postalCode": ["Postal code is required."]
  }
}
```

Validation errors must identify fields without echoing unnecessary personal data.

## DELETE `/api/customer/me/addresses/{addressId}`

Deletes an active address from the authenticated customer's address book.

### Path parameters

| Parameter | Required | Rules |
|---|---:|---|
| `addressId` | Yes | Must identify an active address owned by the authenticated customer. |

### Success response: `204 No Content`

The address no longer appears in `GET /api/customer/me/addresses`. If the deleted address was the default and other addresses remain, exactly one remaining address is default.

### Error responses

| Status | Meaning |
|---:|---|
| `400 Bad Request` | Address identifier is malformed. |
| `401 Unauthorized` | Visitor is not signed in. |
| `403 Forbidden` | Authenticated identity cannot be resolved to a customer profile or policy forbids the request. |
| `404 Not Found` | Address does not exist, is already deleted, belongs to another customer, or customer profile does not exist. |

## GET `/api/customer/me/addresses/default`

Returns the default active address for the authenticated customer, if one exists. This supports existing default-shipping display and checkout selection behavior.

### Success responses

- `200 OK` with an address response when at least one active address exists.
- `204 No Content` or `404 Not Found` when no default address exists because the customer has no active addresses. The implementation must document which no-default response is used and Web must handle it gracefully.

## Web service contract expectations

- Existing Web address consumers can keep calling account-service address methods, but those methods must delegate to CustomerDomain for get/add/delete/default address operations.
- After successful add or delete, Web refreshes local profile/address state from CustomerDomain so dashboard and checkout show current address data.
- Customer-facing flows must not silently fall back to `FakeAccountService`, static profiles, or static address lists when CustomerDomain calls fail.
