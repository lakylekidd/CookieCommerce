namespace CookieCommerce.Web.Models.Products;

public class ProductVariantDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CurrencySymbol { get; set; } = "€";
    public decimal CostPrice => Skus.Sum(sku => sku.Cost * sku.Quantity);
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public bool Active { get; set; }
    public ICollection<ProductVariantSkuDto> Skus { get; set; } = [];
    public ICollection<ProductImageDto> Images { get; set; } = [];
}