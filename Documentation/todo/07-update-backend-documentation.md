# Work Item 07 — Update Backend Documentation

Depends on: `02-review-service-interface-and-mock`

## Goal
Keep `Documentation/backend-documentation.md` in sync per the CLAUDE.md rule: whenever a service interface under `src/CookieCommerce.Web/Services/` is added or changed, update the doc in the same change.

## What to do

After `IReviewService` is created (Work Item 02):

1. Add a new **Review Management** domain section to `backend-documentation.md` with a method table for all `IReviewService` methods.
2. Update the **Domain Overview** table to include the new Review Management domain.
3. Update the *Last updated* footer line with the date and commit when the interface is added.

The seven methods to document:
- `GetReviewFeedAsync`
- `GetProductReviewsAsync`
- `GetReviewEligibilityForProductAsync`
- `GetPendingReviewEligibilityAsync`
- `GetCustomerReviewHistoryAsync`
- `SubmitProductReviewAsync`
- `ClaimGoogleReviewAsync`
