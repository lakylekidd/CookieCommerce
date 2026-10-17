using CookieCommerce.Web.Models;

namespace CookieCommerce.Web.Services.Commerce;

public sealed class StoreSettingsService(ICommerceStore store, CommerceAccess access) : IOperationsService
{
    public Task<CommerceSettings?> LoadAsync() => store.LoadAsync<CommerceSettings>("store");
    public async Task<CommerceSettings> GetAsync() => await LoadAsync() ?? new();
    public async Task<StoreDto> GetStoreAsync() => new() { NextDayDeliveryBeforeTime = (await GetAsync()).NextDayDeliveryCutoff.ToTimeSpan() };

    public async Task SaveAsync(CommerceSettings settings)
    {
        await access.RequireAdminAsync();
        if (settings.StandardShippingCost < 0 || settings.FreeShippingThreshold < 0 || settings.VatRate < 0 || settings.VatRate > 100 || settings.MaxItemsPerOrder < 1)
            throw new InvalidOperationException("Shipping, tax and order limits must be valid positive values.");
        if (!System.Text.RegularExpressions.Regex.IsMatch(settings.OrderReferencePrefix, "^[A-Za-z0-9-]{1,12}$"))
            throw new InvalidOperationException("Enter an order reference prefix of up to 12 characters.");
        if (settings.Preference("pointsPerEuro", 10) < 0 || settings.Preference("pointsPerEuro", 10) > 1000 ||
            settings.Preference("minPointsToRedeem", 100) < 1 || settings.Preference("minOrderToEarnPoints", 0m) < 0 ||
            settings.Preference("dailyQuotaTarget", 50) < 0 || settings.Preference("lowStockThreshold", 10) < 0 ||
            settings.Preference("defaultBakingTemp", 180) <= 0 || settings.Preference("defaultBakingDuration", 12) <= 0 ||
            settings.Preference("maxDefectRatePercent", 5) is < 0 or > 100 ||
            !new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(settings.Preference("adminEmail", "admin@tigervancookie.nl")))
            throw new InvalidOperationException("Check the loyalty, inventory and notification settings.");
        if (settings.Promotions.Any(p => p.Discount < 0 || p.IsPercentage && p.Discount > 100 || string.IsNullOrWhiteSpace(p.Code)) ||
            settings.Promotions.GroupBy(p => p.Code.Trim(), StringComparer.OrdinalIgnoreCase).Any(g => g.Count() > 1))
            throw new InvalidOperationException("Promotion codes must be unique and discounts must be valid.");
        await store.ExecuteAsync("settings", async tx => { await tx.SaveAsync("store", settings); return true; });
    }
}
