# Backend Documentation

This document catalogs all service interface contracts in `CookieCommerce.Web/Services/` and organises them by backend domain. Update this file whenever an interface is added or changed.

Some interfaces (`IBackofficeService`, `IAccountService`, `ICheckoutService`) are composite — they surface methods that belong to multiple domains. Each method is listed under the domain it logically belongs to.

---

## Domain Overview

| Domain | Primary Interface(s) |
|---|---|
| [Authentication Management](#1-authentication-management) | `IAuthService` |
| [Order Management](#2-order) | `ICheckoutService`, `IBackofficeService` *(partial)*, `IAccountService` *(partial)* |
| [Inventory Management](#3-inventory) | `ICookieService`, `IInventoryService`, `IBackofficeService` *(partial)* |
| [Cart Management](#4-cart) | `ICartService` |
| [Logistics Management](#5-logistics) | `ICheckoutService` *(partial)*, `IAccountService` *(partial)* |
| [Customer Management](#6-customer-management) | `CustomerProfileService`, `IAccountService` *(partial)*, `ICustomerService` |
| [Store Management](#7-store-management) | `IImageUploadService`, `INewsletterService`, `IContactService`, `ILocationService`, `IOperationsService`, `IContentService` |

---

## 1. Authentication Management

**Interface:** `IAuthService`
**File:** `Services/IAuthService.cs`

**Responsibility:** Handles all user authentication concerns — sign-in, sign-up, password management, session state, and role checks. Acts as the single source of truth for who the current user is and whether they have admin privileges. Also fires a change event so UI components can react to auth state transitions.

**Requirements:**
- Must support email/password sign-in and new-account registration.
- Must support a forgot-password flow via a tokenised email link.
- Must expose synchronous properties (`IsLoggedIn`, `IsAdmin`, `CurrentUser`) for use in UI guards.
- Must fire `OnChange` whenever auth state changes so components can re-render.
- `InitializeAsync` must be called on app startup to rehydrate session from storage.

| Method / Member | Signature |
|---|---|
| `IsLoggedIn` | `bool IsLoggedIn { get; }` |
| `IsAdmin` | `bool IsAdmin { get; }` |
| `CurrentUser` | `UserDto? CurrentUser { get; }` |
| `SignInAsync` | `Task<bool> SignInAsync(string email, string password)` |
| `SignUpAsync` | `Task<AuthResult> SignUpAsync(SignupDto signupForm)` |
| `SendForgotPasswordLinkAsync` | `Task SendForgotPasswordLinkAsync(string email)` |
| `UpdatePasswordAsync` | `Task<AuthResult> UpdatePasswordAsync(string token, string newPassword)` |
| `InitializeAsync` | `Task InitializeAsync()` |
| `Logout` | `void Logout()` |
| `OnChange` | `event Action OnChange` |

---

## 2. Order

**Interfaces:** `ICheckoutService`, `IBackofficeService` *(order methods)*, `IAccountService` *(order history methods)*
**Files:** `Services/ICheckoutService.cs`, `Services/IBackofficeService.cs`, `Services/IAccountService.cs`

**Responsibility:** Covers the full order lifecycle — from placement through to fulfilment status changes. Checkout converts the active cart into an order, the backoffice methods advance its status, and the account methods expose order history to the customer. The dashboard totals in `IBackofficeService` also live here as they are order-volume metrics.

**Requirements:**
- Guest checkout must not require an existing account; a contact email is sufficient.
- New customer checkout must atomically create the account and place the order.
- All checkout paths must return a `CheckoutResult` indicating success or failure with a reason.
- Checkout must consume the active cart (from `ICartService`) as part of order creation.
- Order status transitions must follow a defined lifecycle (see `Order-Lifecycle.md`).
- `GetQuotaDateOrdersAsync` returns all orders assigned to a given production quota date for oven scheduling.
- Dashboard totals must support period filtering (e.g. `"this week"`, `"this month"`).
- Order history must support pagination (`page` / `pageSize`).

### From `ICheckoutService`

| Method | Signature |
|---|---|
| `CheckoutGuestAsync` | `Task<CheckoutResult> CheckoutGuestAsync(GuestCheckoutRequest request)` |
| `CheckoutNewCustomerAsync` | `Task<CheckoutResult> CheckoutNewCustomerAsync(NewCustomerCheckoutRequest request)` |
| `CheckoutExistingCustomerAsync` | `Task<CheckoutResult> CheckoutExistingCustomerAsync(ExistingCustomerCheckoutRequest request)` |

### From `IBackofficeService`

| Method | Signature |
|---|---|
| `SetOrderAsProcessing` | `Task SetOrderAsProcessing(Guid orderId)` |
| `SetOrderAsReadyForPickup` | `Task SetOrderAsReadyForPickup(Guid orderId)` |
| `GetQuotaDateOrdersAsync` | `Task<QuotaDateOrdersDto> GetQuotaDateOrdersAsync(DateTime quotaDate)` |
| `GetTotalsAsync` | `Task<DashboardDto> GetTotalsAsync(string period = "this week")` |

### From `IAccountService`

| Method | Signature |
|---|---|
| `GetOrdersAsync` | `Task<ICollection<OrderListItemDto>> GetOrdersAsync(int page, int pageSize)` |
| `GetTotalOrdersCountAsync` | `Task<int> GetTotalOrdersCountAsync()` |
| `GetOrderByIdAsync` | `Task<OrderDto?> GetOrderByIdAsync(Guid orderId)` |
| `GetOrderCountInNeedOfAttentionAsync` | `Task<int> GetOrderCountInNeedOfAttentionAsync()` |

---

## 3. Inventory

**Interfaces:** `ICookieService`, `IInventoryService`, `IBackofficeService` *(product & variant methods)*
**Files:** `Services/Products/ICookieService.cs`, `Services/IInventoryService.cs`, `Services/IBackofficeService.cs`

**Responsibility:** Covers all product and stock concerns — storefront browsing, production-area inventory tracking, and backoffice product/variant/SKU management. `ICookieService` is the read-only storefront view; `IInventoryService` tracks daily batch stock; `IBackofficeService` exposes the admin mutations for products and variants.

**Requirements:**
- All `ICookieService` list endpoints must return data suitable for direct rendering — no further joins required by the UI.
- `CancellationToken` parameters must be honoured to support Blazor component disposal.
- `GetProductByIdAsync` must return `null` rather than throw when a product is not found.
- `GetInventoryAsync` reflects current (today's) inventory state; `GetInventoryForDateAsync` returns the projected state for a given date.
- `GetAvailableProductsAsync` returns only products that are active and have SKUs.
- Product and variant mutations (status, name, price, deletion) must be atomic.
- SKU management methods must keep inventory figures consistent with `IInventoryService` data.
- All backoffice product endpoints must be guarded by admin role on the server.

### From `ICookieService` — Storefront catalog

| Method | Signature |
|---|---|
| `GetTopRatedAsync` | `Task<IReadOnlyList<StorefrontProductDto>> GetTopRatedAsync()` |
| `GetFeaturedCookiesAsync` | `Task<IReadOnlyList<StorefrontProductDto>> GetFeaturedCookiesAsync(CancellationToken cancellationToken = default)` |
| `GetFavoriteCookiesAsync` | `Task<IReadOnlyList<StorefrontProductDto>> GetFavoriteCookiesAsync(CancellationToken cancellationToken = default)` |
| `GetProductByIdAsync` | `Task<StorefrontProductDto?> GetProductByIdAsync(Guid id, CancellationToken cancellationToken = default)` |
| `GetReviewsByCookieIdAsync` | `Task<IReadOnlyList<ReviewDto>> GetReviewsByCookieIdAsync(Guid cookieId, CancellationToken cancellationToken = default)` |

### From `IInventoryService` — Production stock

| Method | Signature |
|---|---|
| `GetInventoryAsync` | `Task<InventoryAreaDto> GetInventoryAsync()` |
| `GetInventoryForDateAsync` | `Task<InventoryAreaDto> GetInventoryForDateAsync(DateOnly date)` |
| `GetAvailableProductsAsync` | `Task<IReadOnlyList<BatchProductDto>> GetAvailableProductsAsync()` |

### From `IBackofficeService` — Product & variant management

| Method | Signature |
|---|---|
| `CreateProductAsync` | `Task CreateProductAsync(CreateProductDto createProductDto)` |
| `GetSkusAsync` | `Task<ICollection<SkuDto>> GetSkusAsync()` |
| `GetVariantAsync` | `Task<ProductVariantDto> GetVariantAsync(Guid variantId)` |
| `GetVariantSkus` | `Task<ICollection<ProductVariantSkuDto>> GetVariantSkus(Guid variantId)` |
| `GetInventoryItemsAsync` | `Task<ICollection<ProductDto>> GetInventoryItemsAsync()` |
| `GetInventoryItemAsync` | `Task<ProductDto> GetInventoryItemAsync(Guid productId)` |
| `ChangeProductStatusAsync` | `Task ChangeProductStatusAsync(Guid productId, bool isActive)` |
| `ChangeVariantStatusAsync` | `Task ChangeVariantStatusAsync(Guid variantId, bool isActive)` |
| `ChangeVariantNameAsync` | `Task ChangeVariantNameAsync(Guid variantId, string name)` |
| `ChangeVariantPriceAsync` | `Task ChangeVariantPriceAsync(Guid variantId, decimal price)` |
| `DeleteVariantAsync` | `Task DeleteVariantAsync(Guid variantId)` |
| `AddVariantSkuAsync` | `Task AddVariantSkuAsync(Guid productId, Guid skuId, int quantity)` |
| `RemoveVariantSku` | `Task RemoveVariantSku(Guid variantId, Guid skuId)` |
| `UpdateVariantSkuQuantity` | `Task UpdateVariantSkuQuantity(Guid variantId, Guid skuId, int quantity)` |

---

## 4. Cart

**Interface:** `ICartService`
**File:** `Services/ICartService.cs`

**Responsibility:** Manages the shopping cart session for both anonymous guests and authenticated users. Handles item additions (single and batch), quantity updates, note attachment, and promotional code application. Fires a change event so the cart icon and bag component stay in sync across the page.

**Requirements:**
- Cart must work for unauthenticated users (guest cart stored in session/local storage).
- On login, the guest cart must be merged into the user's cart via `ConvertToUserCartAsync`.
- `OnCartUpdated` must fire after every mutation so the header badge updates without a page reload.
- Promotion codes must validate server-side; the result `ResultDto` carries success/error messaging.
- Batch add methods accept tuples to minimise round-trips when restoring a saved cart.

| Method / Member | Signature |
|---|---|
| `OnCartUpdated` | `event Action? OnCartUpdated` |
| `ConvertToUserCartAsync` | `Task ConvertToUserCartAsync(Guid userId)` |
| `GetCartItemCountAsync` | `Task<int> GetCartItemCountAsync()` |
| `GetCartAsync` | `Task<CartDto> GetCartAsync()` |
| `UpdateQuantityAsync` | `Task UpdateQuantityAsync(Guid itemId, int quantity)` |
| `RemoveItemAsync` | `Task RemoveItemAsync(Guid itemId)` |
| `AddToCartAsync` (no variant) | `Task AddToCartAsync(Guid productId, int quantity)` |
| `AddToCartAsync` (with variant) | `Task AddToCartAsync(Guid productId, Guid productVariantId, int quantity)` |
| `AddMultipleToCartAsync` (no variant) | `Task AddMultipleToCartAsync(IEnumerable<(Guid productId, int quantity)> items)` |
| `AddMultipleToCartAsync` (with variant) | `Task AddMultipleToCartAsync(IEnumerable<(Guid productId, Guid productVariantId, int quantity)> items)` |
| `AddNoteToCartAsync` | `Task AddNoteToCartAsync(string note)` |
| `ApplyPromotionCodeAsync` | `Task<ResultDto> ApplyPromotionCodeAsync(string code)` |
| `RemovePromotionCodeAsync` | `Task<ResultDto> RemovePromotionCodeAsync(string code)` |

---

## 5. Logistics

**Interfaces:** `ICheckoutService` *(delivery method selection)*, `IAccountService` *(tracking)*
**Files:** `Services/ICheckoutService.cs`, `Services/IAccountService.cs`

**Responsibility:** Covers the physical fulfilment side of an order — calculating which delivery options are available for a given address, and providing shipment tracking once an order is dispatched. These methods are currently surfaced through composite interfaces; a dedicated logistics interface may be introduced as this domain grows (e.g. carrier integrations, route planning).

**Requirements:**
- Delivery methods must be computed based on the customer's delivery address (e.g. postcode zone, weight).
- `GetTrackingAsync` must return `null` gracefully when no tracking information is available yet.
- Delivery method data must include enough detail (price, estimated delivery window) for the customer to make a selection.

### From `ICheckoutService`

| Method | Signature |
|---|---|
| `GetDeliveryMethodsAsync` | `Task<IReadOnlyList<DeliveryInfoDto>> GetDeliveryMethodsAsync(AddressBookItemDto address)` |

### From `IAccountService`

| Method | Signature |
|---|---|
| `GetTrackingAsync` | `Task<TrackingDto?> GetTrackingAsync(Guid orderId)` |

---

## 6. Customer Management

**Services/Interfaces:** `CustomerProfileService`, `IAccountService` *(address, favourites, reviews and legacy profile members)*, `ICustomerService`
**Files:** `Services/CustomerProfileService.cs`, `Services/CustomerProfileState.cs`, `Services/CustomerDomainAccountService.cs`, `Services/IAccountService.cs`, `Services/ICustomerService.cs`

**Responsibility:** The complete customer data domain — both the self-service view (a logged-in customer managing their own profile, addresses, favourites, and reviews) and the backoffice operator view (looking up customers and their order history). All operations assume an authenticated context, either the customer themselves or an admin.

**Profile integration:** Customer-facing profile load/save/resend/verification flows use the concrete `CustomerProfileService` registered with the Aspire-discovered `customer-api` HttpClient. `CustomerProfileState` holds the latest live profile so profile displays refresh immediately after a successful save or email verification confirmation. This flow does not use `FakeAccountService`, `StaticProfileList`, or mock fallback data for customer-facing profile fields; failures are surfaced as user-safe service/validation results.

**Address integration:** Customer-facing saved-address reads and writes use `CustomerDomainAccountService`, which implements `IAccountService` for account and checkout components. `GetAddressesAsync` calls CustomerDomain `GET /api/customer/me/addresses`, `GetPrimaryAddressAsync` calls `GET /api/customer/me/addresses/default`, `AddAddressAsync` calls `POST /api/customer/me/addresses`, and `DeleteAddressAsync` calls `DELETE /api/customer/me/addresses/{addressId}` through the Aspire-discovered `customer-domain-api` HttpClient with the current customer context. Read methods return empty/null states on authorization, missing profile, no-default, or API failure; write methods return user-safe `ResultDto` failures and map validation problem field errors without logging address PII. Address operations must not fall back to `FakeAccountService`, `StaticProfileList`, or static address data, and must not mutate fake profile address state. Non-address account behavior remains delegated to `FakeAccountService` until separate features replace it.

**Requirements:**
- `CustomerProfileService` operations require an authenticated customer and call CustomerDomain `/api/customer/me` endpoints rather than trusting caller-supplied customer ids.
- Profile email changes create pending email verification in CustomerDomain; the verified/login email remains active until verification succeeds and AuthenticationManagement consumes the verified-email event.
- Profile service validation maps API validation/problem details to user-safe field feedback and must not log raw verification tokens or unnecessary PII.
- All `IAccountService` operations here require a logged-in customer (resolved via `IAuthService`).
- Address book reads/adds/deletes are CustomerDomain-backed and expose stable address ids plus deterministic indexes for existing UI display.
- `AddAddressAsync` maps CustomerDomain validation problems into `ResultDto.FieldErrors` and duplicate labels into user-safe conflict failures.
- `DeleteAddressAsync` uses stable address ids, not display indexes; deleting a default address relies on CustomerDomain default-promotion rules.
- Primary address designation must be enforced server-side through add/delete default rules; explicit set-primary remains unsupported in the Web account service for this phase.
- Favourites are per-user and per-product.
- Review submission must be idempotent — a customer may only submit one review per product, checked via `HasAlreadyReviewedProductIdAsync`.
- `ICustomerService` endpoints must be restricted to admin users only.
- `GetCustomerByIdAsync` returns `null` when the customer does not exist.
- `GetCustomerOrdersAsync` returns the full unpaginated order list for a given customer (operators need the full picture).

### From `CustomerProfileService` — Live CustomerDomain profile

| Method | Signature |
|---|---|
| `GetProfileAsync` | `Task<UserProfileDto?> GetProfileAsync()` |
| `UpdateProfileAsync` | `Task<ResultDto> UpdateProfileAsync(UserProfileDto profile)` |
| `ResendEmailVerificationAsync` | `Task<ResultDto> ResendEmailVerificationAsync(string pendingEmail)` |
| `ConfirmEmailVerificationAsync` | `Task<UserProfileDto?> ConfirmEmailVerificationAsync(string token)` |
| `HasBirthdateAsync` | `Task<bool> HasBirthdateAsync()` |

### From `IAccountService` — Self-service preferences and legacy profile members

| Method | Signature |
|---|---|
| `GetProfileAsync` | `Task<UserProfileDto?> GetProfileAsync()` *(legacy/mock-backed member; not used for customer-facing live profile display/save)* |
| `UpdateProfileAsync` | `Task<ResultDto> UpdateProfileAsync(UserProfileDto profile)` *(legacy/mock-backed member; not used for customer-facing live profile display/save)* |
| `HasBirthdateAsync` | `Task<bool> HasBirthdateAsync()` *(legacy/mock-backed member; not used for customer-facing live profile display/save)* |
| `GetAddressesAsync` | `Task<IList<AddressBookItemDto>> GetAddressesAsync()` |
| `AddAddressAsync` | `Task<ResultDto> AddAddressAsync(AddressBookItemDto address)` |
| `DeleteAddressAsync` | `Task<ResultDto> DeleteAddressAsync(Guid addressId)` |
| `SetAsPrimaryAsync` | `Task<ResultDto> SetAsPrimaryAsync(Guid addressId)` *(unsupported compatibility member; returns failure)* |
| `GetPrimaryAddressAsync` | `Task<AddressBookItemDto?> GetPrimaryAddressAsync()` |
| `AddToFavoritesAsync` | `Task AddToFavoritesAsync(Guid productId)` |
| `RemoveFromFavoritesAsync` | `Task RemoveFromFavoritesAsync(Guid productId)` |
| `IsFavoriteAsync` | `Task<bool> IsFavoriteAsync(Guid productId)` |
| `GetFavoriteProductsAsync` | `Task<ICollection<StorefrontProductDto>> GetFavoriteProductsAsync()` |
| `SubmitReviewAsync` | `Task SubmitReviewAsync(Guid productId, ReviewSubmitRequest review)` |
| `HasAlreadyReviewedProductIdAsync` | `Task<(bool, int)> HasAlreadyReviewedProductIdAsync(Guid productId)` |

### From `ICustomerService` — Backoffice customer lookup

| Method | Signature |
|---|---|
| `GetCustomersAsync` | `Task<ICollection<CustomerDto>> GetCustomersAsync()` |
| `GetCustomerByIdAsync` | `Task<CustomerDto?> GetCustomerByIdAsync(Guid id)` |
| `GetCustomerOrdersAsync` | `Task<ICollection<OrderListItemDto>> GetCustomerOrdersAsync(Guid customerId)` |

---

## 7. Store Management

**Interfaces:** `IImageUploadService`, `INewsletterService`, `IContactService`, `ILocationService`, `IOperationsService`, `IContentService`
**Files:** `Services/IImageUploadService.cs`, `Services/INewsletterService.cs`, `Services/IContactService.cs`, `Services/ILocationService.cs`, `Services/IOperationsService.cs`, `Services/IContentService.cs`

**Responsibility:** All store-level utilities and configuration that don't belong to a core transactional domain. Covers product media management, newsletter subscriptions, customer contact, Dutch address autocomplete, store metadata (hours, location), and static/CMS content such as FAQs. This domain is primarily support infrastructure consumed by both the storefront and the backoffice.

**Requirements:**
- `ProcessImageUploadAsync` must validate file type and size before upload; images are scoped to a `variantId`.
- `DeletePhotoAsync` must remove the image from both the storage backend and the variant record.
- `GetImagesForVariantAsync` must return images in display order.
- Image storage implementation may use cloud storage (e.g. Azure Blob, S3) or local file storage.
- `SendContactEmailAsync` must not expose internal SMTP errors to the caller — log server-side and return `false`.
- SMTP credentials must be read from configuration, not hard-coded.
- `LookupDutchAddressAsync` must return `null` gracefully for unknown postcodes; the overload with `addition` handles house number suffixes (e.g. "A", "bis").
- `GetStoreAsync` must return all data needed to render store info across the site (hours, address, name) and must be cacheable.
- `GetFaqsAsync` returns the full FAQ tree grouped by category; `SearchAsync` must match against both question and answer text.
- `INewsletterService` is currently a placeholder — expected to cover subscribe/unsubscribe and integrate with a third-party provider (e.g. Mailchimp, Klaviyo).

### `IImageUploadService` — Product media

| Method | Signature |
|---|---|
| `ProcessImageUploadAsync` | `Task<ProductImageDto> ProcessImageUploadAsync(IBrowserFile file, Guid variantId)` |
| `DeletePhotoAsync` | `Task DeletePhotoAsync(Guid imageId, Guid variantId)` |
| `GetImagesForVariantAsync` | `Task<List<ProductImageDto>> GetImagesForVariantAsync(Guid variantId)` |

### `INewsletterService` — Mailing list

| Method | Signature |
|---|---|
| — | *(interface is empty — methods pending)* |

### `IContactService` — Contact form

| Method | Signature |
|---|---|
| `SendContactEmailAsync` | `Task<bool> SendContactEmailAsync(string fromEmail, string subject, string message)` |

### `ILocationService` — Dutch address lookup

| Method | Signature |
|---|---|
| `LookupDutchAddressAsync` (no addition) | `Task<DutchAddressResult?> LookupDutchAddressAsync(string postCode, string streetNumber)` |
| `LookupDutchAddressAsync` (with addition) | `Task<DutchAddressResult?> LookupDutchAddressAsync(string postCode, string streetNumber, string addition)` |

### `IOperationsService` — Store settings

| Method | Signature |
|---|---|
| `GetStoreAsync` | `Task<StoreDto> GetStoreAsync()` |

### `IContentService` — Static content & FAQs

| Method | Signature |
|---|---|
| `SearchAsync` | `Task<IReadOnlyList<FaqCategoryItemDto>> SearchAsync(string value)` |
| `GetFaqsAsync` | `Task<IReadOnlyList<FaqCategoryDto>> GetFaqsAsync()` |

---

*Last updated: 2026-07-07 — reflects live CustomerDomain profile integration and Phase 6 documentation updates.*
