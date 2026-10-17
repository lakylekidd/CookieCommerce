namespace CookieCommerce.Web.Models.Backoffice;

/// <summary>
/// The model that represents a product in today's inventory quota system.
/// </summary>
public class InventoryQuotaProductDto
{
    /// <summary>
    /// The unique identifier of the product.
    /// </summary>
    public Guid ProductId { get; set; }
    /// <summary>
    /// The name of the product.
    /// </summary>
    public string ProductName { get; set; } = string.Empty;
    /// <summary>
    /// The URL of the product image.
    /// </summary>
    public string ProductImageUrl { get; set; } = string.Empty;
}
