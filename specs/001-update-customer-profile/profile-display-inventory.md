# Profile Display Inventory: Update Customer Profile

This inventory records website surfaces discovered during planning that display customer name, email, gender, or date of birth. Implementation tasks must update customer-facing surfaces to use live CustomerDomain profile data and immediate refreshed state after successful profile saves, visible without manual page reload in the same save response/render cycle. Administrative/backoffice surfaces must either consume the correct CustomerDomain/customer read model or be explicitly treated as out of customer-dashboard scope without regressing profile consistency.

## Customer-facing profile surfaces that must refresh immediately in the same save response/render cycle

| Surface | Displayed profile fields | Current source/notes | Required update |
|---|---|---|---|
| `src/CookieCommerce.Web/Components/Account/PersonalDetailsCard.razor` | First name, last name, email, date of birth | Uses `IAccountService.GetProfileAsync()` backed by `FakeAccountService`/`StaticProfileList` today | Use live concrete profile service/shared profile state; show verified email plus pending-email indicator; refresh from PATCH success response without manual page reload in the same render cycle. |
| `src/CookieCommerce.Web/Components/Account/EditProfileModal.razor` | First name, last name, email, gender, date of birth | Edits `UserProfileDto` through fake account service | Use live profile service; allowed gender labels/values are `Male`, `Female`, `Prefer not to disclose`; save email changes as pending; do not promote email in US1. |
| `src/CookieCommerce.Web/Components/Account/Overview.razor` | Hosts profile card | Depends on `PersonalDetailsCard` | Surface loading/failure state when live profile cannot load; no mock fallback. |
| `src/CookieCommerce.Web/Components/Auth/AuthUserButton.razor` | Current user's first/last name and initials | Uses auth state (`Auth.CurrentUser`) | Ensure it is refreshed from live verified profile state or an updated shared profile/auth state after profile save without requiring manual page reload. |
| `src/CookieCommerce.Web/Components/Layout/Navigation.razor` | Welcome first name | Uses auth state (`Auth.CurrentUser`) | Ensure first name updates immediately after successful profile save in the same render cycle. |
| `src/CookieCommerce.Web/Components/Storefront/Checkout/CheckoutCustomerInformation.razor` | Signed-in customer's first name and email | Receives `User` parameter and checkout email | Ensure signed-in customer display uses current verified profile email/name; pending email must not replace checkout/login email before verification. |
| `src/CookieCommerce.Web/Components/Account/Orders/Orders.razor` | Birthdate presence for birthday prompt | Uses `IAccountService.HasBirthdateAsync()` | Route through live profile service; update immediately after date of birth save/clear without manual page reload. |

## Related customer data surfaces to review for stale/mock profile details

| Surface | Displayed fields | Required review |
|---|---|---|
| `src/CookieCommerce.Web/Services/FakeAccountService.cs` | Profile, addresses, birthdate checks | Remove from customer-facing profile load/save/birthdate paths or prevent registration for those paths. |
| `src/CookieCommerce.Web/Services/StaticData/StaticProfileList.cs` | Static customer profile values | Must not be used for customer-facing profile load/save/display after implementation. |
| `src/CookieCommerce.Web/Services/FakeAuthService.cs`, `FakeAuthStateProvider.cs`, `StaticUserList.cs` | Name/email in auth state | Review any customer-facing registration in production configuration; if still present for demos/tests, it must not mask live profile failures. |
| `src/CookieCommerce.Web/Services/KeycloakAuthService.cs`, `KeycloakAuthStateProvider.cs` | Name/email claims/current user | Ensure successful profile save updates shared display state and verified-email promotion updates auth/login state only after US3 verification event consumption. |

## Backoffice/admin customer surfaces to keep consistent

| Surface | Displayed profile fields | Required update/review |
|---|---|---|
| `src/CookieCommerce.Web/Components/Backoffice/CustomersArea/CustomerTableItem.razor` | Full name, email, initials | Ensure it consumes CustomerDomain read model/projection that reflects profile updates, or document if backoffice refresh is outside customer-dashboard immediate-refresh scope. |
| `src/CookieCommerce.Web/Components/Backoffice/CustomersArea/CustomersTableCard.razor` | Search by full name/email | Ensure search source updates from live read model and does not depend on stale static customer data. |
| `src/CookieCommerce.Web/Components/Backoffice/CustomersArea/CustomerDetail.razor` | Full name, gender, date of birth, email, initials | Ensure read model includes profile changes and GDPR-deleted data is absent. |
| `src/CookieCommerce.Web/Models/Customers/CustomerDto.cs` | First name, last name, email, gender, date of birth | Align model with CustomerDomain contract/projection semantics. |
| `src/CookieCommerce.Web/Services/StaticData/MockCustomerData.cs` | Mock customer names/emails/gender/date of birth | Must not be a source for customer-facing updated profile state; review backoffice demo data separately. |

## Not profile-display surfaces for this feature

The planning search also found contact forms, sign-in/sign-up inputs, checkout shipping/billing address names, order address displays, footer labels, settings admin email fields, review profile image URLs, and mock order seed addresses. These are not displays of the authenticated customer's editable profile fields unless implementation discovers they are bound to `UserProfileDto`/current customer profile state; if they are, add them to the inventory before updating code.

## US1 Implementation Decisions (Phase 3 complete)

### Implemented surfaces

| Surface | Decision |
|---|---|
| `PersonalDetailsCard.razor` | Now uses `CustomerProfileService` + `CustomerProfileState`. Shows verified email, pending-email indicator. Displays loading skeleton and user-safe error on failure. Refreshes immediately via `CustomerProfileState.OnProfileChanged` without page reload. |
| `EditProfileModal.razor` | Now uses `CustomerProfileService`. Gender options: `Male`, `Female`, `PreferNotToDisclose`. Pending email indicator with resend button. Field-level validation errors displayed. Immediate state refresh via `CustomerProfileState.SetProfile` on success. |
| `Orders.razor` | `HasBirthdateAsync` now routes through `CustomerProfileService.HasBirthdateAsync()` instead of `FakeAccountService`. |
| `FakeAccountService.cs` | `GetProfileAsync` returns minimal auth-state DTO only (for favorites/address lookups); `UpdateProfileAsync` returns failure; `HasBirthdateAsync` returns false. Customer-facing profile paths removed. |
| `StaticProfileList.cs` | Annotated as backoffice/development-only; `DateOfBirth` fixed to `DateOnly`. Not used for customer-facing profile operations. |

### Remaining exclusions (not in US1 scope)

| Surface | Decision |
|---|---|
| `AuthUserButton.razor`, `Navigation.razor` | Auth state (name/initials) sourced from JWT claims. Live name refresh via `CustomerProfileState` would require these components to subscribe — deferred to polish phase to avoid widening US1 scope. |
| `CheckoutCustomerInformation.razor` | Uses parameter-passed `User`; not directly tied to profile service. Out of US1 scope. |
| Backoffice customer table | Uses `CustomerDto` from backoffice service; not customer-facing. Out of US1 scope. |
