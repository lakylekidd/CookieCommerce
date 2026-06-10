# Work Item 01 — Review Models

## Goal
Extend and add the data models needed to support the full review system across all three sources (Internal, Trustpilot, Google).

## What to do

### Add `ReviewSource` enum
`src/CookieCommerce.Web/Models/Reviews/ReviewSource.cs`
```csharp
public enum ReviewSource { Internal, Trustpilot, Google }
```

### Extend `ReviewDto`
Add missing fields to `src/CookieCommerce.Web/Models/Reviews/ReviewDto.cs`:
- `ReviewSource Source`
- `string? ProductName`
- `DateTime ReviewDate`
- `bool VerifiedPurchase`
- `string? SourceUrl`

### Add `ReviewFeedItemDto`
`src/CookieCommerce.Web/Models/Reviews/ReviewFeedItemDto.cs` — unified view model for the homepage feed:
- `ReviewSource Source`
- `string AuthorName`
- `string? ProfileImageUrl`
- `int Rating`
- `string ReviewText`
- `string? ProductName`
- `bool VerifiedPurchase`
- `DateTime ReviewDate`
- `string? SourceUrl`

### Add `ReviewEligibilityDto`
`src/CookieCommerce.Web/Models/Reviews/ReviewEligibilityDto.cs` — describes a customer's review eligibility for a specific order + product combination:
- `Guid OrderId`
- `string OrderReference`
- `Guid ProductId`
- `string ProductName`
- `bool AlreadyReviewed`
- `int? ExistingRating`

### Add `GoogleClaimReviewRequest`
`src/CookieCommerce.Web/Models/Reviews/GoogleClaimReviewRequest.cs`:
- `string OrderNumber`
- `string Email`
- `string GoogleReviewUrl`

### Add `GoogleClaimReviewResult`
`src/CookieCommerce.Web/Models/Reviews/GoogleClaimReviewResult.cs`:
- `bool Success`
- `string? ErrorMessage`
