# Work Item 03 — Homepage Unified Review Feed

Depends on: `02-review-service-interface-and-mock`

## Goal
Add a review feed section to the storefront homepage that displays reviews from all three sources with clear source attribution.

## What to do

### Create `ReviewFeed.razor`
`src/CookieCommerce.Web/Components/Storefront/Reviews/ReviewFeed.razor`

- Inject `IReviewService` and call `GetReviewFeedAsync()`
- Render a horizontal or grid card layout (mobile: stack, desktop: row)
- Each card shows:
  - Star rating (1–5)
  - Author name + profile image (if available)
  - Review text (truncated to ~120 chars with ellipsis)
  - Product name (Internal reviews only)
  - Source badge — use distinct visual style per source:
    - **Google Review** — Google colours or icon
    - **Trustpilot Review** — Trustpilot green
    - **Verified Purchase** — primary brand colour + verified icon
  - Review date (relative: "2 weeks ago")
- Never hide or omit the source label

### Add to `Storefront.razor`
Insert `<ReviewFeed />` in an appropriate section of the homepage (e.g., below "Top Rated Treats" or between featured sections). Add a section heading such as "What our pride is saying".
