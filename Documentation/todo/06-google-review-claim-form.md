# Work Item 06 — Google Review Claim Form

Depends on: `02-review-service-interface-and-mock`

## Goal
Let customers who left a Google review claim their reward by submitting their order number, email, and the Google Review URL for verification.

## What to do

### Create `ClaimGoogleReview.razor`
`src/CookieCommerce.Web/Components/Storefront/Reviews/ClaimGoogleReview.razor`

Route: `/reviews/claim-google`

**Form fields:**
- Order Number (text input, required)
- Email Address (email input, required)
- Google Review URL (url input, required, placeholder: `https://maps.google.com/...`)
- Submit button

**Flow:**
1. Customer fills in the form and submits
2. Call `IReviewService.ClaimGoogleReviewAsync(request)`
3. Show loading state on the submit button while awaiting
4. On `Success = true`: show a success confirmation card (reward confirmation messaging, e.g. "Your roar was claimed! Expect your reward within 24 hours.")
5. On `Success = false`: show inline error using `result.ErrorMessage`

**Layout:**
- Use a centred single-column card layout (similar to auth pages)
- Include a short explanation of why the claim is needed ("Google doesn't share your identity with us, so we need to verify your order.")

### Add entry points
- Add a "Claim Google Review Reward" link to the storefront footer
- Optionally surface on the Order detail page (`/account/orders/{id}`) as a contextual CTA after the order is delivered
