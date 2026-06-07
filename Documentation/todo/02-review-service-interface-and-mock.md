# Work Item 02 — IReviewService Interface and FakeReviewService

Depends on: `01-review-models`

## Goal
Create a dedicated `IReviewService` for all customer-facing review operations and a fake implementation backed by static data. Existing review methods on `IAccountService` (`SubmitReviewAsync`, `HasAlreadyReviewedProductIdAsync`) should eventually move here; leave them in place for now and add the new ones here.

## Interface
`src/CookieCommerce.Web/Services/IReviewService.cs`

```csharp
public interface IReviewService
{
    // Homepage unified feed (Internal + Trustpilot + Google)
    Task<IReadOnlyList<ReviewFeedItemDto>> GetReviewFeedAsync(int count = 10);

    // Product page — internal verified reviews only
    Task<IReadOnlyList<ReviewDto>> GetProductReviewsAsync(Guid productId);

    // Which orders allow the customer to review this product
    Task<IReadOnlyList<ReviewEligibilityDto>> GetReviewEligibilityForProductAsync(Guid productId);

    // All eligible products the customer hasn't yet reviewed (account area CTA)
    Task<IReadOnlyList<ReviewEligibilityDto>> GetPendingReviewEligibilityAsync();

    // All reviews the customer has submitted (account review history)
    Task<IReadOnlyList<ReviewDto>> GetCustomerReviewHistoryAsync();

    // Submit an internal product review for a specific order
    Task<ResultDto> SubmitProductReviewAsync(Guid productId, Guid orderId, ReviewSubmitRequest request);

    // Google review claim workflow
    Task<GoogleClaimReviewResult> ClaimGoogleReviewAsync(GoogleClaimReviewRequest request);
}
```

## Fake Implementation
`src/CookieCommerce.Web/Services/FakeReviewService.cs`

- `GetReviewFeedAsync` — return 6–8 hardcoded `ReviewFeedItemDto` mixing all three sources
- `GetProductReviewsAsync` — return 3 hardcoded internal verified reviews for any productId
- `GetReviewEligibilityForProductAsync` — return 1–2 orders, first already reviewed, second eligible
- `GetPendingReviewEligibilityAsync` — return 2 eligible order+product pairs
- `GetCustomerReviewHistoryAsync` — return 3 submitted reviews with dates and ratings
- `SubmitProductReviewAsync` — `await Task.Delay(800)` then return `ResultDto { Success = true }`
- `ClaimGoogleReviewAsync` — `await Task.Delay(1000)` then return `GoogleClaimReviewResult { Success = true }`

## DI Registration
Register `FakeReviewService` as `IReviewService` in `Program.cs` alongside the other fake services.
