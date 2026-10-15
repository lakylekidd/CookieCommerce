using CookieCommerce.Web.Models.Cart;
using CookieCommerce.Web.Models.Order;
using CookieCommerce.Web.Models.Products;
using CookieCommerce.Web.Models.Reviews;

namespace CookieCommerce.Web.Services.Commerce;

public sealed class CustomerFavorites
{
    public HashSet<Guid> ProductIds { get; set; } = [];
}

public sealed class CommerceOrder
{
    public bool InvoiceRequested { get; set; }
    public bool IsDevelopmentSeed { get; set; }
    public int RewardPointsEarned { get; set; }
    public int RewardPointsRedeemed { get; set; }
    public string CartKey { get; set; } = "";
    public Guid Id { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid CartId { get; set; }
    public string Email { get; set; } = "";
    public DateOnly DeliveryDate { get; set; }
    public string Note { get; set; } = "";
    public string PaymentProvider { get; set; } = "";
    public string? PaymentSessionId { get; set; }
    public string? PaymentUrl { get; set; }
    public string PaymentStatus { get; set; } = "Pending";
    public OrderDto Order { get; set; } = new();
}

public sealed class CustomerReview
{
    public Guid CustomerId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public ReviewDto Review { get; set; } = new();
}

public sealed class CatalogMetadata
{
    public string Reference { get; set; } = "";
    public bool Featured { get; set; }
    public bool LimitedEdition { get; set; }
    public Dictionary<Guid, List<ProductVariantSkuDto>> VariantSkus { get; set; } = [];
}

public sealed class StoredCart
{
    public CartDto Cart { get; set; } = new() { CartId = Guid.CreateVersion7() };
    public Guid? PendingOrderId { get; set; }
}

public static class LoyaltyRules
{
    public const decimal PointValue = 0.01m;
    public static int Balance(IEnumerable<CommerceOrder> orders, Guid customerId) => Math.Max(0,
        orders.Where(o => o.CustomerId == customerId && o.PaymentStatus == "Paid").Sum(o => o.RewardPointsEarned)
        - orders.Where(o => o.CustomerId == customerId && o.PaymentStatus is "Paid" or "Pending").Sum(o => o.RewardPointsRedeemed));

    public static int Earned(decimal total, CommerceSettings settings, bool birthday) =>
        total < settings.Preference("minOrderToEarnPoints", 0m) ? 0 :
        (int)decimal.Floor(total * Math.Max(0, settings.Preference("pointsPerEuro", 10)) * (birthday && settings.Preference("doubleBirthdayPoints", false) ? 2 : 1));
}

public sealed class PromotionDefinition
{
    public string Code { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal Discount { get; set; }
    public bool IsPercentage { get; set; } = true;
    public bool IsActive { get; set; } = true;
}

public sealed class CommerceSettings
{
    public T Preference<T>(string key, T fallback)
    {
        if (!AdditionalSettings.TryGetValue(key, out var json)) return fallback;
        try { return System.Text.Json.JsonSerializer.Deserialize<T>(json) ?? fallback; }
        catch (System.Text.Json.JsonException) { return fallback; }
    }
    public string OrderReferencePrefix { get; set; } = "TGR";
    public decimal FreeShippingThreshold { get; set; } = 40;
    public decimal StandardShippingCost { get; set; } = 4.95m;
    public decimal VatRate { get; set; } = 7;
    public TimeOnly NextDayDeliveryCutoff { get; set; } = new(18, 0);
    public bool AllowGuestCheckout { get; set; } = true;
    public bool AllowOrderNotes { get; set; } = true;
    public int MaxItemsPerOrder { get; set; } = 120;
    public List<PromotionDefinition> Promotions { get; set; } = [];
    public Dictionary<string, string> AdditionalSettings { get; set; } = [];
}
