# Work Item 05 — Customer Review History Page (Account Area)

Depends on: `02-review-service-interface-and-mock`

## Goal
Add a "My Reviews" page to the customer account area showing all submitted reviews and a call to action for pending eligible products.

## What to do

### Create `ReviewHistory.razor`
`src/CookieCommerce.Web/Components/Account/Reviews/ReviewHistory.razor`

Route: `/account/reviews`

**Pending reviews section (top)**
- Call `IReviewService.GetPendingReviewEligibilityAsync()`
- If any results: show a highlighted banner or card list: "You have [N] products waiting for your roar!"
- Each item: product name, order reference, link to product page

**Submitted reviews section**
- Call `IReviewService.GetCustomerReviewHistoryAsync()`
- Table or card list: product name, rating (stars), date submitted, review text excerpt, approval status badge (Pending / Approved / Rejected)

### Add navigation entry
Add "My Reviews" link to `AccountASide.razor` alongside "My Orders", "Addresses", etc.

### Guard
Page requires authentication — redirect to `/auth/signin` with returnUrl if not logged in (follow existing pattern in account pages).
