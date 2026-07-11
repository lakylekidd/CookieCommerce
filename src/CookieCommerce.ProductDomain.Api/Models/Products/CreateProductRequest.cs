using System.ComponentModel.DataAnnotations;

namespace CookieCommerce.ProductDomain.Api.Models.Products;

public class CreateProductRequest
{
    [Required] public string Name { get; set; } = string.Empty;
    [Required] public string Description { get; set; } = string.Empty;
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    public decimal Price { get; set; }
    public List<ProductVariantRequest> Variants { get; set; } = [];
}

public class ProductVariantRequest
{
    public Guid? Id { get; set; }
    [Required] public string Name { get; set; } = string.Empty;
    [Range(0.01, double.MaxValue, ErrorMessage = "Variant price must be greater than 0")]
    public decimal Price { get; set; }
    public string CurrencySymbol { get; set; } = "€";
    public bool IsActive { get; set; } = true;
    public string? Sku { get; set; }
}
