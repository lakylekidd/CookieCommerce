namespace CookieCommerce.Web.Models.Backoffice;
/// <summary>
/// This is the model for the inventory area of the backoffice dashboard.
/// </summary>
public class InventoryAreaDto
{
    public ICollection<CookieCommerce.Web.Models.Products.SkuDto> Skus { get; set; } = [];
    /// <summary>
    /// This contains today's order quota information
    /// </summary>
    public OrderQuotaDto OrderQuota { get; set; } = new();
    /// <summary>
    /// This contains a list of today's inventory vs the required inventory needed.
    /// </summary>
    public ICollection<InventoryQuotaItemDto> InventoryQuota { get; set; } = [];
    /// <summary>
    /// Contains the batches that are currently active in the system and are either
    /// scheduled, being mixed, or resting. This does not include batches that have 
    /// been completed or cancelled.
    /// </summary>
    public ICollection<BatchDto> Batches { get; set; } = [];
}
