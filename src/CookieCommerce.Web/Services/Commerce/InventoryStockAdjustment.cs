namespace CookieCommerce.Web.Services.Commerce;

/// <summary>An explicit balance correction, separate from editable production history.</summary>
public sealed class InventoryStockAdjustment
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public DateOnly EffectiveDate { get; set; }
    public string Reason { get; set; } = "";
}
