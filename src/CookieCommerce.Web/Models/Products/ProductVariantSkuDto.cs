namespace CookieCommerce.Web.Models.Products;

public class ProductVariantSkuDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Cost { get; set; }
    public decimal TotalCost => Cost * Quantity;
}