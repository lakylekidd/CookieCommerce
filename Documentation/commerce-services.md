# UI services and live commerce setup

## Service audit

The production web application no longer registers or compiles mock services. Legacy fixtures now live under `CookieCommerce.Web.Tests/Fixtures/LegacyMocks`; they are retained for existing regression tests only.

| UI capability | Previous implementation | Live implementation |
| --- | --- | --- |
| Product browsing, details, ratings | Static product list and invented reviews | `CatalogService` reads ProductDomain and persisted customer reviews |
| Shopping cart | Singleton cart shared by visitors | Scoped `CartService`, protected browser identity, PostgreSQL persistence, customer cart merging |
| Checkout | Three unimplemented methods; submit button without handler | `CheckoutService` validates addresses and quantities, recalculates catalog prices and promotions, records an immutable order snapshot, starts hosted payment |
| Payments | Manual card fields without processing | Stripe Checkout by default, optional Mollie adapter, verified provider notifications, idempotent confirmation |
| Favorites, order history, reviews | Lost favorites; sample orders; discarded submissions | Persisted records scoped to the signed-in customer; reviews require a paid fulfilled purchase and are unique per customer/product |
| Profiles and address book | Customer API mixed with fake account methods | CustomerDomain profile/address APIs, including the existing default-address endpoint |
| Backoffice catalog and images | Mutations of static lists and data URLs | ProductDomain APIs, including activation/reactivation and real multipart media uploads |
| Backoffice orders and dashboards | Fixed totals and sample orders | Paid order snapshots, delivery-date quotas and calculated revenue/product rankings |
| Production inventory | Sample batches and unsaved edits | Persisted batches/runs and calculated stock, shortfalls and production quotas |
| Customer management | Invented customers | CustomerDomain list/detail endpoints enriched with actual order and subscription activity |
| Store settings | Local fields and simulated save | Persisted settings; checkout uses shipping, tax, cutoff, guest checkout, notes, order limits and promotions |
| FAQ | Static editorial copy embedded in a fake service | `ContentService` searches the editorial FAQ copy |
| Newsletter | Empty interface and a form without a handler | Validated, idempotent opt-in subscription storage |
| Loyalty | Fixed sample balance and nonworking redemption | Paid-order earnings, pending-payment reservations and validated redemption |
| Operational alerts | Settings without delivery | Durable SMTP outbox, production automation, dashboard sound and browser notifications |
| Password recovery | Successful no-op | Authentication API requests Keycloak's hosted UPDATE_PASSWORD email action |
| Contact form | SMTP | Existing SMTP service retained |
| Order receipt | Link to the shop | Owner-scoped receipt page with browser print/save as PDF |
| Sitemap and robots | Home-page-only placeholder and hardcoded hostname | Public routes and live catalog URLs using the configured site origin |

The shipping method is a store-configured estimate for Dutch addresses. It does not claim a carrier booking or fabricate tracking numbers. Orders can be confirmed as processing, ready, and delivered/collected by an administrator. Newsletter storage records opt-ins; it does not send marketing campaigns.

## Persistence and boundaries

Product and customer records continue to use their existing event-sourced APIs. Checkout, carts, reviews, favorites, subscriptions, operational settings, and production batches use a separate PostgreSQL database named **Commerce**, configured by AppHost. These features had no exposed domain APIs; the services under `CookieCommerce.Web/Services/Commerce` are an operational implementation behind the existing UI contracts. They do not pretend to expose or update the dormant OrderDomain read models. New website orders are stored in Commerce; historical external orders require an explicit migration or adapter.

`commerce_documents` stores encrypted JSON payloads. PostgreSQL advisory transaction locks serialize writes by business key, and payment confirmation updates the order and cart in one transaction. Persist and back up the Commerce database **and** the web app's Data Protection key directory. Losing the key ring prevents decryption. Set `DataProtection__KeyPath` to a persistent shared directory when deploying multiple web instances; keep the same application name and keys across instances. The default local path is `.data-protection` under the web content root.

The existing internal ProductDomain and CustomerDomain APIs must remain on the private service network. They still use the repository's existing trust model, including `X-Customer-Id` for customer profile/address requests. The web layer checks customer ownership and administrator access; this change is not a replacement for the planned API-level JWT authorization rollout.

Loyalty uses **100 points = EUR 1**, with **points or a promotion code**, as requested. Checkout reserves redeemed points while payment is pending, earns points only on paid orders, and releases reservations on failed/expired/cancelled payments. Earnings honor the configured points-per-euro, minimum order and birthday multiplier. Order snapshots preserve their own earnings and redeemed amounts. Marketing campaigns are outside newsletter opt-in storage.

Operational automation scans once a minute. Paid order alerts are queued transactionally with payment confirmation; low-stock alerts, new-customer alerts, high-defect alerts and daily summaries use a durable email queue. Failed SMTP attempts retry after five minutes. New-customer monitoring establishes a baseline when first enabled. The daily summary is generated after 18:00 Europe/Amsterdam. Optional low-stock batch creation accounts for existing planned/completed production, paid demand, stock thresholds and the production target, so repeated scans do not create duplicate batches. Baking runs use the configured temperature and duration defaults.

Appearance settings control the storefront tagline, featured count/flavor, badges and default theme; a visitor's saved theme preference takes priority. Dashboard sound and browser notifications work while the dashboard is open, after the browser grants notification permission. They do not implement closed-browser Web Push or mobile push delivery.

## Running locally

### Development seeding

Seeding runs in Development by default. `DevelopmentBootstrap__Enabled=false` disables the shared identity/product seed, customer address seed and web Commerce seed. Production does not create demo accounts or commerce data.

The identity seed keeps `admin@tiger.com` and `customer@tiger.com`, then adds eight fictional customers at `demo01@cookiecommerce.example` through `demo08@cookiecommerce.example`. All ten mock accounts use password `123`. Names are shuffled reproducibly. Development startup restores the mock passwords, enables these mock identities and clears failed-login restrictions. Real customer passwords are not reset by seeding. CustomerDomain creates encrypted profiles through the normal registration integration events and seeds missing addresses.

The ProductDomain seed retains the original six mock products, images and single/5/12-cookie variants. Commerce restores their original ingredient/cost recipes, six cookie SKUs, cellophane wrappers, and 5-, 8- and 12-cookie boxes. The original mock accidentally reused two cookie IDs for packaging; packaging now has distinct stable IDs. The mislabeled 4-cookie box is corrected to a 5-cookie box.

The web seed waits for the APIs and all ten profiles before creating 100 deterministically identified orders, distributed among those customers: 90 shipped, 2 ready to ship and 8 processing. Shipped orders have historical delivery dates; the ten active orders belong to today's workload. These are persisted order snapshots with `DEMO-` references, an `IsDevelopmentSeed` flag and a `Development` payment provider, not real charges. Paid demo orders appear in dashboard totals and loyalty/history; they are excluded from automated operational email and low-stock scheduling. A persisted seed revision applies this distribution once to existing demo orders while retaining their lines, prices, customer ownership and notes. Subsequent starts preserve status/date edits; real orders are excluded from the migration. Demo order thumbnails are refreshed from product main images.

Generic product thumbnails across cart/checkout, customer orders, administrator customer history, daily orders, dashboard rankings and inventory resolve the current main image directly from ProductDomain, including inactive products. Refreshing displayed order lines does not rewrite financial snapshots or saved live-order records. Missing/deleted main images use the logo placeholder rather than a variant photo; product variant galleries still show their own photos.

Completed seed production covers fulfilled demo orders and supplies an initial 30 cookies per flavor. On each restart, an explicit development stock adjustment restores the available balance to exactly 30 per cookie without rewriting production history. Packaging stock is restored to 100 while retaining other SKU fields. The inventory screen lists all SKU balances. Recipe assignment is seeded only where a variant has no saved recipe; an intentionally empty saved recipe remains empty.

AppHost retains PostgreSQL, RabbitMQ and Keycloak in named Docker volumes and persistent containers. Product media and Data Protection keys live under `%LOCALAPPDATA%/CookieCommerce` on Windows; set `DevelopmentStorage:RootPath` on AppHost to override this. Preserve these files with database backups. The old local media/key directories are not automatically deleted. If adopting these named volumes from an older anonymous-container setup with data to retain, migrate the old container data into the corresponding volumes before its first startup; changing storage locations does not migrate database records.

1. Start Docker and run `dotnet run --project src/CookieCommerce.AppHost` from the repository root. AppHost provisions PostgreSQL, RabbitMQ, Keycloak and the APIs, including the new Commerce database.
2. Create products and variants through the backoffice, or enable the repository's existing development bootstrap. Sample UI products are no longer used as fallback data.
3. Configure the web application with the environment variables below. Supply secrets through environment variables or a secrets manager, never source control.

| Variable | Value |
| --- | --- |
| `Payments__Provider` | `Stripe` (default) or `Mollie` |
| `Payments__PublicBaseUrl` | Public website origin, such as `https://shop.example.com`; local testing can use an HTTPS tunnel |
| `Payments__StripeSecretKey` | Stripe test/live secret API key |
| `Payments__StripeWebhookSecret` | Stripe endpoint signing secret, starting with `whsec_` |
| `Payments__MollieApiKey` | Required only for Mollie |
| `ConnectionStrings__Commerce` | PostgreSQL connection string when running outside AppHost |
| `DataProtection__KeyPath` | Persistent key-ring directory for deployment |

Missing payment credentials produce a checkout error and preserve the cart. They do not simulate a successful payment. The customer can retry the pending checkout or cancel it to edit their cart. Pending provider sessions are expired/cancelled before unlocking the cart.

For Stripe, register **snapshot** events at `POST /api/payments/stripe/webhook`:

- `checkout.session.completed`
- `checkout.session.async_payment_succeeded`
- `checkout.session.async_payment_failed`
- `checkout.session.expired`

For a local listener:

```powershell
stripe listen --forward-to https://localhost:<web-port>/api/payments/stripe/webhook
```

Use the listener's signing secret locally. The return page only displays persisted payment state; a successful redirect alone never marks an order paid. Webhooks check the stored session, currency and amount and safely tolerate repeated paid notifications. Mollie uses `POST /api/payments/mollie/webhook` and retrieves each payment from Mollie's authenticated API before applying its state.

Configure `Smtp:Host`, `Port`, `EnableSsl`, `From`, and optional `Username`/`Password` on the web application for the contact form and operational alerts. Configure SMTP on the Keycloak realm, and expose the correct Keycloak public hostname, for password recovery emails. Keycloak hosts the reset form; the old website token form cannot consume Keycloak action tokens.

## Verification

Build with `dotnet build src/CookieCommerce.slnx -p:SkipTailwind=true`. The optional property skips CSS generation for backend/service checks. Normal builds still run the existing Tailwind target.

The complete solution builds, including Tailwind. The latest Web regression run passes 106 tests, including the seed migration and main-image coverage. Previously verified unchanged suites passed ProductDomain 83, CustomerDomain 114, and AuthenticationManagement 7. Container-dependent infrastructure and integration suites were excluded. New workflow tests cover browser cart isolation and persistence, login/logout behavior, server repricing, payment-session reuse/cancellation, webhook signature/amount validation, duplicate payment confirmation, account ownership, review eligibility, subscriptions, activation, status controls, sitemap generation and recovery-page routing. Docker initially failed with a `dockerInference` socket error, but recovered on the subsequent local run. That earlier Aspire run started PostgreSQL, RabbitMQ, Keycloak and the APIs. Live checks passed for the storefront, catalog, cart, checkout, sign-in, product details, media, sitemap and health endpoint; the seeded administrator signed in through the browser and opened the backoffice dashboard. The run uses the verified local source copy under `%TEMP%/CookieCommerce-verification-20261014/src` to avoid UNC container bind paths. Docker recovered on the subsequent startup and the live seed completed successfully: ten customer profiles, six products, 100 orders and ten SKUs. The authenticated inventory screen confirmed 30 available units for every cookie and 100 for each packaging SKU. Prior local PostgreSQL, RabbitMQ and Keycloak data were copied into the named volumes, retaining the original stopped containers and volumes as backups. Copied Keycloak files required ownership by its container user (UID 1000, group 0) to avoid a read-only H2 database. The restarted live app confirmed exactly 90 shipped, 2 ready-to-ship and 8 processing demo orders, with all 204 demo lines matching current main images; cookie balances remained 30 per product and all four packaging SKUs held 100 units. Browser checks confirmed the uploaded Matcha main image in administrator orders and customer history/details. Full container-dependent test suites and real provider test-mode checkout still need to run before deployment.

The build still reports existing nullable/component warnings and NuGet vulnerability warnings in backend dependencies, including Marten 8.17. Dependency remediation and API-level JWT enforcement remain deployment prerequisites; they are not covered by the passing isolated tests.

Provider references: [Stripe Checkout sessions](https://docs.stripe.com/api/checkout/sessions/create), [Stripe webhook verification](https://docs.stripe.com/webhooks), [Mollie payments](https://docs.mollie.com/reference/create-payment), [Keycloak email actions](https://www.keycloak.org/docs-api/latest/rest-api/index.html).
