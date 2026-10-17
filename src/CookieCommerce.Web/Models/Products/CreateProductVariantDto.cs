namespace CookieCommerce.Web.Models.Products;

public class CreateProductVariantDto
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string CurrencySymbol { get; set; } = "€";
}
