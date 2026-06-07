# Review Aggregation and Management System

## Objective

Create a centralized review platform for Tiger van Cookie that:

1. Collects product reviews directly on the website.
2. Collects business reviews from Trustpilot.
3. Accepts and verifies Google Reviews submitted by customers.
4. Aggregates all review sources into a unified review feed.
5. Enables future customer reward programs based on review activity.
6. Preserves source attribution and review authenticity.
7. Ensures customers can only review products they have actually purchased.
8. Limits customers to one review per order for each purchased product.

---

# Review Sources

## 1. Internal Product Reviews

### Purpose

Collect reviews for individual products sold by Tiger van Cookie.

### Examples

* Chocolate Chip Cookie
* Red Velvet Cookie
* Matcha Cookie

### Requirements

Store internally:

* Review ID
* Customer ID
* Order ID
* Product ID
* Rating (1-5)
* Review Title
* Review Text
* Review Date
* Verified Purchase Flag
* Approval Status

### Verification

A review is considered verified when:

* Customer purchased the product being reviewed
* Customer is logged in OR
* Order number can be validated

### Review Eligibility Rules

To maintain review quality and authenticity:

* Customers may only review products that appear on a completed order associated with their account or validated order number.
* Customers may not review products they have never purchased.
* Purchasing one product does not grant permission to review other products.
* A customer is limited to one review per order per purchased product.
* Duplicate reviews for the same product on the same order must be prevented.
* Customers may submit reviews for the same product again only if it was purchased in a different order.

### Customer Review Visibility

For reviews managed directly by Tiger van Cookie:

* Customers should be able to view how many reviews they have submitted for each product.
* Product pages should indicate whether the customer has already reviewed that product for a specific order.
* Customers should be able to see which purchased products are still eligible for review.
* Review submission buttons should only be available for eligible purchased products.

### Rewards

Internal reviews can be automatically rewarded because customer identity is known.

Examples:

* Loyalty points
* Discount codes
* Free cookie voucher

---

## 2. Trustpilot Reviews

### Purpose

Collect independent third-party business reviews.

### Requirements

Use Trustpilot invitation system after purchase.

Workflow:

1. Customer places order.
2. Order marked as completed.
3. Trustpilot invitation sent automatically.
4. Customer submits review on Trustpilot.
5. Review appears on Trustpilot profile.
6. Review imported through Trustpilot API.

### Stored Data

* Trustpilot Review ID
* Rating
* Author Name
* Review Text
* Review Date
* Trustpilot URL
* Invitation Status
* Source = Trustpilot

### Benefits

* Independent review platform
* Trusted by consumers
* Better business credibility
* Can be displayed on website

### Rewards

Trustpilot invitations can be linked to customer orders.

Because the review invitation originated from a known customer:

* Reward eligibility can be tracked.
* Review completion can trigger rewards.

---

## 3. Google Reviews

### Purpose

Collect public business reviews through Google.

### Limitation

Google does not provide customer identity information.

Typically available:

* Reviewer name
* Reviewer profile image
* Review text
* Rating

Not available:

* Customer email
* Order number
* Customer account

---

# Google Review Verification Workflow

## Customer Flow

1. Customer leaves Google Review.

2. Customer visits Tiger van Cookie website.

3. Customer opens "Claim Review Reward".

4. Customer submits:

   * Order Number
   * Email Address
   * Google Review URL

5. System validates:

   * Order exists
   * Email matches order
   * Google review exists
   * Review not already claimed

6. Review marked as verified.

### Reward Eligibility

Once verified:

* Customer can receive discount
* Customer can receive loyalty points
* Customer can receive promotional rewards

---

# Unified Review Database

Create a single review table.

## Review Entity

Review

* Id
* Source
* SourceReviewId
* CustomerId (nullable)
* OrderId (nullable)
* ProductId (nullable)
* AuthorName
* Rating
* ReviewText
* ReviewDate
* VerifiedPurchase
* VerifiedReviewer
* RewardIssued
* SourceUrl
* Status

### Review Constraints

* One review per order per product.
* Prevent duplicate reviews for the same OrderId and ProductId combination.
* Product reviews must be linked to a valid purchased product.
* Reviews submitted for products not included in the order must be rejected.

## Source Values

* Internal
* Trustpilot
* Google

---

# Review Aggregation Layer

Create a Review Aggregation Service.

Responsibilities:

* Import Trustpilot reviews
* Import Google reviews
* Read internal reviews
* Normalize data format
* Produce unified review stream

---

# Unified Review View Model

ReviewFeedItem

* Source
* AuthorName
* Rating
* ReviewText
* ProductName
* VerifiedPurchase
* ReviewDate
* SourceUrl

---

# Frontend Display

## Homepage Review Feed

Display all review sources together.

Example:

★★★★★ Sarah
Google Review

"The cookies arrived fresh and delicious."

---

★★★★★ Mark
Trustpilot Review

"Fantastic customer service."

---

★★★★★ Lisa
Verified Purchase
Chocolate Chip Cookies

"Best cookie I've had in Amsterdam."

---

### Source Labels

Display clear source attribution:

* Google Review
* Trustpilot Review
* Verified Purchase

Never hide review origin.

---

# Product Page Reviews

Only display reviews relevant to the product.

Example:

Chocolate Chip Cookie

★★★★★ Lisa
Verified Purchase

★★★★★ David
Verified Purchase

★★★★★ Emma
Verified Purchase

Google and Trustpilot reviews should not appear as product reviews unless specifically linked to the product.

### Review Submission Controls

When a customer is logged in:

* Show whether the product has been purchased.
* Show how many reviews the customer has submitted for that product.
* Show which orders are eligible for review.
* Disable review submission if the customer has already reviewed the product for that order.
* Hide or disable review functionality for products the customer has never purchased.

---

# Review Moderation

Internal Reviews

Allow:

* Approve
* Reject
* Flag

Trustpilot Reviews

Read-only.

Google Reviews

Read-only.

Do not edit third-party reviews.

---

# Reward System

Create generic reward tracking.

Reward

* Id
* CustomerId
* ReviewId
* RewardType
* RewardValue
* IssuedDate

Reward Types:

* Discount Code
* Loyalty Points
* Free Cookie
* Free Shipping

Prevent duplicate rewards for the same review.

---

# Future Enhancements

* AI sentiment analysis
* Featured reviews
* Review summaries
* Product-specific review analytics
* Customer review leaderboard
* Automated review request campaigns
* Review fraud detection
* Review response management

---

# Recommended Architecture

Services

* Review Service
* Trustpilot Integration Service
* Google Review Integration Service
* Review Verification Service
* Reward Service
* Review Aggregation Service

Database

* Reviews
* Rewards
* ReviewClaims
* ReviewImports

Frontend

* Unified Review Feed
* Product Review Section
* Google Claim Form
* Trustpilot Status Dashboard
* Review Moderation Dashboard
* Customer Review History Dashboard

Goal: Maintain a single source of truth for all reviews while preserving review authenticity, source attribution, customer verification, purchase validation, one-review-per-order enforcement, and future reward capabilities.
