namespace CookieCommerce.Web.Models.Products;

public class ProductVariantImageDto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Url { get; set; } = string.Empty;
    public bool IsMain { get; set; }
}