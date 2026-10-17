namespace CookieCommerce.ProductDomain.Application.Products.ReadModels;

public class ProductMediaAssetReadModel
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid? VariantId { get; set; }
    public string Role { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long LengthBytes { get; set; }
    public string Sha256 { get; set; } = string.Empty;
    public string? AltText { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public class ProductVariantReadModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Sku { get; set; }
    public decimal Price { get; set; }
    public string CurrencySymbol { get; set; } = "€";
    public bool IsActive { get; set; } = true;
    public List<ProductMediaAssetReadModel> Photos { get; set; } = [];
}
