namespace CookieCommerce.Web.Models.Products;

public class CreateProductDto
{
    public string ProductReference { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public ICollection<CreateProductVariantDto> Variants { get; set; } = [];
}
