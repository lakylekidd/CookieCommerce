# Feature Specification: Customer Address Management

**Feature Branch**: `002-customer-address-management`

**Created**: 2026-07-07

**Status**: Draft

**Input**: User description: "I would like to add the ability to get, add, and delete addresses. Currently this implementation is done by a fakeaccountservice.cs. I would like this implementation to be done directly by the customer domain."

## Clarifications

### Session 2026-07-07

- Q: Where should fake addresses for Keycloak-bootstrapped users be stored and served from? → A: Dev-bootstrap creates/syncs each fake user's addresses into CustomerDomain state/read models.
- Q: Which component should own development bootstrap orchestration for fake users and addresses? → A: New dev-bootstrap library orchestrates fake user creation plus CustomerDomain address seeding; Keycloak service becomes identity adapter only.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - View saved addresses (Priority: P1)

As an authenticated customer, I want to view the addresses saved to my account so that I can manage my delivery and billing details from my profile and checkout flows.

**Why this priority**: Viewing the current address book is the base capability required before customers can decide whether to add or remove an address.

**Independent Test**: Can be fully tested by signing in as a customer with existing saved addresses and confirming the profile or checkout address list shows only that customer's current addresses.

**Acceptance Scenarios**:

1. **Given** an authenticated customer with saved addresses, **When** the customer opens their address book, **Then** all active saved addresses for that customer are shown with recipient, label, street, house number, postal code, city, province, country, phone, and default-address status.
2. **Given** an authenticated customer with no saved addresses, **When** the customer opens their address book, **Then** the customer sees an empty-address state instead of mock or unrelated data.
3. **Given** one customer has saved addresses and another customer signs in, **When** the second customer views their address book, **Then** the first customer's addresses are not visible.

---

### User Story 2 - Add a saved address (Priority: P2)

As an authenticated customer, I want to add a valid address to my account so that I can reuse it for future deliveries and checkout.

**Why this priority**: Adding addresses is the primary way customers maintain reusable delivery information and eliminates reliance on fake account data.

**Independent Test**: Can be fully tested by signing in as a customer, submitting a valid new address, and confirming it appears in that customer's address list after the save completes.

**Acceptance Scenarios**:

1. **Given** an authenticated customer, **When** the customer submits a complete valid address, **Then** the address is saved to that customer's address book and appears in subsequent address retrievals.
2. **Given** an authenticated customer submits an address missing required recipient or location details, **When** the save is requested, **Then** the address is not saved and the customer receives field-level validation feedback.
3. **Given** an authenticated customer adds their first address, **When** the save succeeds, **Then** that address becomes the default address.
4. **Given** an authenticated customer adds an address marked as the default, **When** the save succeeds, **Then** that address becomes the only default address for the customer.

---

### User Story 3 - Delete a saved address (Priority: P3)

As an authenticated customer, I want to delete an address I no longer use so that my account only offers relevant delivery choices.

**Why this priority**: Deletion completes basic address-book management and prevents stale personal data from remaining visible in customer flows.

**Independent Test**: Can be fully tested by signing in as a customer with at least two saved addresses, deleting one address, and confirming it no longer appears while the remaining addresses are unchanged.

**Acceptance Scenarios**:

1. **Given** an authenticated customer with saved addresses, **When** the customer deletes one of their addresses, **Then** that address is removed from future address retrievals for that customer.
2. **Given** an authenticated customer tries to delete an address that does not belong to them or no longer exists, **When** deletion is requested, **Then** no other customer's data changes and the customer receives a not-found or already-removed outcome.
3. **Given** an authenticated customer deletes their default address while other addresses remain, **When** deletion succeeds, **Then** exactly one remaining address is treated as the default.
4. **Given** an authenticated customer deletes their only address, **When** deletion succeeds, **Then** the address book becomes empty and no default address remains.

---

### Edge Cases

- A customer submits blank, whitespace-only, overlong, or malformed address fields.
- A customer repeats an add or delete action after a timeout or page refresh.
- A customer attempts to access addresses while unauthenticated or after their session is no longer valid.
- A delete request targets an address that was already deleted or has an invalid identifier.
- Saved addresses contain personal data and must remain protected in stored history, customer-facing responses, and customer erasure flows.
- Address data used by profile and checkout views may update slightly after a save; customers should not see stale mock data after a successful change.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST allow an authenticated customer to retrieve the active addresses associated with their own customer account.
- **FR-002**: The system MUST return an empty address list when an authenticated customer has no saved addresses.
- **FR-003**: The system MUST allow an authenticated customer to add a new address to their own customer account.
- **FR-004**: The system MUST validate required address details before saving, including recipient name, street, house number, city, postal code, province or region when applicable, country, phone number, and customer-visible label.
- **FR-005**: The system MUST reject invalid address submissions without changing the customer's existing address book.
- **FR-006**: The system MUST allow an authenticated customer to delete an existing address from their own customer account.
- **FR-007**: The system MUST prevent customers from retrieving, adding, or deleting addresses for another customer account.
- **FR-008**: The system MUST ensure at most one active address is marked as the default address for a customer.
- **FR-009**: The system MUST automatically designate the first saved address as the default address.
- **FR-010**: The system MUST preserve a valid default-address state when the current default address is deleted and other addresses remain.
- **FR-011**: The system MUST make address changes visible to customer profile and checkout address-selection flows without relying on fake or static account data.
- **FR-012**: The system MUST provide clear success, validation, unauthorized, and not-found outcomes for address retrieval, addition, and deletion.
- **FR-013**: The system MUST protect saved address information as customer personal data and preserve existing customer erasure behavior.
- **FR-014**: The system MUST record address-book changes as customer-owned domain state so address history is auditable within the customer domain.
- **FR-015**: The system MUST seed each development-bootstrap fake customer with multiple fake addresses in CustomerDomain-owned address state or read models, so the current-customer address endpoint returns those addresses without reading address data from Keycloak attributes or fake account services.
- **FR-016**: Development bootstrap orchestration MUST live outside Authentication Management in a dedicated dev-bootstrap library; Keycloak-specific code is limited to identity user creation/synchronization for that bootstrap flow.

### Key Entities *(include if feature involves data)*

- **Customer**: The account owner whose address book is being managed; owns all saved address entries and default-address rules.
- **Customer Address**: A saved address entry containing a customer-visible label, recipient name, street and house-number details, city, postal code, province or region, country, phone number, active/deleted status, and default-address status.
- **Address Book**: The customer's collection of active saved addresses, including rules for retrieval order and default-address selection.

### Architecture & Compliance Notes *(mandatory when applicable)*

- **Bounded Context**: CustomerDomain owns this feature because address-book entries are customer profile data and must no longer be served from fake account data.
- **Backend/API Impact**: Customer-facing address retrieval, add, and delete operations are required for the currently authenticated customer; public contracts must not expose internal domain state directly. The customer controller must expose an endpoint that returns all active addresses for the currently authenticated customer.
- **State Changes**: Adding and deleting addresses change customer-owned domain state and should produce auditable customer-domain history.
- **Read Models/Projections**: Customer profile and checkout address-selection views need a current customer address-book representation.
- **Integration Events**: No cross-domain business decision is introduced by this feature; if other domains later require customer address snapshots, they must receive them asynchronously rather than by synchronous customer-domain calls.
- **PII/Data Protection**: Addresses, recipient names, and phone numbers are personal data and must preserve encrypted-at-rest handling, clear-text exposure only at approved customer-facing boundaries, and customer erasure semantics.
- **Frontend/UI Impact**: Existing customer profile and checkout address-management flows should display real customer-domain address data and user-friendly validation results.
- **Aspire/Containerization Impact**: No new runtime service is expected; existing customer-domain runtime composition should remain valid.
- **Bootstrap/Test Data Impact**: A dedicated dev-bootstrap library should orchestrate fake user creation and CustomerDomain-owned fake address seeding for each bootstrapped user. Keycloak-specific services should act only as identity adapters, keeping identity data in Keycloak separate from address-book domain state.
- **Service Interface Documentation**: Customer-facing service documentation must be updated because address operations move from fake account behavior to customer-domain behavior.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100% of authenticated address-book views show addresses owned by the signed-in customer and never show fake, static, or another customer's addresses.
- **SC-002**: At least 95% of valid address additions are visible to the same customer in profile or checkout flows within 2 seconds of completion.
- **SC-003**: 100% of invalid address submissions are rejected with customer-understandable feedback and without modifying existing saved addresses.
- **SC-004**: 100% of successful deletions remove the address from subsequent customer address-book views while leaving unrelated addresses unchanged.
- **SC-005**: Across all add and delete operations, each customer has either zero default addresses when no addresses exist or exactly one default address when at least one address exists.
- **SC-006**: Customer support or tester verification can complete the full view-add-delete address flow in under 3 minutes using an authenticated customer account.

## Assumptions

- The feature is limited to authenticated customers managing their own address book; administrative address management is out of scope.
- The current address-book fields used by profile and checkout remain the expected customer-facing data shape.
- The first saved address becoming the default is acceptable when the customer has no existing default address.
- When a default address is deleted and other addresses remain, the system may choose the oldest remaining active address as the new default unless a future feature introduces explicit default selection.
- Address updates are out of scope for this feature; customers can add a corrected address and delete the outdated one.
- Existing authentication and customer identity resolution are reused for determining the current customer.
