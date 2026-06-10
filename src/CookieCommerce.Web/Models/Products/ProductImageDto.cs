namespace CookieCommerce.Web.Models.Products;

public class ProductImageDto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Url { get; set; } = string.Empty;
}