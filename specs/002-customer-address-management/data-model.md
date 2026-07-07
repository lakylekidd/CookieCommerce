# Data Model: Customer Address Management

## Customer

**Description**: Authenticated customer account that owns an address book.

**Source of truth**: CustomerDomain customer aggregate event stream.

**Fields relevant to this feature**:

| Field | Type | Required | Notes |
|---|---|---:|---|
| `CustomerId` | GUID | Yes | Derived from the authenticated customer for customer-facing flows. |
| `AddressBook` | Collection of Customer Address | No | Active saved addresses owned by this customer. |
| `ForgottenDateUtc` | Timestamp nullable | No | Forgotten customers must not expose address data. |

**Rules**:

- A customer can retrieve, add, or delete only their own addresses in customer-facing flows.
- Unauthenticated visitors cannot access address-book operations.
- Forgotten customers have no readable address-book projection.

## Customer Address

**Description**: A saved delivery/billing address entry in a customer's address book.

**Fields**:

| Field | Type | Required | Validation | PII handling |
|---|---|---:|---|---|
| `AddressId` | Stable identifier | Yes | Unique within the customer address book; not reused after deletion. | Not PII. |
| `Index` | Integer/display ordinal | Derived | Stable enough for existing UI selection while active; may be derived from order. | Not PII. |
| `Label` | Text | Yes | Non-empty after trimming; unique among active addresses for the same customer. | Customer-entered text; treat as PII-adjacent. |
| `Icon` | Text nullable | No | Optional display hint only. | Not sensitive unless customer-entered. |
| `FirstName` | Text | Yes | Non-empty after trimming; length-bounded. | Encrypt before event storage. |
| `LastName` | Text | Yes | Non-empty after trimming; length-bounded. | Encrypt before event storage. |
| `Street` | Text | Yes | Non-empty after trimming; length-bounded. | Encrypt before event storage. |
| `HouseNumber` | Text | Yes | Non-empty after trimming; supports local formats. | Encrypt before event storage. |
| `HouseNumberAddition` | Text nullable | No | Trimmed; length-bounded. | Encrypt before event storage when present. |
| `City` | Text | Yes | Non-empty after trimming; length-bounded. | Encrypt before event storage. |
| `PostalCode` | Text | Yes | Non-empty after trimming; normalized for display/search where applicable. | Encrypt before event storage. |
| `Province` | Text | Yes | Non-empty when required by the current customer-facing address shape. | Encrypt before event storage. |
| `Country` | Text | Yes | Non-empty; accepts existing UI country format. | Encrypt before event storage. |
| `Phone` | Text | Yes | Non-empty; normalized or validated at boundary. | Encrypt before event storage. |
| `IsDefault` | Boolean | Yes | At most one active address per customer can be default. | Not PII. |
| `CreatedAtUtc` | Timestamp | Yes | Supplied by application boundary for ordering/audit. | Not PII. |
| `DeletedAtUtc` | Timestamp nullable | No | Set by delete event or represented by removal from active projection. | Not PII. |

**Validation rules**:

- Required text fields reject null, empty, and whitespace-only values.
- Required fields are length-bounded to protect storage, logs, and UI display.
- Duplicate active labels for the same customer are rejected.
- Invalid identifiers or labels for deletion produce not-found behavior and no state change.

**State transitions**:

1. `NotSaved`: address exists only in the user's submitted form.
2. `Active`: add succeeds; address appears in the customer's address book.
3. `Default`: active address is selected as the default; all other active addresses for the customer are non-default.
4. `Deleted`: delete succeeds; address no longer appears in customer-facing address-book reads.

## Address Book

**Description**: Customer-owned collection of active saved addresses with default-address invariant.

**Relationships**:

- Belongs to exactly one Customer.
- Contains zero or more active Customer Addresses.
- Projected into customer read models for profile and checkout address selection.

**Rules**:

- Empty address book has zero default addresses.
- Non-empty address book has exactly one default address.
- Adding the first address makes it default.
- Adding an address marked as default unsets the previous default.
- Adding a non-default address preserves the existing default if one exists.
- Deleting the default address promotes the oldest remaining active address to default.
- Deleting a non-default address preserves the current default.
- Repeated delete of the same active address is safe from data-corruption perspective and returns a not-found/already-removed outcome.

## Customer Address Read Model

**Description**: Decrypted customer-facing address view for profile and checkout flows.

**Source**: Projection from CustomerDomain address events after decrypting with the customer's key.

**Fields**:

| Field | Notes |
|---|---|
| `CustomerId` | Owner; used for authorization/filtering only. |
| `Addresses` | Active decrypted address DTOs in deterministic display order. |
| `DefaultAddressId` or `IsDefault` | Represents default state for selection. |

**Projection rules**:

- Customer creation initializes an empty address list.
- Address-added events append or insert an active address and apply default rules.
- Address-removed events remove the active address and apply default reassignment rules.
- Customer-forgotten/deleted events delete the read model or remove address data so decrypted PII is no longer available.

## Web Address DTO

**Description**: Customer-facing UI model used by account and checkout components.

**Fields**: Mirrors the existing Web address shape: index, icon, label, first name, last name, street, house number, house-number addition, city, postal code, province, country, phone, and default flag.

**Rules**:

- UI DTOs must not expose CustomerDomain aggregate/event types.
- Web services map CustomerDomain response data to the existing UI shape.
- After add or delete succeeds, cached/profile state must refresh so profile and checkout see current addresses.
