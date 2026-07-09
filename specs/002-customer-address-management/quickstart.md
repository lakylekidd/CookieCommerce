# Quickstart: Validate Customer Address Management

This guide describes validation scenarios for moving customer address get/add/delete behavior from fake Web data to CustomerDomain.

## Prerequisites

- .NET 10 SDK installed.
- Docker available for Aspire-managed infrastructure.
- Repository dependencies restored.
- A test customer account that can sign in to the Web app.
- CustomerDomain API and Web app configured through Aspire service discovery.

## 1. Build and test

```powershell
dotnet restore src
dotnet build src
dotnet test src
```

Expected outcome:

- Build completes with zero errors.
- CustomerDomain aggregate, handler, projection, and API tests for address get/add/delete/default rules pass.
- Web service tests for address methods calling CustomerDomain and avoiding fake fallback pass.
- `dotnet test src` discovers the relevant tests.

## 2. Start the application composition

```powershell
dotnet run --project src/CookieCommerce.AppHost
```

Expected outcome:

- Aspire starts Web, CustomerDomain API, PostgreSQL, and required platform dependencies.
- CustomerDomain API and Web are healthy.
- No new runtime service is required for this feature.

## 3. Verify empty address book

1. Sign in as a customer with no saved addresses.
2. Open the account address book and checkout address selector.
3. Optionally call `GET /api/customer/me/addresses` as that customer.

Expected outcome:

- Address views show an empty state.
- CustomerDomain returns `200 OK` with an empty array from `GET /api/customer/me/addresses`.
- CustomerDomain returns `204 No Content` from `GET /api/customer/me/addresses/default` when the customer has no active default address.
- No static or fake address data appears.

## 4. Verify add address

1. Sign in as a customer.
2. Add a valid address with recipient, street, house number, city, postal code, province, country, phone, and label.
3. Refresh or revisit account and checkout address views.
4. Optionally call `GET /api/customer/me/addresses` and `GET /api/customer/me/addresses/default`.

Expected outcome:

- Add succeeds with a clear success message.
- `POST /api/customer/me/addresses` returns `201 Created` with the saved address response, including its stable `id`.
- The new address appears in profile and checkout address-selection flows within 2 seconds.
- If it is the first address, it is default and is returned by `GET /api/customer/me/addresses/default`.
- The address is stored and read through CustomerDomain, not `FakeAccountService`.

## 5. Verify validation

Try adding addresses with these invalid inputs one at a time:

- Blank label.
- Blank first name or last name.
- Blank street or house number.
- Blank city.
- Blank postal code.
- Blank province/country.
- Blank phone.
- Duplicate label for the same customer.

Expected outcome:

- Invalid submissions do not change the existing address book.
- Field-level validation feedback is shown.
- Duplicate label returns a conflict or equivalent user-safe validation outcome.
- Validation responses do not echo unnecessary personal data.

## 6. Verify delete address

1. Sign in as a customer with at least two saved addresses.
2. Delete a non-default address.
3. Verify the address list.
4. Delete the default address.
5. Verify the address list again.

Expected outcome:

- The deleted address no longer appears after each delete.
- Deleting a non-default address leaves the default unchanged.
- Deleting the default address promotes exactly one remaining address to default.
- Deleting the only address leaves an empty address book with no default.

## 7. Verify authorization and ownership

1. Sign in as Customer A and create an address.
2. Sign out and sign in as Customer B.
3. View Customer B's address book.
4. Attempt direct API calls that target Customer A's address id or legacy customer-id routes, if exposed.

Expected outcome:

- Customer B does not see Customer A's address.
- Customer B cannot delete Customer A's address.
- Customer-facing operations derive ownership from the authenticated customer.
- Legacy id-based routes, if retained, are protected or not used by customer-facing Web flows.

## 8. Verify no fake fallback

1. Temporarily make CustomerDomain unavailable or force an address API failure in a controlled development/test environment.
2. Open account and checkout address views.
3. Attempt add/delete.

Expected outcome:

- Web shows a clear user-safe failure or empty/error state as appropriate.
- Web does not show fake/static addresses as if they were live customer data.
- Fake address state is not mutated by customer-facing address actions.

## 9. Verify privacy and erasure

1. Add an address containing realistic recipient/contact/location data.
2. Trigger the existing customer erasure/forget flow.
3. Attempt to read the customer profile/address read model.
4. Inspect event/log behavior through development tooling without exposing secrets.

Expected outcome:

- Decrypted address read-model data is removed or unavailable after erasure.
- Address PII in immutable history is protected by customer-key crypto-shredding.
- Logs and problem responses do not contain full address payloads, customer keys, or encrypted event data.

## 10. Verify documentation

Review `Documentation/backend-documentation.md` after implementation.

Expected outcome:

- Customer-facing address methods are documented as CustomerDomain-backed.
- Any changed Web service behavior or contracts are reflected.
- Documentation no longer describes get/add/delete addresses as fake-account behavior.
