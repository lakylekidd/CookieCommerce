namespace CookieCommerce.Web.Models.Products;

public class ProductImageDto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? ProductId { get; set; }
    public Guid? VariantId { get; set; }
    public string Role { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long LengthBytes { get; set; }
    public string? AltText { get; set; }
}