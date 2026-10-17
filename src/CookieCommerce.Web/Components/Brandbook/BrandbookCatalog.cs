namespace CookieCommerce.Web.Components.Brandbook;

public sealed record BrandbookEntry(
    string Id,
    string Name,
    string Domain,
    string Purpose,
    string Reuse,
    string Source,
    string Usage,
    bool Illustrative = false);

public static class BrandbookCatalog
{
    public static IReadOnlyList<BrandbookEntry> Entries { get; } =
    [
        new("foundation", "Color and typography", "Shared", "Theme colors, fonts, spacing, and radii used throughout the app.",
            "Global foundation", "Styles/app.css", "font-display · font-body · text-primary · --ui-space-lg"),
        new("theme", "Theme control", "Shared", "The existing dark-mode control and visible action states.",
            "Shared component", "Components/Shared/ToggleNightMode.razor", "<ToggleNightMode />"),
        new("shop-card", "Featured product card", "Web Shop", "Product imagery, badges, price, and a compact add action.",
            "Shop only", "Components/Storefront/Products/FeaturedCookieCard.razor", "<CookieCard IsFeatured=\"true\" ... />", true),
        new("shop-actions", "Shop actions and form", "Web Shop", "Rounded purchase actions and spacious, warm form fields.",
            "Shop only", "Components/Storefront/Home/Storefront.razor", "bg-primary rounded-full · focus:border-primary", true),
        new("backoffice-action", "Administrative action", "Backoffice", "Bordered primary action with hover fill, active scale, focus, and disabled feedback.",
            "Backoffice only", "Components/Backoffice/ProductsArea/Products.razor", "<button class=\"backoffice-action ui-focus-visible\">…</button>"),
        new("backoffice-metric", "Metric card", "Backoffice", "A compact metric with trend state and loading skeleton.",
            "Backoffice only", "Components/Backoffice/DashboardArea/TotalOrders.razor", "<TotalOrders Total=\"128\" PercentageChange=\"12\" />"),
        new("backoffice-table", "Data table", "Backoffice", "Dense rows, sorting/filter controls, status, and empty state.",
            "Backoffice only", "Components/Backoffice/CustomersArea/CustomersTableCard.razor", "<CustomersTableCard Customers=\"customers\" IsLoading=\"false\" />", true),
        new("dashboard-card", "Account card", "Customer Dashboard", "The plain profile card shell used on Overview.",
            "Dashboard only", "Components/UI/CustomerDashboard/AccountCard.razor", "<AccountCard>…</AccountCard>"),
        new("dashboard-status", "Order status", "Customer Dashboard", "Order progression badges with color and motion tied to status.",
            "Dashboard only", "Components/Account/Orders/OrderStatus.razor", "<OrderStatus Status=\"status\" />"),
        new("dashboard-empty", "Address empty state", "Customer Dashboard", "A calm prompt for missing saved addresses.",
            "Dashboard only", "Components/Account/Addresses.razor", "No saved shipping spots yet", true)
    ];

    public static IEnumerable<BrandbookEntry> Filter(string domain, string query)
    {
        var entries = domain == "All"
            ? Entries
            : Entries.Where(entry => entry.Domain == domain);

        return string.IsNullOrWhiteSpace(query)
            ? entries
            : entries.Where(entry =>
                entry.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                entry.Purpose.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                entry.Source.Contains(query, StringComparison.OrdinalIgnoreCase));
    }
}
