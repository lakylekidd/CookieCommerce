# Quickstart: Validate Update Customer Profile

This guide describes end-to-end validation for the customer dashboard profile update feature after implementation.

## Prerequisites

- .NET 10 SDK installed.
- Docker available for Aspire-managed infrastructure.
- Repository dependencies restored.
- Test user/customer account available or created through the existing sign-up/customer creation flow.
- External email verification delivery dependency configured for development, with a documented way to inspect or receive delivered verification links/tokens. CustomerDomain owns token creation, storage, and validation; this feature does not implement a new email delivery subsystem. The CustomerDomain delivery handoff must include a concrete sender/adapter registered in DI plus pending recipient email, customer id, token/reference, expiry, verification URL or route data, purpose, correlation id, and retry/idempotency metadata without logging raw tokens or unnecessary PII.

## 1. Build and test the solution

```powershell
dotnet restore src
dotnet test src --no-restore --list-tests
dotnet build src
dotnet test src
```

For the new runnable test projects, a quick discovery smoke test can be run directly when diagnosing setup issues:

```powershell
dotnet test src/CookieCommerce.Web.Tests/CookieCommerce.Web.Tests.csproj --no-restore --list-tests
dotnet test src/CookieCommerce.AuthenticationManagement.Tests/CookieCommerce.AuthenticationManagement.Tests.csproj --no-restore --list-tests
```

Expected outcome:

- Build completes with zero errors.
- CustomerDomain unit/integration/projection/API tests pass.
- `src/CookieCommerce.AuthenticationManagement.Tests` is a runnable xUnit project referenced by `src/CookieCommerce.slnx`, and AuthenticationManagement consumer/contract/integration tests for verified email changes and forgotten/deleted-customer events are discovered and pass.
- `src/CookieCommerce.Web.Tests` is a runnable xUnit project referenced by `src/CookieCommerce.slnx`, and Web service/profile validation/component tests are discovered and pass.
- No feature test files exist outside runnable test projects included by `dotnet test src`.

## 2. Start and validate the application composition

```powershell
dotnet run --project src/CookieCommerce.AppHost
```

Expected outcome:

- Aspire starts PostgreSQL, RabbitMQ, Keycloak, CustomerDomain API, AuthenticationManagement API, and Web.
- CustomerDomain API, AuthenticationManagement API, RabbitMQ, Keycloak, PostgreSQL, and Web are healthy in the Aspire dashboard.
- Web has a service-discovered CustomerDomain API client and does not require hard-coded localhost endpoints.
- Affected services remain registered in `src/CookieCommerce.AppHost/AppHost.cs` and participate in the composition after profile service discovery, verified-email integration, forgotten/deleted-customer integration, and email-handoff configuration changes.

## 3. Verify profile-display inventory coverage

Review `specs/001-update-customer-profile/profile-display-inventory.md` before implementation and again before completion.

Expected outcome:

- Every customer-facing website surface that displays the authenticated customer's name, verified email, gender, or date of birth is listed and updated to use live profile state or explicitly marked as not profile-bound.
- The current required customer-facing surfaces include `PersonalDetailsCard.razor`, `EditProfileModal.razor`, `Overview.razor`, `AuthUserButton.razor`, `Navigation.razor`, `CheckoutCustomerInformation.razor`, and `Orders.razor` birthdate prompt.
- Backoffice customer read surfaces (`CustomerTableItem.razor`, `CustomersTableCard.razor`, `CustomerDetail.razor`) are reviewed for consistency with CustomerDomain read models or explicitly documented as outside immediate customer-dashboard refresh scope.
- `FakeAccountService`, `StaticProfileList`, and related static/mock profile sources are not used for customer-facing profile display, load, save, or birthdate checks.

## 4. Verify profile loads from live data

1. Sign in as a customer.
2. Open the customer dashboard profile/personal details area.
3. Confirm first name, last name, verified email address, gender, and date of birth match the saved CustomerDomain profile data.
4. Stop or make CustomerDomain API unavailable and refresh the profile area.

Expected outcome:

- Profile data is displayed from the live customer profile service.
- When CustomerDomain API is unavailable, the dashboard shows a clear failure/loading state.
- The dashboard does not show `StaticProfileList`, `FakeAccountService`, or other mock profile data as if it were real.

## 5. Verify successful non-email profile update

1. Open the dashboard update profile modal/form.
2. Change first name, last name, gender, and date of birth to valid values while keeping the same email.
3. Save the form.
4. Leave and return to the dashboard profile area.
5. Optionally call `GET /api/customer/me` for the signed-in customer.

Expected outcome:

- Save completes with a clear success confirmation.
- Updated non-email values appear immediately after save on every website surface that displays customer name, email, gender, or date of birth, without manual page reload and within the same successful save response/render cycle, with no sync-pending or stale profile state.
- The authenticated profile GET endpoint `GET /api/customer/me` returns the updated values.
- No mock profile data is modified or used for the customer-facing result.

## 6. Verify pending email change behavior (US1 boundary)

1. Sign in with the customer's current verified email.
2. Open the dashboard update profile modal/form.
3. Change the email to a valid unique new address and save.
4. Leave and return to the dashboard profile area.
5. Sign out and sign in again before verification.

Expected outcome:

- The dashboard shows the current verified email as the profile/login email.
- The dashboard shows the requested new email as pending verification.
- The customer can still sign in with the old verified email.
- The customer cannot sign in with the new email before verification succeeds.
- The profile GET endpoint `GET /api/customer/me` returns `email` as the old verified email and `pendingEmail` as the requested new email.
- CustomerDomain has created/stored a verification token reference and handed off delivery to the external email dependency without logging the raw token.
- The delivery handoff can be validated through the registered `IEmailVerificationSender`/external adapter path: payload includes pending recipient, customer id, token or safe reference, expiry, verification route/URL, purpose `CustomerEmailChange`, correlation id, and idempotency key.
- Re-sending verification uses the current pending email and retry-safe handoff metadata; it must not create multiple active pending email states or expose the token in user-facing messages/logs.
- US1 validation stops here: no login credential promotion or verified email replacement is expected until the US3 verification-confirmation flow succeeds.

## 7. Verify email verification and login promotion (US3 boundary)

1. Complete the verification flow for the pending new email using the link/token delivered by the external email dependency.
2. Refresh the dashboard profile area.
3. Before the AuthenticationManagement consumer processes the verified-email event, attempt sign-in behavior if the test harness can pause or inspect the consumer.
4. Allow/trigger AuthenticationManagement to consume the verified-email integration event.
5. Sign out.
6. Attempt to sign in with the old email.
7. Attempt to sign in with the new verified email.

Expected outcome:

- CustomerDomain validates the token, verification succeeds, and pending email/token reference are cleared.
- The dashboard now shows the new email as the verified profile email.
- Until AuthenticationManagement consumes the verified-email event, login still uses the old verified email and the new email is not accepted for login.
- AuthenticationManagement receives/processes the verified-email integration event idempotently.
- After consumer processing, the old email no longer works as the login credential.
- After consumer processing, the new verified email is required for login.

## 8. Verify validation behavior

Attempt these updates one at a time:

- Blank first name.
- Blank last name.
- Invalid email format.
- Email address already used as another customer's verified email.
- Email address already pending for another customer.
- Future date of birth.
- Unsupported gender value submitted directly to the API.
- Invalid, expired, or mismatched email verification token.

Expected outcome:

- Invalid profile updates do not change saved profile fields.
- Invalid or duplicate email changes do not create or promote a pending email because verified/pending email uniqueness is checked before pending email creation in the Foundation/US1 path; US2 only refines validation messages/tests.
- UI/API returns field-specific, user-safe validation feedback.
- Duplicate email returns a conflict or validation result without exposing another customer's details.
- Failed verification leaves login on the current verified email.

## 9. Verify authorization

1. Sign out and attempt to open the update profile page/endpoint.
2. Sign in as Customer A and verify dashboard profile calls use `/api/customer/me`; if any id-based customer endpoint remains callable, attempt to read/update Customer B's profile by changing the id in a direct API call.
3. Attempt to verify Customer B's pending email using Customer A's session or a mismatched token.

Expected outcome:

- Unauthenticated access is rejected or redirected to sign-in.
- Dashboard `/me` profile calls return only Customer A's profile, and any id-based cross-customer read/update attempts are forbidden or otherwise rejected.
- Mismatched verification attempts are rejected.
- Customer B's profile and login email remain unchanged.

## 10. Verify GDPR crypto-shredding and deletion access behavior

1. Update a customer's full name.
2. Request an email change and leave it pending, then verify it.
3. Trigger the existing GDPR deletion flow for that customer.
4. Attempt to rebuild/read the customer's profile projection.
5. Inspect deletion event payload shape, not plaintext values.
6. Confirm CustomerDomain published a PII-free `CustomerForgotten`/`CustomerDeleted` integration event through the outbox and did not call or directly modify AuthenticationManagement login identity state.
7. Allow/trigger AuthenticationManagement to consume the event idempotently.
8. Attempt to sign in with the deleted customer's former verified email.
9. Register a new customer using that same email address.

Expected outcome:

- The customer-specific key is removed.
- Read models for that customer are deleted.
- Updated full name and verified/pending email values in immutable history are permanently unreadable without the key.
- Gender and date of birth are stored in encrypted immutable profile events with the customer-specific key and are permanently unreadable after key deletion; any decrypted read-model copies are removed.
- CustomerDomain deletion records and the `CustomerForgotten`/`CustomerDeleted` integration event contain no personal data.
- Operational logs produced during profile update, email verification, and deletion/forgetting contain no first name, last name, email address, date of birth, gender, or raw verification tokens.
- CustomerDomain does not synchronously delete, disable, or otherwise make business decisions about AuthenticationManagement login identities.
- AuthenticationManagement consumes the event idempotently and removes/disables the login identity from active lookup.
- After AuthenticationManagement processing, login for the deleted customer fails with a not found response.
- After AuthenticationManagement processing, registration with the deleted customer's former email succeeds for a new customer, provided no other active customer uses it. Before processing, email reuse may remain blocked by the existing AuthenticationManagement state.

## 11. Verify documentation and service contracts

Review:

- `Documentation/backend-documentation.md`
- `src/CookieCommerce.slnx`
- `src/CookieCommerce.Web.Tests/CookieCommerce.Web.Tests.csproj`
- `src/CookieCommerce.AuthenticationManagement.Tests/CookieCommerce.AuthenticationManagement.Tests.csproj`
- `src/CookieCommerce.Web/Services/CustomerProfileService.cs` and `src/CookieCommerce.Web/Services/CustomerProfileState.cs`
- `specs/001-update-customer-profile/contracts/customer-profile-api.md`
- `specs/001-update-customer-profile/profile-display-inventory.md`

Expected outcome:

- Backend documentation reflects changed profile integration behavior or any changed existing Web service contract.
- Contracts and implementation agree on profile fields, pending email behavior, verification, validation, async forgotten/deleted-customer integration, AuthenticationManagement consumer ownership, external delivery handoff contract/sender/adapter/DI registration/payload, Aspire participation, and failure behavior.
- Web and AuthenticationManagement test projects are solution-referenced and discoverable by `dotnet test src`, with discovery validated only after restore when using `--no-restore`, and with no orphan tests outside those runnable projects.
