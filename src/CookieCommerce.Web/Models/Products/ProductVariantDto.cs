namespace CookieCommerce.Web.Models.Products;

public class ProductVariantDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CurrencySymbol { get; set; } = "€";
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public bool Active { get; set; }
}