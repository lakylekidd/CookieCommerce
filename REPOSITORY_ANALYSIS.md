# CookieCommerce — Repository Analysis

> Generated: 2026-06-07

---

## 1. Purpose of the Application

CookieCommerce is the e-commerce platform for **Tiger van Cookie**, a Dutch artisan cookie business. The application covers the full customer journey and back-office operations:

- **Storefront** — Browse a curated cookie catalog, view product detail pages, manage a cart, and complete checkout as a guest, new, or returning customer.
- **Account** — Order history, address book, favourites, reviews, and profile management.
- **Backoffice** — Admin dashboard with order processing, product & variant management, inventory tracking, production batch management, branding settings, and quota/quota-order views.
- **Legal/Content** — FAQ, privacy policy, terms, refund policy, cookie policy, and an "Our Story" page.
- **Utility** — Track-and-trace, contact form, sitemap (`/sitemap.xml`), and robots.txt (`/robots.txt`).

The UI is a **Blazor Server application** targeting **.NET 10** that, at this stage, runs entirely on in-memory mock data — the backend domain services (Event Sourcing, CQRS) exist elsewhere in the solution and are not yet wired up to this web layer.

---

## 2. Architecture & Major Components

### 2.1 Technology Stack

| Layer | Technology |
|---|---|
| Runtime | .NET 10, ASP.NET Core |
| UI framework | Blazor Server (Interactive Server rendermode) |
| Styling | Tailwind CSS 4.2, Tailwind plugins (forms, typography, container-queries) |
| Auth | Fake cookie scheme (placeholder for real Identity/OpenIddict) |
| State | In-process memory (singleton cart) + ProtectedBrowserStorage (user email) |
| Address lookup | PDOK Dutch address API (real HTTP) |
| Analytics | Google Analytics, Google AdSense, Matomo, Microsoft Clarity |
| Observability | OpenTelemetry via `CookieCommerce.ServiceDefaults` (Aspire integration) |
| Containerisation | Docker multi-stage build (SDK 10 → ASP.NET 10, port 8080) |
| CSS build | Tailwind CLI invoked as a post-build MSBuild target |

### 2.2 Solution Layout

```
CookieCommerce/
├── src/
│   ├── CookieCommerce.Web/               ← This document's focus
│   ├── CookieCommerce.ServiceDefaults/   ← Aspire service defaults (telemetry, health)
│   ├── CookieCommerce.CustomerDomain/    ← Domain + Event Sourcing
│   ├── CookieCommerce.CatalogDomain/
│   ├── CookieCommerce.OrderDomain/
│   ├── CookieCommerce.ProductDomain/
│   ├── CookieCommerce.CustomerDomain.Tests/
│   ├── CookieCommerce.CatalogDomain.Tests/
│   ├── CookieCommerce.OrderDomain.Tests/
│   └── CookieCommerce.ProductDomain.Tests/
└── REPOSITORY_ANALYSIS.md
```

### 2.3 Blazor Component Architecture

```
App.razor
└── Routes.razor (CascadingAuthenticationState + AuthorizeRouteView)
    ├── MainLayout        ← Root: head tags, Google/Matomo/Clarity scripts (production only)
    │   ├── ShopLayout    ← + Navigation + Footer
    │   ├── AccountLayout ← + AccountHeader/Footer + aside nav
    │   ├── BackofficeLayout ← + BackofficeHeader + aside + shimmer states
    │   ├── AuthLayout    ← Minimal auth page wrapper
    │   └── ComingSoonLayout ← Default for unauthenticated routes
    └── Individual page components (see §4)
```

### 2.4 Service Layer

All services are interface-based and injected via DI. Every implementation is currently a **Fake/Mock** class backed by in-memory static data. Swap the registration in `Program.cs` to wire up real backend clients.

| Interface | Fake Implementation | Lifetime |
|---|---|---|
| `IAuthService` | `FakeAuthService` | Scoped |
| `AuthenticationStateProvider` | `FakeAuthStateProvider` | Scoped |
| `ICartService` | `MockCartService` | Singleton |
| `ICookieService` | `CookieService` | Scoped |
| `IAccountService` | `FakeAccountService` | Scoped |
| `IBackofficeService` | `FakeBackofficeService` | Scoped |
| `IInventoryService` | `FakeInventoryService` | Scoped |
| `ICheckoutService` | `FakeCheckoutService` | Scoped |
| `ILocationService` | `LocationService` | Scoped |
| `IContentService` | `FakeContentService` | Singleton |
| `IImageUploadService` | `FakeImageUploadService` | Scoped |
| `IOperationsService` | `FakeOperationsService` | Scoped |
| `ThemeService` | *(concrete)* | Scoped |

`INewsletterService` is declared but has no implementation yet.

---

## 3. Folder Structure Explanation

```
src/CookieCommerce.Web/
├── Components/
│   ├── Account/         Pages & sub-components for logged-in user area
│   ├── Auth/            Sign-in, sign-up, forgot/reset password pages
│   ├── Backoffice/      Admin dashboard, products, orders, inventory, settings, branding
│   ├── Layout/          All layout components (MainLayout, ShopLayout, etc.)
│   ├── Pages/           System pages: Error, NotFound, Contact, TrackAndTrace
│   ├── Shared/          Reusable UI: Breadcrumbs, SubscribeSection, ProductCard, etc.
│   └── Storefront/      Customer-facing: Home, Shop, Product detail, Cart, Checkout, Legal
├── Models/
│   ├── Account/         UserProfileDto
│   ├── Addresses/       AddressBookItemDto, DutchAddressResult, PdokResponse
│   ├── Auth/            UserDto, AuthResult, SignupDto
│   ├── Backoffice/      DashboardDto, BatchDto, OrderQuotaDto, InventoryAreaDto, etc.
│   ├── Cart/            CartDto, CartItemDto, CartPromotionDto
│   ├── Checkout/        CheckoutRequest variants, DeliveryInfoDto, AddressDto
│   ├── Content/         FaqCategoryDto
│   ├── Order/           OrderDto, OrderListItemDto, TrackingDto, QuotaOrder models
│   ├── Products/        ProductDto, StorefrontProductDto, ProductVariantDto, SkuDto, etc.
│   └── Reviews/         ReviewDto, ReviewSubmitRequest
├── Services/
│   ├── Products/        CookieService + static data lists (products, SKUs, inventory)
│   └── StaticData/      All mock/static in-memory datasets and fake service implementations
├── Styles/
│   └── app.css          Tailwind entry point with theme tokens, custom utilities, keyframes
├── wwwroot/
│   ├── css/             app.min.css (compiled output — do not edit directly)
│   ├── js/              global.js (theme), phone-helper.js, charting.js
│   ├── images/          Logo, social icons, QR codes
│   ├── fake-images/     Mock product photography (grouped by cookie variant)
│   └── lib/bootstrap/   Bootstrap grid/utilities
├── Properties/
│   └── launchSettings.json
├── App.razor            Root Blazor document
├── Routes.razor         Router with auth wrappers
├── _Imports.razor       Global @using directives
├── Program.cs           DI, middleware, app configuration
├── SiteMapExtensions.cs /sitemap.xml and /robots.txt endpoints
├── appsettings.json
├── appsettings.Development.json
├── CookieCommerce.Web.csproj
├── Dockerfile
└── package.json         Node deps for Tailwind CLI
```

---

## 4. Important Entry Points

### `Program.cs`
- Registers all services (see §2.4)
- Configures forwarded headers (reverse-proxy / homelab)
- Sets up DataProtection with key persistence to `/root/.aspnet/DataProtection-Keys`
- Registers the `"FakeCookies"` authentication scheme (placeholder)
- Calls `builder.AddServiceDefaults()` — wires Aspire health checks, service discovery, OpenTelemetry
- Maps Blazor (`MapRazorComponents<App>`) with `InteractiveServerRenderMode`
- Registers custom sitemap and robots endpoints via `SiteMapExtensions`

### `Routes.razor`
- Wraps the router in `CascadingAuthenticationState` and `AuthorizeRouteView`
- Falls back to `NotFound` component for unknown routes
- Default layout: `ComingSoonLayout` (shown for unauthenticated requests)

### Page Routes (summary)

| URL pattern | Component | Layout |
|---|---|---|
| `/storefront` | Storefront.razor | ShopLayout |
| `/shop` | MainShop.razor | ShopLayout |
| `/shop/cookie/{id:guid}` | Product.razor | ShopLayout |
| `/our-story` | OurStory.razor | ShopLayout |
| `/cart` | Cart.razor | ShopLayout |
| `/checkout` | Checkout.razor | ShopLayout |
| `/legal/*` | Legal sub-pages | ShopLayout |
| `/auth/signin` | CustomerSignin.razor | AuthLayout |
| `/auth/signup` | CustomerSignup.razor | AuthLayout |
| `/auth/forgot-password` | CustomerForgotPassword.razor | AuthLayout |
| `/auth/reset-password` | CustomerResetPassword.razor | AuthLayout |
| `/account` (and sub-paths) | Account pages | AccountLayout |
| `/backoffice/*` | Admin pages | BackofficeLayout |
| `/track-and-trace` | TrackAndTrace.razor | ShopLayout |
| `/contact` | Contact.razor | ShopLayout |

---

## 5. Database Structure & Migrations

**There is currently no database in the Web project.** All data is in-memory:

| Static Data Class | Contents |
|---|---|
| `StaticUserList` | Hardcoded user accounts used by `FakeAuthService` |
| `StaticProfileList` | User profile records |
| `StaticAddressList` | Address book entries |
| `StaticStorefrontProductList` | Full cookie product catalog |
| `StaticSkuList` | SKU definitions |
| `StaticInventoryList` | Inventory stock levels |
| `MockDashboardData` | Backoffice dashboard metrics |
| `MockOrderData` / `StaticOrders` | Sample order history |
| `StaticTodaysOrders` | Today's orders for the admin view |
| `StaticFaqList` | FAQ categories and questions |
| `TigerPalette` | Brand colour constants |

The domain projects (`CustomerDomain`, `OrderDomain`, etc.) use **Marten** (PostgreSQL-backed Event Store) with CQRS/Event Sourcing. Once the Web layer is connected to those APIs, the mock services will be replaced with HTTP clients.

**No EF Core migrations exist in this project.** The domain test projects include event store setup but not surfaced here.

---

## 6. External Integrations

### PDOK Dutch Address API (real, active)
`LocationService` calls `https://api.pdok.nl/bzk/locatieserver/search/v3_1/` for address auto-complete and lookup. This is the only live external HTTP dependency in the Web project.

### Analytics & Monitoring (production environment only, MainLayout.razor)
- **Google Analytics** — tag `G-HNS116Q0G5`
- **Google AdSense** — publisher `ca-pub-2415952858841772`
- **Matomo** — `matomo.vlachos.nl`
- **Microsoft Clarity** — key `vxvchlgg85`

All tracking scripts are wrapped in an `<Environment Names="Production">` guard so they don't fire in development.

### JavaScript Libraries (loaded in MainLayout / wwwroot)
- **Chart.js 3.9.1** — dashboard charts
- **canvas-confetti 1.9.2** — celebration animation on checkout completion
- **intl-tel-input 24.5.0** — international phone number input
- **Material Icons / Material Symbols** — Google icon fonts (online CDN)
- **Fredoka & Nunito** — Google Fonts (online CDN)
- **Bootstrap** — grid utilities only (vendored under `wwwroot/lib/bootstrap/`)

### .NET / NuGet
- `Microsoft.Extensions.AI 10.3.0` — AI extension abstractions (future use)
- `SmartComponents.LocalEmbeddings 0.1.0-preview` — local semantic search embeddings (future use)
- `CookieCommerce.ServiceDefaults` (project reference) — Aspire service defaults, OpenTelemetry, HTTP resilience

---

## 7. Build & Deployment Process

### Local Development

```bash
# Install Node deps (first time only)
npm install

# Run with hot-reload
dotnet watch run

# Or separately: watch Tailwind CSS
npm run tailwind:watch
```

The `.csproj` file includes a `Tailwind` MSBuild target that runs after every build:

```xml
npx @tailwindcss/cli -i ./Styles/app.css -o ./wwwroot/css/app.min.css --minify
```

The compiled output at `wwwroot/css/app.min.css` must be committed (or rebuilt on each deploy) — it is not auto-generated at runtime.

### Docker

The `Dockerfile` uses a two-stage build:

1. **Build stage** (`mcr.microsoft.com/dotnet/sdk:10.0`)
   - Installs Node.js 20 via apt-get
   - Restores NuGet packages
   - Installs Tailwind CLI (`npm install tailwindcss @tailwindcss/cli`)
   - Runs `dotnet publish -c Release` with `StaticWebAssetsEnabled=true`
   - Verifies Blazor framework files exist

2. **Runtime stage** (`mcr.microsoft.com/dotnet/aspnet:10.0`)
   - Exposes port **8080**
   - Sets `ASPNETCORE_URLS=http://+:8080`

Build & run:

```bash
docker build -t cookiecommerce-web .
docker run -p 8080:8080 cookiecommerce-web
```

### Launch Profiles

| Profile | URL |
|---|---|
| `http` | http://localhost:5124 |
| `https` | https://localhost:7298 |
| `Docker` | All ports exposed |

### CI/CD

No CI/CD pipeline files (`.github/workflows`, `azure-pipelines.yml`, etc.) were found in the repository at the time of this analysis.

---

## 8. Test Strategy

### Web Project
**No tests exist for the Web project.** The Blazor UI, service interfaces, and mock implementations are untested.

### Domain Projects
Unit/integration tests exist for the four domain projects:

| Test Project | Scope |
|---|---|
| `CookieCommerce.CustomerDomain.Tests` | Customer aggregate, events |
| `CookieCommerce.CatalogDomain.Tests` | Catalog aggregate |
| `CookieCommerce.OrderDomain.Tests` | Order aggregate, workflows |
| `CookieCommerce.ProductDomain.Tests` | Product aggregate |

These tests cover domain logic and event sourcing but do not test the Web layer at all.

### Gaps
- No Blazor component tests (bUnit)
- No integration tests for service layer
- No end-to-end tests (Playwright, Selenium)
- No contract tests between Web and future backend APIs

---

## 9. Areas of Technical Debt

### High Priority

| Issue | Location | Notes |
|---|---|---|
| All services are mocked | `Services/StaticData/` | Cart, auth, orders, products, inventory — none persisted |
| Fake authentication | `FakeAuthService`, `FakeAuthStateProvider` | localStorage-only, no real identity provider |
| Empty `CheckoutResult` | `Models/Checkout/CheckoutResult.cs` | No result model defined; checkout is a no-op |
| `INewsletterService` unimplemented | `Services/` | Interface declared, no concrete class |
| Hardcoded promo code | `MockCartService` | `"DISCOUNT10"` is baked in |
| `Console.WriteLine` in production code | `LocationService` | Should use `ILogger<LocationService>` |

### Medium Priority

| Issue | Location | Notes |
|---|---|---|
| Static sitemap | `SiteMapExtensions.cs:12-14` | TODO comment; needs DB-driven product URLs |
| Hardcoded domain | `SiteMapExtensions.cs` | `https://tigervancookie.nl` is hardcoded |
| Hardcoded analytics IDs | `MainLayout.razor` | Should come from configuration |
| Simulated latency | Various mock services | `Task.Delay(800)` scattered throughout |
| Singleton cart | `MockCartService` | Cart is lost on app restart |
| Cart/Order constants | `CartDto`, `OrderDto` | Tax, shipping, threshold hardcoded in model |

### Low Priority

| Issue | Location | Notes |
|---|---|---|
| Commented-out auth config | `Program.cs:51-58` | Placeholder for OpenIddict/Identity |
| Inconsistent null handling | Services | Mix of null checks and `ArgumentNullException.ThrowIfNull` |
| `AI` / `SmartComponents` packages | `.csproj` | Referenced but not used yet |

---

## 10. Onboarding a New Developer

### Prerequisites

- .NET 10 SDK
- Node.js 20+ (for Tailwind CSS CLI)
- Docker Desktop (optional, for containerised runs)
- An IDE: Visual Studio 2022 (17.12+) or Rider 2024.3+

### First Run

```bash
# Clone
git clone <repo-url>
cd CookieCommerce

# Install Node deps
cd src/CookieCommerce.Web
npm install

# Run (Tailwind is built automatically on dotnet build/watch)
dotnet watch run --project src/CookieCommerce.Web
```

Open https://localhost:7298. You should see the storefront.

### Key Credentials (Development)

Because authentication is fully mocked, any hardcoded user from `StaticUserList` will work. Check that file for available test accounts (e.g., an admin user with `Role = "Admin"` to access `/backoffice`).

### Mental Model

1. **Every service is fake.** Nothing hits a real database. Data resets on every app restart (except the user email in browser localStorage).
2. **Static data lives in `Services/StaticData/`.** This is the source of truth for all mock products, orders, users, and inventory.
3. **Layouts control what shell chrome appears.** Learn the layout hierarchy in §2.3 before building new pages.
4. **CSS is Tailwind 4.** Do not edit `wwwroot/css/app.min.css` directly — edit `Styles/app.css` and let the build step regenerate it.
5. **Dark mode is class-based.** `ThemeService` toggles a `dark` class on `<html>` via JS interop. Use Tailwind's `dark:` prefix for dark mode styles.
6. **Auth guard.** Pages requiring login use `@attribute [Authorize]`. The fake auth state reads from browser localStorage — sign in via `/auth/signin` to set it.

### Where to Look for What

| Question | Where to look |
|---|---|
| How is a page routed? | Component file, look for `@page "/..."` directive |
| Where does product data come from? | `Services/Products/StaticStorefrontProductList.cs` |
| How does cart work? | `Services/StaticData/MockCartService.cs` + `Models/Cart/CartDto.cs` |
| How does auth work? | `Services/StaticData/FakeAuthService.cs` + `FakeAuthStateProvider.cs` |
| How is theming handled? | `ThemeService.cs` + `wwwroot/js/global.js` + `Styles/app.css` |
| Where are shared UI components? | `Components/Shared/` |
| What HTTP clients are registered? | `Program.cs` (`AddHttpClient` calls) |
| How is the CSS built? | `.csproj` Tailwind target + `package.json` |

### Adding a New Page (checklist)

1. Create a `.razor` file under the appropriate `Components/` subfolder.
2. Add `@page "/your-route"` at the top.
3. Set `@layout YourLayout` (or inherit from the default).
4. Add the route to the sitemap in `SiteMapExtensions.cs` if it should be indexed.
5. Link to it from the navigation component if it needs a nav entry.

### Connecting to Real Backend Services

When the domain APIs are ready, replace each `Fake*` registration in `Program.cs` with an `Http*` client implementation:

```csharp
// Before
builder.Services.AddScoped<ICartService, MockCartService>();

// After
builder.Services.AddHttpClient<ICartService, CartApiClient>(client =>
    client.BaseAddress = new Uri("https://cart-api"));
```

OpenTelemetry, resilience, and service discovery are already wired via `AddServiceDefaults()`.
