# Data Model: Update Customer Profile

## Customer Profile

**Description**: Customer-owned personal details displayed and edited from the dashboard.

**Source of truth**: `CustomerAggregate` event stream in CustomerDomain, projected to `CustomerReadModel` for reads.

**Fields**:

| Field | Type | Required | Validation | GDPR handling |
|---|---|---:|---|---|
| `CustomerId` | GUID | Yes | Must identify the authenticated customer's own profile | Not PII by itself, but do not expose for another customer |
| `FirstName` | Text | Yes | Non-empty after trimming | Event-sourced personal data; encrypt with customer-specific key before event storage |
| `LastName` | Text | Yes | Non-empty after trimming | Event-sourced personal data; encrypt with customer-specific key before event storage |
| `VerifiedEmail` | Email text | Yes | Valid email format; unique across customers | Event-sourced personal data; encrypt with customer-specific key before event storage |
| `PendingEmail` | Email text nullable | No | Valid email format; uniqueness across verified and pending customer emails except current profile must be checked before creating pending state in Foundation/US1 | PII; encrypt if stored in immutable events; clear after verification or cancellation |
| `EmailVerificationRequestedAtUtc` | Timestamp nullable | No | Set when `PendingEmail` is created | Not PII; useful for expiry/audit |
| `VerificationTokenReference` | Text/reference nullable | No | Created when `PendingEmail` is created; raw token must never be stored in events or logs | Treat as secret operational data; store hashed/reference form only |
| `VerificationExpiresAtUtc` | Timestamp nullable | No | Used to reject stale verification tokens | Not PII |
| `Gender` | Enum/string | No | Supported values only: `Male`, `Female`, `Prefer not to disclose` | Event-sourced personal data for this feature; encrypt with the customer-specific key before event storage and remove decrypted read-model value on GDPR deletion |
| `DateOfBirth` | Date nullable | No | Real date in the past; no future date | Event-sourced personal data for this feature; encrypt with the customer-specific key before event storage and remove decrypted read-model value on GDPR deletion |
| `Addresses` | Collection | No | Existing address rules apply | Out of scope for this profile edit flow |

**Relationships**:

- Belongs to one Customer Account Identity.
- May have at most one active pending email change.
- May be consumed by dashboard UI through a Web service DTO.
- Publishes a verified-email integration event for AuthenticationManagement when a pending email is verified/promoted.
- May publish a customer snapshot/update integration event for other bounded contexts if they maintain local customer snapshots.

**State transitions**:

1. `Created`: customer profile is initialized by customer creation with a verified email.
2. `Active/Updated`: first name, last name, gender, and date of birth can be updated by the owning authenticated customer.
3. `EmailChangePending`: customer requests a new email; current verified email remains profile/login email and pending email is awaiting verification.
4. `EmailVerified/Promoted`: CustomerDomain verification token validation succeeds; pending email becomes the verified CustomerDomain profile email and pending email/token reference are cleared. The login email remains unchanged until AuthenticationManagement consumes the verified-email integration event idempotently and updates the login identity.
5. `Forgotten`: GDPR deletion removes the CustomerDomain key and read models; encrypted event data becomes irretrievable; deletion event contains no PII; CustomerDomain publishes an outbox-backed `CustomerForgotten`/`CustomerDeleted` integration event. AuthenticationManagement consumes that event idempotently before the login identity is removed/disabled and the former login email can be registered by a new customer.

## Customer Account Identity

**Description**: The authenticated identity used to determine which customer profile can be read or changed.

**Fields**:

| Field | Type | Required | Notes |
|---|---|---:|---|
| `AuthenticatedUserId` | GUID or identity subject | Yes | Comes from current authentication session |
| `CustomerId` | GUID | Yes | Links session identity to CustomerDomain profile |
| `LoginEmail` | Email text | Yes | Uses the current verified email until pending verification succeeds |

**Rules**:

- A customer can read/update only their own profile.
- Unauthenticated visitors cannot load or update the dashboard profile.
- Requesting an email change must not allow duplicate verified or pending customer account email addresses; the uniqueness checker belongs to the foundational/US1 pending-email creation path, while US2 may only refine duplicate-email validation messages and tests.
- AuthenticationManagement updates its login credential only after CustomerDomain verifies/promotes the pending email, publishes the verified-email integration event through the outbox, and AuthenticationManagement consumes that event successfully; CustomerDomain verification alone must not mutate login identity state.
- After customer deletion, CustomerDomain must not directly delete, disable, or modify the login identity. It publishes a PII-free `CustomerForgotten`/`CustomerDeleted` integration event; after AuthenticationManagement processes it idempotently, login attempts for the deleted identity receive not found and the former login email no longer blocks new registration.

## Profile Update

**Description**: A requested change to editable non-email personal details.

**Fields**:

| Field | Type | Required | Validation |
|---|---|---:|---|
| `CustomerId` | GUID | Yes | Derived from the authenticated customer for dashboard `/me` flows; explicit ids require server-side policy |
| `FirstName` | Text | Yes | Non-empty after trimming |
| `LastName` | Text | Yes | Non-empty after trimming |
| `Gender` | Enum/string | No | Supported values only: `Male`, `Female`, `Prefer not to disclose` |
| `DateOfBirth` | Date nullable | No | Past date only |
| `RequestedAtUtc` | Timestamp | Yes | Supplied by application boundary if needed for audit |

**Lifecycle**:

1. Submitted from dashboard.
2. Server authenticates/authorizes the customer.
3. Request validation rejects invalid field values with field-specific errors and no state change.
4. Application checks aggregate existence.
5. Application encrypts event-sourced PII using the customer key.
6. Aggregate appends a profile update event.
7. Projection/read model and the successful command response provide the saved live profile state.
8. Gender and date of birth are persisted as encrypted profile event fields using the customer-specific key, projected as decrypted removable read-model fields, and removed from read models during GDPR deletion.
9. Web immediately refreshes every customer-facing surface that displays customer name, email, gender, or date of birth from the saved live profile state; updated values are visible without manual page reload in the same successful save response/render cycle, and no sync-pending state or stale profile display is acceptable.

## Email Change Request

**Description**: A requested replacement email address that must be verified before becoming authoritative.

**Fields**:

| Field | Type | Required | Validation |
|---|---|---:|---|
| `CustomerId` | GUID | Yes | Derived from authenticated dashboard customer for `/me` flows; explicit ids require server-side policy |
| `CurrentVerifiedEmail` | Email text | Yes | Existing profile/login email; remains active while pending |
| `RequestedEmail` | Email text | Yes | Valid format; unique across verified and pending customer emails except current profile |
| `RequestedAtUtc` | Timestamp | Yes | Supplied by application boundary |
| `VerificationTokenReference` | Text/reference | Yes | Token or hashed token reference; do not expose raw secrets in logs/events |
| `VerificationExpiresAtUtc` | Timestamp | Recommended | Used to prevent stale pending changes |

**Lifecycle**:

1. Customer submits a new email from the dashboard.
2. Application validates format, ownership, aggregate existence, and verified/pending email uniqueness before creating or replacing any pending email state.
3. Aggregate records pending encrypted email without changing verified email.
4. CustomerDomain creates/stores a safe verification token reference and hands off only the required delivery data to the external email delivery dependency: pending recipient email, customer id, token/reference, expiry, verification URL or route data, email-change purpose, correlation id, and retry/idempotency metadata.
5. Dashboard shows current verified email plus pending email status.
6. Customer completes verification through the verification contract.
7. On CustomerDomain verification success, pending email becomes the verified CustomerDomain profile email and a verified-email integration event is published for AuthenticationManagement; the login credential changes only after AuthenticationManagement consumes that event.
8. On expiry or replacement request, stale pending email is rejected or replaced according to application policy, with current verified email unchanged.

## Email Verification

**Description**: Proof that the customer controls the requested new email address.

**Fields**:

| Field | Type | Required | Validation |
|---|---|---:|---|
| `CustomerId` | GUID | Yes | Identifies the profile with a pending email |
| `VerificationToken` | Text | Yes | Must match pending verification and be unexpired |
| `VerifiedAtUtc` | Timestamp | Yes | Supplied by application boundary |

**Rules**:

- Verification succeeds only when a pending email exists for the customer and the token is valid.
- A successful CustomerDomain verification promotes the pending email to the CustomerDomain `VerifiedEmail`, clears `PendingEmail`, and emits a verified-email integration event; the login credential changes only after AuthenticationManagement consumes that event.
- Failed verification must not change profile or login identity.
- Verification confirmation and AuthenticationManagement login promotion are US3 responsibilities; US1 completes when pending email exists, delivery handoff is made, and old verified email remains active.
- Verification responses must be user-safe and must not expose token secrets.

## Email Verification Delivery Handoff

**Description**: Retry-safe payload/command CustomerDomain passes through a concrete sender/adapter registered in DI to the external email delivery dependency after creating or resending an email-change verification token. The delivery dependency renders/sends the message; it does not decide whether a token is valid or whether the email can be promoted.

**Fields**:

| Field | Type | Required | Validation / handling |
|---|---|---:|---|
| `CustomerId` | GUID | Yes | Identifies the profile with the pending email; not sufficient to authorize verification by itself |
| `PendingEmail` | Email text | Yes | Recipient for the verification message; avoid unnecessary logging |
| `Token` or `TokenReference` | Secret/reference text | Yes | Prefer reference when supported; raw token must never be logged or persisted in immutable events |
| `ExpiresAtUtc` | Timestamp | Yes | Matches CustomerDomain validation expiry |
| `VerificationUrl` or `VerificationRouteData` | URL/route data | Yes | Points to the verification-confirmation route with enough data for the customer to complete verification |
| `Purpose` | Text/enum | Yes | `CustomerEmailChange` |
| `CorrelationId` | GUID/text | Yes | For traceability across request, outbox, and delivery logs without PII |
| `IdempotencyKey` | Text | Yes | Stable for safe retries/resends; duplicate handoffs must not create multiple active verification states |

**Rules**:

- CustomerDomain remains authoritative for token creation, storage, expiry, validation, and pending-email promotion, and must provide the handoff contract, concrete sender/adapter, DI registration, and configuration needed to invoke the external delivery dependency.
- The handoff must not log raw tokens, customer keys, full profile payloads, or unnecessary PII.
- Failed delivery can be retried using the idempotency metadata without changing the verified profile/login email.
- Login credential promotion is not triggered by this handoff or by CustomerDomain verification alone; it happens only after US3 verification succeeds, the outbox publishes the verified-email event, and AuthenticationManagement consumes the event.

## Customer Profile Read Model

**Description**: Decrypted customer profile view used for dashboard reads and API responses.

**Fields**:

| Field | Type | Notes |
|---|---|---|
| `Id` | GUID | Customer id |
| `Email` | Email text | Current verified email; decrypted only when customer key exists |
| `PendingEmail` | Email text nullable | Pending unverified email; decrypted only when customer key exists and shown as pending |
| `EmailVerificationStatus` | Text/enum | `Verified` or `PendingVerification` |
| `FirstName` | Text | Decrypted only when customer key exists |
| `LastName` | Text | Decrypted only when customer key exists |
| `Gender` | Enum/string | Decrypted from encrypted profile events; allowed values: `Male`, `Female`, `Prefer not to disclose`; removed from the read model on GDPR deletion |
| `DateOfBirth` | Date nullable | Decrypted from encrypted profile events; removed from the read model on GDPR deletion |
| `Addresses` | Collection | Existing address behavior remains unchanged |

**Projection rules**:

- `CustomerCreated` creates initial read model if customer key exists.
- Profile update event applies changed personal details and the command response/read model must allow Web to refresh customer-facing profile displays immediately after a successful save without manual page reload in the same save response/render cycle.
- Email-change-requested event sets pending email/status without replacing verified email.
- Email-verified/promoted event replaces verified email, clears pending email, and updates status.
- `CustomerForgotten`/`CustomerDeleted` deletes the CustomerDomain read model. The deletion/forgetting application flow publishes a PII-free integration event; AuthenticationManagement, not the projection or CustomerDomain deletion handler, removes/disables active login identity and login-email uniqueness claims in its own idempotent consumer.
- If customer key is missing, projection rebuild must not restore plaintext profile data.

## Customer Forgotten/Deleted Integration Event

**Description**: PII-free asynchronous message published by CustomerDomain when a customer is forgotten/deleted so AuthenticationManagement can perform login-identity cleanup without CustomerDomain making login-identity business decisions directly.

**Fields**:

| Field | Type | Required | Notes |
|---|---|---:|---|
| `EventId` | GUID | Yes | Stable idempotency key for outbox publication and consumer retries |
| `OccurredAtUtc` | Timestamp | Yes | Time CustomerDomain recorded the deletion/forgetting outcome |
| `CustomerId` | GUID | Yes | Stable customer reference; not enough by itself to expose profile PII |
| `IdentitySubject` | Text/reference nullable | Recommended | Stable non-PII AuthenticationManagement identity reference when available |
| `Reason` | Text/enum | Yes | `CustomerForgotten` or repository-equivalent deletion reason |
| `CorrelationId` | GUID/text | Yes | Cross-service tracing without PII |

**Rules**:

- Published through CustomerDomain's transactional outbox after the PII-free deletion state is recorded.
- Must not include plaintext email, full name, gender, date of birth, raw verification tokens, customer encryption keys, or other PII.
- AuthenticationManagement consumes the message idempotently and owns all login identity removal/disablement and login-email uniqueness release decisions.
- Same-email re-registration and login not-found behavior are guaranteed only after AuthenticationManagement processes the event.
- Duplicate processing must be harmless if the login identity is already removed or disabled.

## Customer Key Record

**Description**: Customer-specific encryption key used for event-sourced PII.

**Fields**:

| Field | Type | Notes |
|---|---|---|
| `CustomerId` | GUID | Key owner |
| `Key` | Secret bytes/text | Stored in key store; never logged or exposed through API |

**Rules**:

- Created before encrypted personal data enters customer events.
- Used by application/projection boundaries that encrypt/decrypt PII.
- Deleted during GDPR erasure to crypto-shred event-sourced PII, including pending and verified emails.

## Web Profile DTO

**Description**: UI-facing model used by the dashboard form and typed service.

**Fields**:

| Field | Type | Notes |
|---|---|---|
| `Id` | GUID | Current customer id |
| `FirstName` | Text | Editable |
| `LastName` | Text | Editable |
| `Email` | Email text | Current verified email; displayed as the profile/login email |
| `PendingEmail` | Email text nullable | Display-only pending email requiring verification |
| `EmailVerificationStatus` | Text/enum | Drives pending verification message |
| `Gender` | Text/enum | Editable; maps only to `Male`, `Female`, or `Prefer not to disclose` |
| `DateOfBirth` | Date nullable | Editable/clearable |
| `FavoriteProducts` | Collection | Existing account behavior; not part of profile update contract |
| `Addresses` | Collection | Existing account behavior; not part of profile update contract |

**Rules**:

- Dashboard profile load/save must populate this DTO from the live CustomerDomain contract, not `StaticProfileList` or `FakeAccountService` data.
- Saving a changed email creates/updates pending email state; it does not replace `Email` until verification succeeds.
- After a successful save, all website surfaces showing customer name, email, gender, or date of birth must use the refreshed DTO/live profile state immediately, without manual page reload and within the same save response/render cycle.
- API failure must produce a user-safe error state, not fake successful data.
