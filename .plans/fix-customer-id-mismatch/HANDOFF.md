## Problem
Three UUIDs are independently generated for what should be a single user identity:

1. **Keycloak UUID** (`keycloakId`) – what Keycloak assigns; baked into the JWT `sub` claim.
2. **Auth-domain UUID** (`userId`) – `Guid.CreateVersion7()` in `RegisterUserHandler`; used as the Marten event stream ID and returned in `LoginResult.UserId`.
3. **Customer-domain UUID** (`customerId`) – another `Guid.CreateVersion7()` in `CreateCustomerHandler`, completely unlinked.

When `CustomerProfileService` calls `GET /api/customer/me` with `X-Customer-Id: user.Id`, the controller looks up `CustomerReadModel` by that ID — but the customer was stored under a different UUID, so it's never found → `MissingCustomerProfileException`.

There is a secondary inconsistency: at login `user.Id = LoginResult.UserId` (auth-domain UUID), but after a page refresh `user.Id = JWT sub = Keycloak UUID`. These are also different.

## Root cause locations
- `UserRegisteredConsumer.cs` – ignores `@event.UserId`; customer gets a fresh random ID.
- `CreateCustomerHandler.cs` – always calls `Guid.CreateVersion7()` regardless of input.
- `KeycloakAuthService.SignInAsync` – sets `user.Id = result.UserId` (auth-domain) while the refresh path uses JWT `sub` (Keycloak).

## Fix: use Keycloak UUID as the single shared identity
The JWT `sub` (Keycloak UUID) is always available from the token, making it the only truly stable key. All six tasks together make it the identity used everywhere:

| Location | Before | After |
|---|---|---|
| `UserRegisteredIntegrationEvent` | `UserId` only | + `KeycloakId` |
| `RegisterUserHandler` | publishes `UserId` only | also publishes `KeycloakId` |
| `CreateCustomer` command | no ID param | + optional `Guid? CustomerId = null` |
| `CreateCustomerHandler` | always `Guid.CreateVersion7()` | `request.CustomerId ?? Guid.CreateVersion7()` |
| `UserRegisteredConsumer` | no ID → fresh UUID | parses `KeycloakId` → passes as `CustomerId` |
| `KeycloakAuthService.SignInAsync` | `user.Id = result.UserId` | `user.Id` from `ParseJwtToUserDto(result.AccessToken).Id` (JWT sub) |

After the fix: customer stored under Keycloak UUID = `user.Id` everywhere = JWT `sub` → profile lookup succeeds.
