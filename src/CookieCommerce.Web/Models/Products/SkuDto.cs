namespace CookieCommerce.Web.Models.Products;

public class SkuDto
{
    public Guid Id { get; set; }
    public Guid? ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Cost { get; set; }
    public int Stock { get; set; }
    public string SkuType { get; set; } = "Product";
    public ICollection<string> Ingredients { get; set; } = [];
}
