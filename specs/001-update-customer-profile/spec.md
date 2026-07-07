# Feature Specification: Update Customer Profile

**Feature Branch**: `feature/001-update-customer-profile`

**Created**: 2026-07-06

**Status**: Draft

**Input**: User description: "I would like my customers to be able to edit their personal details such as first and last name, email address, gender and date of birth from the update profile in their dashboard."

## Clarifications

### Session 2026-07-06

- Q: How must GDPR compliance apply to updated customer profile data? → A: Follow `Documentation/GDPR-Compliance.md`: encrypt event-sourced profile personal data with customer-specific keys, including full name, email, gender, and date of birth; crypto-shred by deleting the key; delete decrypted read-model copies directly; and emit deletion records without personal data.
- Q: How should the dashboard handle the existing mock profile data and services? → A: Replace mock profile data and services with the actual implemented customer profile endpoints; any missing endpoint capability is in scope to complete rather than continuing to mock the flow.
- Q: When a customer changes their email address, when should the profile and login identity switch to the new email? → A: Store the new email as pending; keep profile and login using the old verified email until verification completes.
- Q: What account access behavior is required after customer deletion? → A: Deleted customers cannot log in and receive a not found response after AuthenticationManagement processes the forgotten-customer event; the same email address can be used to register a new customer account only after that asynchronous processing completes.
- Q: Which gender/profile options are allowed? → A: Male, Female, Prefer not to disclose.
- Q: Where must updated profile information be displayed? → A: Wherever customer name, email, gender, or date of birth is shown on the website.
- Q: What profile synchronization delay is acceptable after saving? → A: Updated profile details must appear immediately everywhere, visible without a manual page reload within the same successful save response/render cycle; no sync-pending state is acceptable.
- Q: Is email verification delivery and token handling in scope? → A: Token creation, storage, and validation are in scope; email delivery is handled by an external dependency.
- Q: Which user story owns login credential promotion after email verification? → A: US3 owns login credential promotion; US1 is complete with editable profile data, pending email request, and old verified email remaining active until verification.
- Q: What website profile-display discovery is required? → A: Planning/tasks must explicitly inventory every website surface that displays customer name, email, gender, or date of birth before updating those surfaces.
- Q: What is the email-verification delivery handoff contract? → A: CustomerDomain must hand off recipient pending email, customer id, verification token/reference, expiry, verification URL or route data, purpose, correlation id, and safe retry/idempotency metadata to the external email delivery dependency without logging raw tokens or unnecessary PII.
- Q: What Aspire validation is required before completion? → A: The polish/quality-gate phase must validate the affected services still participate in the Aspire composition.
- Q: What is the boundary between customer deletion and login identity? → A: CustomerDomain must not synchronously delete, disable, or modify AuthenticationManagement login identities; it publishes a forgotten-customer integration event through the outbox, and AuthenticationManagement consumes it idempotently before disabling/removing the login identity and allowing re-registration with the same email.
- Q: What storage model applies to gender and date of birth for GDPR erasure? → A: Gender and date of birth use encrypted immutable event storage with customer-specific crypto-shredding for GDPR removal, not read-model-only storage.
- Q: What concrete path must email verification delivery use? → A: CustomerDomain must define a delivery handoff contract, concrete sender/adapter, DI registration, payload, expiry, recipient, customer id, purpose, correlation/idempotency metadata, and no raw-token or unnecessary-PII logging.
- Q: When does a verified pending email become the login email? → A: Only after AuthenticationManagement consumes the verified-email integration event; CustomerDomain verification alone does not update login credentials.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Update personal details from dashboard (Priority: P1)

As an authenticated customer, I want to open the update profile area in my dashboard, edit my first name, last name, email address, gender, and date of birth, and save the changes so my account information stays accurate.

**Why this priority**: This is the core customer value. Without this journey, customers must rely on support or keep inaccurate profile information.

**Independent Test**: Can be fully tested by signing in as a customer, opening the dashboard update profile area, confirming the displayed values come from the customer's saved profile rather than sample data, changing first name, last name, gender, and date of birth to valid values, requesting an email change, saving, confirming non-email updates are shown afterwards, and confirming the old verified email remains the active profile and login email while the requested email is pending verification.

**Acceptance Scenarios**:

1. **Given** an authenticated customer is viewing their dashboard, **When** they open update profile, **Then** their current first name, last name, email address, gender, and date of birth are displayed for review and editing.
2. **Given** an authenticated customer enters valid non-email changes to their personal details, **When** they save the profile, **Then** the changes are accepted, a success confirmation is shown, and the updated details are visible on the profile page.
3. **Given** a customer saves updated profile details, **When** they leave and return to the dashboard profile area, **Then** the saved verified details remain visible, and any unverified email change is shown as pending without replacing the verified profile email.
4. **Given** a customer requests an email address change, **When** the new email address is not yet verified, **Then** the customer continues signing in with the old verified email address.

---

### User Story 2 - Prevent invalid profile updates (Priority: P2)

As a customer, I want clear validation feedback when I enter invalid or incomplete profile details so I can correct mistakes before my profile is changed.

**Why this priority**: Profile information affects account communication and customer records, so invalid data must not be accepted.

**Independent Test**: Can be tested by attempting to save invalid names, an invalid email address, an unsupported gender option, and an invalid date of birth, then confirming the customer receives clear correction guidance and no invalid changes are saved.

**Acceptance Scenarios**:

1. **Given** a customer enters an invalid email address, **When** they save the profile, **Then** the profile is not updated and the customer is told the email address must be valid.
2. **Given** a customer omits a required name field, **When** they save the profile, **Then** the profile is not updated and the customer is told which field must be completed.
3. **Given** a customer enters a date of birth in the future, **When** they save the profile, **Then** the profile is not updated and the customer is told the date of birth must be a valid past date.

---

### User Story 3 - Protect account identity and privacy (Priority: P3)

As a customer, I want profile updates to affect only my own account and protect my personal data so my identity and privacy remain secure.

**Why this priority**: Personal details are sensitive customer data. Unauthorized changes or inappropriate disclosure would damage trust and may create compliance risk.

**Independent Test**: Can be tested by verifying that an authenticated customer can only view and update their own profile, unauthenticated visitors cannot use the update profile area, and profile update confirmations do not expose unnecessary personal information.

**Acceptance Scenarios**:

1. **Given** an unauthenticated visitor attempts to access update profile, **When** they request the page, **Then** they are required to sign in before viewing or changing personal details.
2. **Given** a customer is signed in, **When** they update profile details, **Then** only that customer's own profile is changed.
3. **Given** a profile update succeeds or fails, **When** feedback is shown, **Then** the message is clear without exposing unnecessary sensitive personal details.
4. **Given** a customer verifies a pending email address, **When** verification succeeds and the verified-email change is consumed by AuthenticationManagement, **Then** the new email becomes the profile email and required login email.

### Edge Cases

- A customer attempts to change their email address to an address already used by another account; the update is rejected with a clear message and existing details remain unchanged.
- A customer has a pending unverified email address change; the current verified email remains the profile and login email until the pending address is verified.
- A customer submits no actual changes; the system treats the save as successful or informs the customer that no changes were made, without creating duplicate or conflicting profile records.
- A customer submits the form multiple times or retries after a transient failure; the final profile state remains consistent and duplicate update effects are avoided.
- A customer starts with missing optional personal details; the update profile area still loads and allows completion of supported fields.
- PII handling must preserve GDPR crypto-shredding: event-sourced profile personal data, including full name, verified email, pending email, gender, and date of birth, remains encrypted with a customer-specific key; read-model copies are deleted directly; and erasure leaves no retrievable plaintext profile data.
- If profile information is shown in dashboard read models or other customer-facing summaries, saving a profile update must refresh all customer-facing profile displays immediately; no sync-pending state or stale profile display is acceptable after a successful save.
- If the live customer profile service is unavailable or rejects a request, the dashboard must show a clear failure state and must not fall back to saving or displaying mock profile data as if it were real.
- After customer deletion, CustomerDomain must publish a forgotten-customer integration event through the outbox and must not synchronously delete, disable, or modify the AuthenticationManagement login identity. Until AuthenticationManagement idempotently processes that event, login identity removal and same-email re-registration are eventually consistent; after processing, login attempts for the deleted customer must fail with a not found response and registration with the former email address is allowed for a new customer account.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST allow authenticated customers to access an update profile area from their dashboard.
- **FR-002**: The system MUST display the customer's current first name, last name, email address, gender, and date of birth in the update profile area.
- **FR-003**: The system MUST allow the customer to edit and save first name, last name, gender, and date of birth for their own profile, and MUST allow the customer to request an email address change for their own profile.
- **FR-004**: The system MUST require first name, last name, and email address before accepting a profile update.
- **FR-005**: The system MUST validate that requested email addresses use a valid email format and are not already associated with another customer account before creating a pending email change; the email uniqueness implementation/check MUST exist in the foundational or US1 profile-update path, not first appear as a later validation refinement.
- **FR-006**: The system MUST validate that date of birth is a real calendar date in the past when provided.
- **FR-007**: The system MUST limit gender values to `Male`, `Female`, and `Prefer not to disclose`.
- **FR-008**: The system MUST reject invalid updates without changing the saved profile and MUST tell the customer which fields need correction.
- **FR-009**: The system MUST show a clear success confirmation after a valid profile update is saved.
- **FR-010**: The system MUST ensure customers can only view and update their own profile details.
- **FR-011**: The system MUST preserve GDPR privacy protections for PII according to `Documentation/GDPR-Compliance.md`: event-sourced profile personal data, including full name, verified email, pending email, gender, and date of birth, MUST remain encrypted with a customer-specific key, and erasure MUST make encrypted personal data permanently irretrievable by removing that key.
- **FR-011a**: The system MUST store gender and date of birth using the same encrypted immutable event storage model as other profile PII, and MUST remove any decrypted read-model copies during GDPR deletion without relying on event rewriting.
- **FR-012**: The system MUST make updated profile details available immediately wherever the customer's name, email address, gender, or date of birth is shown on the website after a successful save; updated values MUST be visible without a manual page reload in the same successful save response/render cycle, and no sync-pending state or stale profile display is acceptable. The implementation plan MUST inventory all such website profile-display surfaces before updating them.
- **FR-013**: The system MUST record profile changes as an auditable customer state change without exposing sensitive personal details in user-facing messages or operational logs; profile update, email verification, and deletion/forgetting log tests MUST explicitly prove logs contain no PII or raw verification tokens.
- **FR-014**: The customer dashboard update profile flow MUST use the actual implemented customer profile endpoints for loading and saving profile details, not mock data or mock services.
- **FR-015**: If an actual endpoint capability needed for the dashboard profile flow is missing or incomplete, the feature scope MUST include completing that capability rather than preserving a mock-only path.
- **FR-016**: When a customer requests an email address change, the system MUST store the requested email address as pending, create/store a verification token, and validate that token before the pending email replaces the current verified profile email; delivery of the verification email is handled by an external dependency. CustomerDomain MUST provide a concrete delivery handoff path consisting of a contract, sender/adapter, dependency-injection registration, and message payload containing the pending recipient email, customer id, token or token reference, expiry, verification route/URL data, email-change purpose, correlation id, and retry/idempotency metadata without logging raw tokens or unnecessary PII.
- **FR-017**: Until the pending email address is verified, the system MUST keep the current verified email address as the customer's profile email and login credential.
- **FR-018**: After the pending email address is successfully verified, the system MUST make the new email address the customer's profile email, and the new email MUST become the required login credential only after AuthenticationManagement consumes the verified-email integration event.
- **FR-019**: When a customer is deleted or forgotten, CustomerDomain MUST publish a forgotten-customer integration event using the outbox pattern and MUST NOT synchronously delete, disable, or modify the AuthenticationManagement login identity. AuthenticationManagement MUST consume the event idempotently and disable or remove the login identity; re-registration with the same email address MUST be allowed only after that event has been processed.

### Key Entities *(include if feature involves data)*

- **Customer Profile**: The customer-owned personal details used for account identification and dashboard display. Key attributes include first name, last name, verified email address, optional pending email address, gender (`Male`, `Female`, or `Prefer not to disclose`), and date of birth; all profile PII uses encrypted immutable event storage with decrypted read-model copies removed during GDPR deletion.
- **Customer Account Identity**: The signed-in customer identity that determines which profile may be viewed or edited and whether an email address is unique. Login uses the current verified email address until a pending replacement email is verified; customer deletion/forgetting is communicated to AuthenticationManagement asynchronously through a forgotten-customer integration event, after which the login identity is disabled or removed from active lookup so the former email can be registered again.
- **Profile Update**: A requested change to one or more supported personal details, including validation outcome and the resulting saved profile state. Email address changes produce a pending verification state before replacing the verified email.
- **Email Verification**: Proof that the customer controls a requested new email address; this feature owns verification token creation, storage, validation, and the concrete delivery handoff sender/adapter, while an external dependency handles email delivery. Successful CustomerDomain verification promotes the pending email to the verified profile email; it becomes the login email only after AuthenticationManagement consumes the verified-email integration event.

### Architecture & Compliance Notes *(mandatory when applicable)*

- **Bounded Context**: CustomerDomain owns customer profile state and deletion/forgetting decisions; AuthenticationManagement owns login identities. CustomerDomain must not synchronously delete, disable, or modify AuthenticationManagement login identities. AuthenticationManagement must be updated when a pending customer email is verified because the verified email is the sign-in credential.
- **Backend/API Impact**: Customer profile read and update behavior must expose a customer-safe contract for the dashboard while keeping business validation outside the presentation layer.
- **State Changes**: Valid updates should be represented as customer profile state changes that preserve an auditable history. Email address changes have a pending-verification state and a verified/promoted state. Customer deletion/forgetting must preserve deletion records without personal data and publish an asynchronous forgotten-customer event through the outbox; AuthenticationManagement transitions the login identity out of active login and email-uniqueness lookup only after consuming that event idempotently.
- **Read Models/Projections**: Dashboard profile read models or summaries that show personal details must reflect saved updates immediately after a successful save; the user must not see a sync-pending state or stale profile display.
- **Integration Events**: If other bounded contexts require customer profile details, changes must be shared through integration events and local read models with retry-safe handling. Customer deletion/forgetting must be shared from CustomerDomain to AuthenticationManagement through an outbox-backed integration event; AuthenticationManagement consumers must be idempotent so retries do not duplicate or corrupt login-identity removal. Email verification delivery is an external dependency; CustomerDomain must implement a concrete delivery handoff contract, sender/adapter, and DI registration that sends a payload containing the generated verification token/reference, pending recipient email, customer id, expiry, verification route/URL data, email-change purpose, correlation id, and retry/idempotency metadata without implementing a new email delivery subsystem or logging raw tokens/unnecessary PII.
- **PII/Data Protection**: First name, last name, email address, gender, and date of birth are PII. GDPR compliance must follow `Documentation/GDPR-Compliance.md`: full name, verified email, pending email, gender, and date of birth values that enter immutable customer history are encrypted with a customer-specific key; deleting that key makes those values permanently unreadable; decrypted read-model copies of profile PII are removed during GDPR deletion; deletion records contain no personal data.
- **Frontend/UI Impact**: The customer dashboard update profile experience must provide accessible labels, validation feedback, keyboard-friendly controls, visible focus states, and responsive behavior across mobile and desktop. Existing mock profile data and mock profile services must be replaced by live customer profile service integration for this flow. Any website surface that displays the customer's name, email address, gender, or date of birth must use the updated customer profile data rather than stale mock/static profile data.
- **Aspire/Containerization Impact**: No new runtime service is expected; any changed runtime surface must remain startable through the existing application composition, and the final polish/quality gate must validate the affected services still participate in the Aspire composition.
- **Service Interface Documentation**: Customer profile service contract documentation should be updated if the customer-facing service interface changes.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: At least 95% of customers in usability testing can update all supported profile fields from the dashboard without assistance.
- **SC-002**: Customers can complete a valid profile update in under 2 minutes from opening the dashboard profile area.
- **SC-003**: 100% of invalid profile update attempts in acceptance testing prevent saved changes and identify the field requiring correction.
- **SC-004**: 100% of authorization tests confirm a customer cannot view or update another customer's personal details.
- **SC-005**: Profile update related support requests decrease by at least 30% within 60 days of release compared with the prior 60-day baseline.
- **SC-006**: 100% of GDPR deletion verification tests confirm updated profile PII in immutable events, including full name, verified email, pending email, gender, and date of birth, is permanently unreadable through customer-key crypto-shredding and any decrypted read-model copies are removed.
- **SC-007**: 100% of end-to-end profile update acceptance tests verify that displayed and saved dashboard profile data comes from the actual customer profile service, updates immediately after a successful save wherever customer profile details are shown, and has no mock-data fallback in the customer-facing flow.
- **SC-008**: 100% of email-change acceptance tests confirm that login continues using the old verified email before verification and requires the new email only after AuthenticationManagement consumes the verified-email integration event.
- **SC-009**: 100% of customer deletion acceptance tests confirm CustomerDomain publishes a forgotten-customer event through the outbox without synchronously modifying AuthenticationManagement, AuthenticationManagement consumes the event idempotently, the deleted customer cannot log in and receives a not found response after processing, and same-email re-registration is allowed only after processing completes.

## Assumptions

- Customers must be authenticated before accessing dashboard profile management.
- First name, last name, and email address are required profile fields; gender and date of birth may be optional unless existing business rules require them.
- Date of birth is used only as profile information for this feature; age-restricted purchasing rules are outside this feature unless already enforced elsewhere.
- Email address changes must remain unique across customer accounts, update the customer's profile email after CustomerDomain verification, and update the login credential only after AuthenticationManagement consumes the verified-email integration event.
- Existing authentication, dashboard navigation, privacy controls, and customer erasure processes will be reused.
- Existing mock profile data and mock profile services are development placeholders and are not acceptable for the completed customer-facing dashboard profile flow.
- GDPR handling follows `Documentation/GDPR-Compliance.md`: full name, verified email, pending email, gender, and date of birth are treated as event-sourced encrypted profile personal data; decrypted read-model copies are removed during GDPR deletion.
- Administrative profile editing and customer password changes are outside the scope of this feature.
