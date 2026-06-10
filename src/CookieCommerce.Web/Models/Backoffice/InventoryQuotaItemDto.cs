namespace CookieCommerce.Web.Models.Backoffice;

/// <summary>
/// Represents an item in today's inventory quota.
/// </summary>
public class InventoryQuotaItemDto
{
    /// <summary>
    /// The quantity of the product that is required to meet today's fulfillment.
    /// </summary>
    public int QuantityRequired { get; set; }
    /// <summary>
    /// The quantity of the product that is ready for fulfillment today.
    /// </summary>
    public int QuantityReadyForFulfillment { get; set; }
    /// <summary>
    /// The product details required associated with this inventory quota item.
    /// </summary>
    public InventoryQuotaProductDto Product { get; set; } = new();
    /// <summary>
    /// The status of this inventory quota item.
    /// </summary>
    public InventoryQuotaItemStatus Status { get; set; } = InventoryQuotaItemStatus.NotStarted;
}
