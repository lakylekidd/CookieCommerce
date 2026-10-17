# Blazor UI design system

The existing UI is the visual source of truth. This is a small layer over the Tailwind v4 theme in `src/CookieCommerce.Web/Styles/app.css`, not a replacement for page-level composition. The compiled stylesheet is `wwwroot/css/app.min.css`; run `npm ci` in the Web project before building it locally.

The optional [UI Brandbook](../src/CookieCommerce.Web/Components/Brandbook/BrandbookPage.razor) at `/design-system` displays searchable specimens for all four catalog categories. It uses the real `ToggleNightMode`, Backoffice metric, Dashboard `AccountCard`, and order status components where they can render without application data. Product and table previews use labeled local mock content; their source paths identify the live components. The route is available only in Development with `Brandbook:Enabled=true`; Aspire passes that setting to Web and adds an **Open UI Brandbook** command. The route bypasses the storefront launch setting so the reference remains available before launch.

To add a specimen, register its owner, reuse scope, source path, and usage in `BrandbookCatalog.cs`, then add a preview for its ID in `BrandbookPage.razor`. Keep previews local and side-effect free; mark illustrative mockups so readers know to consult the live component for behavior.

## Shared foundation

- **Typography:** `Nunito` is the body font and `Fredoka` is the display and heading font. The root `App.razor` applies body text and light/dark surface colors. Material Symbols are the common icon system, usually at 18–24 px; keep a text label or accessible name on icon-only controls.
- **Theme:** Tailwind's existing `--color-primary`, surface, card, text, amber, and orange values are shared. The existing dark mode is toggled on the document root by `wwwroot/js/global.js` and `ToggleNightMode.razor`. Do not hard-code a light-only style where the surrounding page has a dark variant.
- **Spacing and motion:** `--ui-space-sm` (0.5rem), `--ui-space-lg` (1.5rem), and `--ui-duration-standard` (300ms) name values already used repeatedly. Tailwind's existing spacing scale remains in use; these tokens are for extracted patterns, not a new scale.
- **Keyboard access:** `ui-focus-visible` gives extracted action buttons a 2px primary outline with a 2px offset. Native buttons and links retain keyboard behavior. Maintain visible focus, disabled feedback, and `cursor-pointer` where actions already use them. Inputs generally use a primary focus border/ring; preserve page-specific focus treatments.
- **Reuse boundary:** `Components/Shared` contains truly cross-area pieces such as `Breadcrumbs`, `ToggleNightMode`, and product/order fragments. `Components/UI` is reserved for small reusable domain patterns. Avoid a universal button or card that erases meaningful differences.

## Web Shop

- **Pages inspected:** `Storefront`, `MainShop`, `Cart`, product cards, and the FAQ. `ShopLayout` owns the storefront navigation/footer and scopes `shop-ui`.
- **Appearance:** airy container layouts, display headings, amber/orange brand surfaces, rounded full call-to-action links, large rounded product cards, soft cookie shadows, and occasional playful motion. `--shop-heading-gap` keeps the shop page heading's existing 0.5rem gap. `shop-page-title` extracts the existing `MainShop` heading treatment without changing its appearance.
- **Components:** reuse `CookieCard`, which delegates to `FeaturedCookieCard` or `RegularCookieCard`; reuse `VariantSelector`, `ProductImageSelector`, and checkout-specific cards in their current area. The product card's hover lift, image zoom, badge, add-to-cart loading/checkmark, disabled state, and responsive controls stay together.
- **Forms and navigation:** shop navigation uses the shared `Navigation` component; checkout has its own focused layout. Search, address, and checkout forms use rounded fields with a primary focus border/ring. FAQ disclosure uses native `<details>` and a rotating icon. Preserve the shop's larger spacing and stronger hover motion.

## Backoffice

- **Pages inspected:** Dashboard, Products, Customers, Inventory tables, and Settings. `BackofficeLayout` scopes `backoffice-ui` and provides the fixed sidebar, sticky header, scrollable content region, and compact footer.
- **Appearance:** denser `p-8` work area, rounded 2xl panels, thin gray borders, compact table text, restrained shadows, and primary orange actions. `--backoffice-action-radius` retains the Products action's `rounded-2xl` corner radius (2rem in this project's theme). `backoffice-action` reproduces its uppercase bordered action with hover fill, slight active scale, focus outline, and disabled state. Customers' audit action now follows that same convention.
- **Components:** reuse `CustomersTableCard`, `BatchesTableCard`, `InventoryTable`, their row components, and dashboard metric cards. These panels own search/filter, pagination, sorting, skeleton, and empty-state behavior. Keep table-specific interactions inside the table component.
- **Loading:** the existing amber `animate-skeleton` shimmer is defined once in `Styles/app.css` so Backoffice panels and the Brandbook metric preview use the same animation.
- **Forms and navigation:** Settings panels and product modals keep their specialized inputs. The sidebar and mobile menu use `NavLink`; the header contains search, theme toggle, and notifications. Check modal buttons, sort controls, row actions, and pagination for focus and disabled states when touching those components.

## Customer Dashboard

- **Pages inspected:** Overview, Addresses, Orders, Favorites, and profile cards. `AccountLayout` scopes `dashboard-ui`, with a narrower content area, account sidebar, and header/footer.
- **Appearance:** friendly but calmer than the shop: `rounded-xl` profile cards (1.5rem in this project's theme), light gray borders, white/dark card surfaces, 1.5rem card padding, orange action links, and occasional reward gradients. `--dashboard-card-radius` and `--dashboard-card-padding` preserve the profile card values. `AccountCard` wraps the matching Overview cards without changing their content.
- **Components:** use `AccountCard` for the plain Overview profile cards, `AccountAddressCard` for address management, `OrdersHeader` and order row/status components for order history. Do not force the dashed Favorites and add-address cards into the plain card shell; their different borders and motion are intentional.
- **Forms, status, navigation:** account forms and address modals keep their existing validation and loading feedback. Orders use a branded loading overlay and skeleton rows; address actions show a spinner and disabled state. Status badges are small, rounded, and colored by state. The account sidebar is distinct from the Backoffice sidebar.

## Adoption and follow-up

The representative migration is `MainShop`'s heading, Products and Customers actions, and Overview's two profile cards. The shared addition is a small set of spacing, motion, and focus tokens; the domain additions are the scoped tokens and styles above. No general-purpose UI framework or cross-domain visual normalization was introduced.

The Customers audit button previously had no hover, active, focus, cursor, or disabled styling matching its neighboring Backoffice actions. It now uses `backoffice-action`. Other page-specific visual differences remain as found, including Shop product-card motion, Backoffice table density, and Dashboard reward/favorites treatments.

No clearly accidental Shop or Customer Dashboard mismatch was found in the representative pages inspected. The different Shop product-card variants and Dashboard reward/favorites surfaces were left as intentional variants.

When extending the system, first compare multiple pages in the same domain. Extract only a repeated pattern, then migrate a small sample and verify light/dark, keyboard focus, disabled/loading states, and narrow layouts. A useful later pass is to audit icon-only controls and repeated table filter/pagination states for accessible names and focus visibility.

```razor
<AccountCard>
    <h2 class="text-xl font-display font-bold">Personal Details</h2>
    <!-- Account-specific content -->
</AccountCard>

<button type="button" class="backoffice-action ui-focus-visible" disabled="@isSaving">
    Save changes
</button>
```
