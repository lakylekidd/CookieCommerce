namespace CookieCommerce.Web.Models.Products;

public class InventoryItemDto
{
    public Guid ProductId { get; init; }
    public string ProductReference { get; init; } = string.Empty;
    public string ProductName { get; init; } = string.Empty;
    public string ProductImageUrl { get; init; } = string.Empty;
    public int Stock { get; init; }
    public bool Active { get; set; }
}
