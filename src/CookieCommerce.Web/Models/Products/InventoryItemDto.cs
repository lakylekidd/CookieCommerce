namespace CookieCommerce.Web.Models.Products;

public class InventoryItemDto
{
    public Guid ProductId { get; set; }
    public string ProductReference { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string ProductImageUrl { get; set; } = string.Empty;
    public int Stock { get; set; }
    public bool Active { get; set; }
}
