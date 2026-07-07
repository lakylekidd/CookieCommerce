# Contract: Customer Profile API

This contract describes the CustomerDomain endpoints required by the dashboard update profile flow. Existing endpoints may be reused if they satisfy the same behavior; missing capabilities are in scope for this feature.

## Security and ownership

- All dashboard profile endpoints require an authenticated customer session unless explicitly noted for an out-of-band verification link.
- A customer can read and update only their own profile unless a separate server-side administrative policy is explicitly applied outside this customer dashboard flow.
- Dashboard-facing profile endpoints use `/api/customer/me` and derive the customer id from the authenticated session; caller-supplied customer ids must not be trusted for ordinary customer profile operations.
- Responses and errors must not include encrypted payloads, customer keys, raw verification tokens, or unnecessary PII.
- The verified email remains the profile and login email until a pending email is successfully verified.

## GET `/api/customer/me`

Returns the authenticated customer's own profile for display in the dashboard and any customer-facing website surface that shows profile details. Existing id-based endpoints may remain for internal/admin/deletion flows only if protected by ownership or server-side policy.

### Path parameters

None.

### Success response: `200 OK`

```json
{
  "id": "018f2f5c-6f65-7bc2-9bc3-534db0455c2a",
  "firstName": "Jamie",
  "lastName": "Baker",
  "email": "jamie@example.com",
  "pendingEmail": "jamie.new@example.com",
  "emailVerificationStatus": "PendingVerification",
  "gender": "Prefer not to disclose",
  "dateOfBirth": "1990-05-12",
  "addresses": []
}
```

If there is no pending email change, `pendingEmail` is `null` and `emailVerificationStatus` is `Verified`. Supported `gender` values are `Male`, `Female`, and `Prefer not to disclose`.

### Error responses

| Status | Meaning |
|---:|---|
| `401 Unauthorized` | Visitor is not signed in. |
| `403 Forbidden` | Authenticated identity cannot be resolved to a customer profile or policy forbids the request. |
| `404 Not Found` | Customer profile does not exist or has been forgotten. |

## PATCH `/api/customer/me/profile`

Updates editable dashboard profile details for the authenticated customer. Non-email fields are saved immediately when valid. If `email` differs from the current verified email, the endpoint first checks verified/pending email uniqueness excluding the current customer, then creates a pending email change, creates/stores a verification token reference, and hands off delivery to the external email dependency; the current verified email remains authoritative until CustomerDomain token verification succeeds. The login credential remains the old verified email until AuthenticationManagement consumes the verified-email integration event. The delivery handoff contract must include pending recipient email, customer id, token or token reference, expiry, verification URL or route data, email-change purpose, correlation id, and retry/idempotency metadata; raw tokens and unnecessary PII must not be logged. The PATCH/request/resend behavior belongs to US1; CustomerDomain pending-email promotion and AuthenticationManagement login credential consumption belong to US3.

### Path parameters

None.

### Request body

```json
{
  "firstName": "Jamie",
  "lastName": "Baker",
  "email": "jamie.new@example.com",
  "gender": "Prefer not to disclose",
  "dateOfBirth": "1990-05-12"
}
```

### Field rules

| Field | Required | Rules |
|---|---:|---|
| `firstName` | Yes | Non-empty after trimming. |
| `lastName` | Yes | Non-empty after trimming. |
| `email` | Yes | Valid format. If different from current verified email, uniqueness across verified and pending emails except the current customer must be checked before any pending email is created; only then does it become pending verification. |
| `gender` | No | Supported values: `Male`, `Female`, `Prefer not to disclose`. |
| `dateOfBirth` | No | Null or a real calendar date in the past. |

### Success response: `200 OK`

Returns the saved profile as read back from the live customer profile model. The `email` field remains the verified email when an email change is pending. This response must be sufficient for Web to refresh customer-facing surfaces showing profile details without manual page reload in the same save response/render cycle.

```json
{
  "id": "018f2f5c-6f65-7bc2-9bc3-534db0455c2a",
  "firstName": "Jamie",
  "lastName": "Baker",
  "email": "jamie@example.com",
  "pendingEmail": "jamie.new@example.com",
  "emailVerificationStatus": "PendingVerification",
  "gender": "Prefer not to disclose",
  "dateOfBirth": "1990-05-12",
  "addresses": []
}
```

### Error responses

| Status | Meaning |
|---:|---|
| `400 Bad Request` | Request body cannot be read or contains unsupported values. |
| `401 Unauthorized` | Visitor is not signed in. |
| `403 Forbidden` | Authenticated identity cannot be resolved to a customer profile or policy forbids the request. |
| `404 Not Found` | Customer profile does not exist or has been forgotten. |
| `409 Conflict` | Requested email address is already associated with another verified or pending customer account email. |
| `422 Unprocessable Entity` or validation problem | Field-level validation failed. |

### Validation problem shape

The API should use the repository's normal problem response conventions. Field-level errors must identify the field without echoing sensitive values.

```json
{
  "title": "One or more validation errors occurred.",
  "status": 422,
  "errors": {
    "email": ["Email address is already in use."],
    "dateOfBirth": ["Date of birth must be in the past."]
  }
}
```

## POST `/api/customer/me/email-verification`

Requests or resends verification for the authenticated customer's current pending email address. CustomerDomain creates/stores the verification token reference and delegates actual email delivery to the configured external dependency.

### Path parameters

None.

### Request body

```json
{
  "pendingEmail": "jamie.new@example.com"
}
```

The request body may be omitted if the customer already has exactly one pending email. If supplied, it must match the current pending email and must not be logged.

### Success response: `202 Accepted`

```json
{
  "status": "PendingVerification",
  "message": "Verification email sent. Continue signing in with your current email until verification is complete."
}
```

### Error responses

| Status | Meaning |
|---:|---|
| `400 Bad Request` | No pending email exists or request shape is invalid. |
| `401 Unauthorized` | Visitor is not signed in. |
| `403 Forbidden` | Authenticated identity cannot be resolved to a customer profile or policy forbids the request. |
| `404 Not Found` | Customer profile does not exist or has been forgotten. |
| `409 Conflict` | Pending email is no longer available. |
| `429 Too Many Requests` | Verification requests are throttled. |

### External delivery handoff

CustomerDomain owns token creation/storage/validation and hands delivery to the configured external email dependency through a concrete sender/adapter registered in DI. The handoff payload or command must provide enough data for that dependency to render and deliver the verification message without querying CustomerDomain synchronously for business decisions:

```json
{
  "customerId": "018f2f5c-6f65-7bc2-9bc3-534db0455c2a",
  "pendingEmail": "jamie.new@example.com",
  "tokenReference": "opaque-token-or-reference",
  "expiresAtUtc": "2026-07-06T13:34:56Z",
  "verificationUrl": "https://web.example/account/email-verification/confirm?tokenReference=opaque-token-or-reference",
  "verificationRoute": "/account/email-verification/confirm",
  "routeValues": {
    "tokenReference": "opaque-token-or-reference"
  },
  "purpose": "CustomerEmailChange",
  "correlationId": "018f2f5d-5f95-77fa-ae19-0fa8a4fbfef1",
  "idempotencyKey": "customer-email-change:018f2f5c-6f65-7bc2-9bc3-534db0455c2a:018f2f5d-5f95-77fa-ae19-0fa8a4fbfef1"
}
```

Requirements:

- The raw token value must never be written to logs or user-facing errors. If the delivery dependency only needs a reference, prefer a reference over the raw token.
- The payload must include either a complete `verificationUrl` or enough route data for the delivery dependency to build the verification link safely.
- The handoff must be safe to retry: duplicate delivery requests for the same idempotency key must not create multiple active verification states.
- The concrete CustomerDomain sender/adapter must be registered in DI with configuration/options for the external dependency and must avoid raw token or unnecessary PII logging.
- The external dependency handles rendering and delivery only; CustomerDomain remains authoritative for whether the token is valid, expired, or linked to the pending customer email.
- The customer continues signing in with the old verified email until US3 verification succeeds and the AuthenticationManagement promotion flow completes.

## POST `/api/customer/me/email-verification/confirm`

Confirms that the customer controls the pending email address. Token creation, storage, and validation are owned by CustomerDomain; actual email delivery is handled externally. This endpoint and the resulting verified-email integration/login-promotion flow are US3, not part of the US1 editable-profile MVP boundary.

### Path parameters

None.

### Request body

```json
{
  "token": "opaque-verification-token"
}
```

### Success response: `200 OK`

```json
{
  "id": "018f2f5c-6f65-7bc2-9bc3-534db0455c2a",
  "firstName": "Jamie",
  "lastName": "Baker",
  "email": "jamie.new@example.com",
  "pendingEmail": null,
  "emailVerificationStatus": "Verified",
  "gender": "Prefer not to disclose",
  "dateOfBirth": "1990-05-12",
  "addresses": []
}
```

### Required side effect

- CustomerDomain validates the verification token, promotes the pending email to verified profile email, clears the pending token reference, and publishes an outbox-backed integration event for AuthenticationManagement indicating that the verified profile email changed from the previous verified email to the new verified email.
- CustomerDomain does not synchronously update AuthenticationManagement login credentials; until the consumer processes the event, login continues to use the old verified email.
- AuthenticationManagement consumes the event idempotently and updates the login credential. Only after this consumer processing succeeds must the customer use the new email to sign in.

### Error responses

| Status | Meaning |
|---:|---|
| `400 Bad Request` | Request body cannot be read or no pending email exists. |
| `401 Unauthorized` | Authenticated confirmation is required and visitor is not signed in. |
| `403 Forbidden` | Authenticated identity or token/customer relationship does not authorize this confirmation. |
| `404 Not Found` | Customer profile does not exist or has been forgotten. |
| `409 Conflict` | Pending email no longer matches the verification request. |
| `410 Gone` | Verification token expired. |
| `422 Unprocessable Entity` | Verification token is invalid. |

## DELETE `/api/customer/{customerId}`

Existing GDPR deletion endpoint. This feature must not regress it and must preserve the clarified account-access behavior.

### Required behavior

- Requires authentication and ownership or an explicit server-side administrative policy.
- Deletes the customer-specific encryption key.
- Deletes read models for the customer.
- Emits a deletion event with no personal data.
- Ensures updated profile PII, including verified and pending email values, is permanently unreadable or removed according to `Documentation/GDPR-Compliance.md`.
- Publishes an outbox-backed `CustomerForgotten`/`CustomerDeleted` integration event with no personal data and enough stable non-PII identity/correlation data for AuthenticationManagement to identify the login identity safely.
- Does not synchronously delete, disable, update, or otherwise make business decisions about AuthenticationManagement login identities or login-email uniqueness.
- Requires AuthenticationManagement to consume the forgotten/deleted-customer event idempotently, remove or disable the login identity from active lookup, and release login-email uniqueness so subsequent login attempts fail as `404 Not Found`/not found and a new customer can register with the same email after processing.

### Post-deletion validation expectations

| Flow | Expected result |
|---|---|
| Login with deleted customer's former email before AuthenticationManagement processes the event | Existing login state may remain temporarily available/unavailable according to current AuthenticationManagement state; this is the eventual-consistency window and must be observable/retryable. |
| Login with deleted customer's former email after AuthenticationManagement processes the event | Not found response; no successful session is created. |
| Register a new customer using the same email before AuthenticationManagement processes the event | Registration is still blocked if the old login identity/email uniqueness claim remains active. |
| Register a new customer using the same email after AuthenticationManagement processes the event | Registration is allowed if the email is not used by another active customer. |
| Read deleted customer profile | `404 Not Found`. |

## Integration event: customer forgotten/deleted

This is a logical contract for CustomerDomain-to-AuthenticationManagement deletion coordination. The concrete message name should follow repository conventions; prefer `CustomerForgotten` for GDPR erasure language, with `CustomerDeleted` acceptable if existing message names use delete terminology.

```json
{
  "eventId": "018f2f5d-5f95-77fa-ae19-0fa8a4fbfef1",
  "occurredAtUtc": "2026-07-06T12:34:56Z",
  "customerId": "018f2f5c-6f65-7bc2-9bc3-534db0455c2a",
  "identitySubject": "auth-subject-or-stable-reference",
  "reason": "CustomerForgotten",
  "correlationId": "018f2f5d-5f95-77fa-ae19-0fa8a4fbfef1"
}
```

Requirements:

- Publish only after CustomerDomain has accepted the customer deletion/forgetting request and recorded the PII-free deletion state.
- Publish through the transactional outbox so CustomerDomain deletion state and outgoing message are coupled.
- Do not include plaintext email, full name, gender, date of birth, raw tokens, customer encryption keys, or other PII.
- AuthenticationManagement must consume the event idempotently and own all decisions about disabling/removing the login identity, removing it from active login lookup, and releasing login-email uniqueness.
- After successful consumer processing, login for the deleted identity returns not found and same-email registration is allowed if no other active account uses that email.
- Duplicate or retried messages must not recreate identities, leak PII, or fail if the identity is already removed/disabled.

## Integration event: verified customer email changed

This is a logical contract for cross-bounded-context coordination. The concrete message name may follow existing repository conventions.

```json
{
  "eventId": "018f2f5d-5f95-77fa-ae19-0fa8a4fbfef1",
  "occurredAtUtc": "2026-07-06T12:34:56Z",
  "customerId": "018f2f5c-6f65-7bc2-9bc3-534db0455c2a",
  "previousEmail": "jamie@example.com",
  "verifiedEmail": "jamie.new@example.com"
}
```

Requirements:

- Publish only after email verification succeeds.
- Publish through the transactional outbox so profile state and outgoing message are coupled.
- Consumer in AuthenticationManagement must be idempotent and safe to retry.
- CustomerDomain verification/promotion alone does not change the login credential; login continues with the old verified email until this event is consumed successfully.
- Message logging must avoid unnecessary PII; if full payload logging exists, redact email values. Operational log tests must prove profile update, email verification, and deletion/forgetting logs contain no first name, last name, email address, date of birth, gender, or raw verification tokens.

## Web service contract

The Blazor dashboard and any website surface that displays customer profile details must consume the API through a concrete typed service under `src/CookieCommerce.Web/Services`; do not add a new same-project Web service interface for this feature.

### Required operations

The concrete profile service must support these operations:

```csharp
Task<UserProfileDto?> GetProfileAsync();
Task<ResultDto> UpdateProfileAsync(UserProfileDto profile);
Task<ResultDto> ResendEmailVerificationAsync(string pendingEmail);
Task<bool> HasBirthdateAsync();
```

Implementation requirements:

- `GetProfileAsync` loads profile data from `GET /api/customer/me` for the current authenticated customer.
- `UpdateProfileAsync` sends profile updates to `PATCH /api/customer/me/profile`, maps validation/problem responses to `ResultDto` plus field feedback where the UI supports it, and refreshes shared/live profile state immediately from the success response so the updated values are visible without manual page reload in the same render cycle.
- `ResendEmailVerificationAsync` calls `POST /api/customer/me/email-verification`; CustomerDomain owns token creation/storage/validation and external infrastructure handles delivery via the handoff payload above.
- When the email changes, the UI must display the pending email state and tell the customer to continue signing in with the old verified email until CustomerDomain verification succeeds and AuthenticationManagement has consumed the verified-email event.
- No `StaticProfileList`, `FakeAccountService`, or mock-data fallback may be used for customer-facing profile load/save or any surface that displays customer name, email, gender, or date of birth.
- If existing interface signatures change, update `Documentation/backend-documentation.md`.
