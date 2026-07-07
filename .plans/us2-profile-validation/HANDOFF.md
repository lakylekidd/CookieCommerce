## Phase 4 – User Story 2: Prevent invalid profile updates

### Goal
Invalid names, invalid/duplicate emails, unsupported gender values, and future dates of birth are rejected with clear field-specific feedback and no saved profile changes.

### What exists (US1 baseline)
- `CustomerProfileValidationException` – single-message, no field info
- `DuplicateCustomerEmailException` – generic 409 conflict
- `CustomerEmailUniquenessService.EnsureEmailAvailableForPendingChangeAsync` – throws generic duplicate exception
- `DomainExceptionMiddleware` – maps exceptions to generic `ProblemDetails`
- `CustomerProfileService.UpdateProfileAsync` – reads `errors` dict from 422; 409 returns no field error
- `EditProfileModal.razor` – already renders `fieldErrors[field]` per field

### What US2 adds
1. **`CustomerProfileValidator`** (`Application/Customers/Profile/`) – validates required names (≤100 chars), email format, gender is defined enum, DOB in past.
2. **Enrich `CustomerProfileValidationException`** to carry `Dictionary<string,string[]> FieldErrors`.
3. **Refine `CustomerEmailUniquenessService`** – distinct messages for VerifiedEmail vs PendingEmail conflict, throws with `FieldErrors["email"]`.
4. **Wire validator into `UpdateCustomerProfileCommandHandler`** – validate before any events; throw on failure.
5. **Update `DomainExceptionMiddleware`** – `CustomerProfileValidationException` with field errors → `ValidationProblemDetails` (422). `DuplicateCustomerEmailException` → 409 with `errors.email`.
6. **Update `CustomerProfileService`** – 409 response sets `FieldErrors["email"]`.

### Files changed
- `CustomerDomain.Domain/Exceptions/CustomerProfileExceptions.cs`
- `CustomerDomain.Application/Customers/Profile/CustomerProfileValidator.cs` (new)
- `CustomerDomain.Application/Customers/Profile/CustomerEmailUniquenessService.cs`
- `CustomerDomain.Application/Customers/Handlers/UpdateCustomerProfileCommandHandler.cs`
- `CustomerDomain.Api/Middleware/DomainExceptionMiddleware.cs`
- `CookieCommerce.Web/Services/CustomerProfileService.cs`
- `CustomerDomain.Tests/DomainTests/CustomerAggregate_ProfileValidation_Tests.cs` (new)
- `CustomerDomain.Tests/ApplicationTests/UpdateCustomerProfileValidationTests.cs` (new)
- `CustomerDomain.Tests/IntegrationTests/CustomerProfileValidationApiTests.cs` (new)
- `CookieCommerce.Web.Tests/Services/CustomerProfileValidationTests.cs` (new)
