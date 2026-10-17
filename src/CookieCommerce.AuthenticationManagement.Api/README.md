# Authentication Management API

Authentication Management owns Keycloak identities, credentials, session tokens, and realm-role administration. Customer profiles remain owned by Customer Domain; administrator account endpoints resolve customer contact data through the configured `customer-domain-api` client.

## Customer account administration

All `/api/admin/accounts` endpoints require a bearer token with the `Admin` role.

| Method | Route | Response | Behavior |
|---|---|---|---|
| `GET` | `/api/admin/accounts/{customerId}` | `KeycloakAccountStatus` | Returns credential state, failed attempts, role state, and whether the target is the current actor. |
| `POST` | `/api/admin/accounts/{customerId}/provision` | `204` | Idempotently creates a linked, enabled Keycloak user with `User`, `UPDATE_PASSWORD`, and no password. |
| `POST` | `/api/admin/accounts/{customerId}/invite` | `202 PasswordSetupInvitationResponse` | Revokes older unused setup tokens and returns a new 30-minute token to the trusted Web email pipeline. |
| `PUT` | `/api/admin/accounts/{customerId}/role/admin` | `204` | Assigns `Admin`. |
| `DELETE` | `/api/admin/accounts/{customerId}/role/admin` | `204` or `409 ProblemDetails` | Removes `Admin`; rejects self-downgrade and removal of the final administrator. |

`PasswordSetupInvitationResponse` contains `token`, `email`, `firstName`, `lastName`, and `expiresAt`. Raw tokens are returned only once and are stored in Authentication Management as SHA-256 hashes.

## Public password setup

`POST /api/auth/password-setup` accepts:

```json
{
  "token": "single-use-token",
  "password": "new password with at least 8 characters"
}
```

On success, the token is consumed, the permanent Keycloak password is set, required setup actions are cleared, and the response uses the same login result contract as `POST /api/auth/login`. Invalid, expired, revoked, or used tokens return `400 ProblemDetails`. The endpoint uses the recovery rate-limit policy.
