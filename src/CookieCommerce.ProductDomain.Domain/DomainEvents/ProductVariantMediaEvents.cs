using CookieCommerce.ProductDomain.Domain.ValueObjects;

namespace CookieCommerce.ProductDomain.Domain.DomainEvents;

public class ProductVariantAdded
{
    public Guid ProductId { get; set; }
    public Guid VariantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Sku { get; set; }
    public decimal Price { get; set; }
    public string CurrencySymbol { get; set; } = "€";
    public bool IsActive { get; set; } = true;
}

public class ProductVariantUpdated
{
    public Guid ProductId { get; set; }
    public Guid VariantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Sku { get; set; }
    public decimal Price { get; set; }
    public string CurrencySymbol { get; set; } = "€";
    public bool IsActive { get; set; } = true;
}

public class ProductVariantRemoved
{
    public Guid ProductId { get; set; }
    public Guid VariantId { get; set; }
}

public class ProductMainImageSet
{
    public Guid ProductId { get; set; }
    public ProductMediaAsset MediaAsset { get; set; } = default!;
}

public class ProductMainImageRemoved
{
    public Guid ProductId { get; set; }
    public Guid MediaAssetId { get; set; }
}

public class VariantPhotoAdded
{
    public Guid ProductId { get; set; }
    public Guid VariantId { get; set; }
    public ProductMediaAsset MediaAsset { get; set; } = default!;
}

public class VariantPhotoRemoved
{
    public Guid ProductId { get; set; }
    public Guid VariantId { get; set; }
    public Guid MediaAssetId { get; set; }
}
