# Research: Customer Address Management

## Decision: CustomerDomain owns address-book state and contracts

**Rationale**: Addresses are customer profile PII and the feature explicitly moves get/add/delete behavior away from `FakeAccountService`. CustomerDomain already contains a customer aggregate, address events, address commands, a read model, and customer profile API surface that can be extended to cover the complete customer-facing address shape.

**Alternatives considered**:

- Keep the fake Web service and synchronize later: rejected because it preserves mock state as the source of truth.
- Create a separate Address bounded context: rejected because saved addresses are part of customer profile ownership and default-address rules belong to the customer aggregate.
- Use route-supplied customer ids for customer address management: rejected for customer-facing flows because customers must manage only their own addresses.

## Decision: Model address changes as Customer aggregate events

**Rationale**: The constitution requires durable business state to change through event-sourced aggregates. Address addition and deletion are auditable customer-owned state changes. Existing `CustomerAddressAdded` and `CustomerAddressRemoved` events provide the starting point but need to support the full UI address fields and default-address invariant.

**Alternatives considered**:

- Store address rows directly in a mutable table: rejected because it bypasses the established CustomerDomain event-sourced model.
- Store addresses only in a Web session/local state: rejected because profile and checkout need durable customer data and customer erasure support.

## Decision: Encrypt address PII before immutable event storage and decrypt only for approved read models/responses

**Rationale**: Saved addresses include recipient names, street address, postal code, city, province, country, and phone number. The constitution requires sensitive PII to be encrypted before storage in domain events and erased through customer-key crypto-shredding. Existing customer profile encryption/key-store patterns should be reused for address event payloads.

**Alternatives considered**:

- Store address values as plaintext in events: rejected because immutable event history would retain PII after customer erasure.
- Keep only encrypted address data with no read model: rejected because profile and checkout need customer-facing address reads without rebuilding streams for every display.

## Decision: Use `/api/customer/me/addresses` style customer-facing contracts

**Rationale**: The existing profile flow derives ownership from the authenticated customer context. Address management should follow the same pattern so customers cannot supply another customer's id. Existing id-based endpoints can remain only for internal/admin flows if protected by policy, but the customer-facing Web address book should use current-customer endpoints.

**Alternatives considered**:

- Use existing `/api/customer/{customerId}/addresses` endpoints from the Web app: rejected because trusting caller-supplied ids conflicts with the feature's ownership and authorization requirements.
- Fold address add/delete into profile PATCH only: rejected because address operations are separate user journeys and need independent success/validation/not-found outcomes.

## Decision: Keep default-address rules inside the customer aggregate

**Rationale**: The invariant is customer-specific: zero defaults when no addresses exist; exactly one default when at least one active address exists. The aggregate can enforce this consistently for add and delete regardless of API or UI entry point.

**Alternatives considered**:

- Let Web choose and persist default state: rejected because multiple surfaces could drift and another client could violate the invariant.
- Allow multiple defaults and fix during reads: rejected because persisted domain state would be invalid and deletion behavior would be ambiguous.

## Decision: Replace only address operations in `IAccountService`/Web flow, not unrelated fake account features

**Rationale**: `FakeAccountService` still backs favorites, mock orders, tracking, and review placeholders. This feature is scoped to get/add/delete addresses and primary-address reads impacted by those operations. Broader account-service replacement belongs to separate features.

**Alternatives considered**:

- Replace all `FakeAccountService` behavior now: rejected as larger than the requested address-management change.
- Add a second Web address service without adapting existing consumers: rejected because current account/profile/checkout components already use `IAccountService` address methods.
