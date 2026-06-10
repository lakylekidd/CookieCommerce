# Work Item 04 — Product Page Review Eligibility Controls

Depends on: `02-review-service-interface-and-mock`

## Goal
Replace the current all-or-nothing `HasAlreadyReviewedProductIdAsync` check in `AddReview.razor` with per-order eligibility so customers can review the same product from different orders, and so the UI clearly shows which orders are still eligible.

## Current behaviour (`AddReview.razor`)
- Checks a single boolean: "has customer reviewed this product at all?"
- No concept of order-specific eligibility
- No indication if the product was never purchased

## New behaviour

### Update `AddReview.razor`
Replace the `IAccountService.HasAlreadyReviewedProductIdAsync` call with `IReviewService.GetReviewEligibilityForProductAsync(ProductId)`.

**If list is empty (product never purchased):**
- Hide the review section entirely (no ghost, no message), OR show a subtle "Purchase this product to leave a review" note

**If list has entries:**
- Show a dropdown or card list of eligible orders: "Order #12345 — eligible" / "Order #12346 — already reviewed"
- The currently selected order determines whether the submit button is active
- Once an order is selected, submitting calls `IReviewService.SubmitProductReviewAsync(ProductId, SelectedOrderId, request)`

### Add product review list above `AddReview`
Call `IReviewService.GetProductReviewsAsync(ProductId)` to show existing verified internal reviews on the product page. Each review shows author, rating, date. Google and Trustpilot reviews must NOT appear here.
