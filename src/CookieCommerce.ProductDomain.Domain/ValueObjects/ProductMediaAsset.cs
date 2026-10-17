namespace CookieCommerce.ProductDomain.Domain.ValueObjects;

public enum ProductMediaRole
{
    MainImage = 1,
    VariantPhoto = 2
}

public sealed record ProductMediaAsset(
    Guid Id,
    Guid ProductId,
    Guid? VariantId,
    ProductMediaRole Role,
    string StorageKey,
    string Url,
    string OriginalFileName,
    string ContentType,
    long LengthBytes,
    string Sha256,
    string? AltText,
    DateTimeOffset CreatedAt,
    bool IsCurrent = true);
