namespace CookieCommerce.ProductDomain.Api.Models.Products;

public sealed record ProductMediaAssetResponse(
    Guid Id,
    Guid ProductId,
    Guid? VariantId,
    string Role,
    string Url,
    string OriginalFileName,
    string ContentType,
    long LengthBytes,
    string? AltText);

public sealed record ProductVariantResponse(
    Guid Id,
    string Name,
    string? Sku,
    decimal Price,
    string CurrencySymbol,
    bool IsActive,
    IReadOnlyList<ProductMediaAssetResponse> Photos);

public sealed record ProductDetailsResponse(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    bool IsActive,
    ProductMediaAssetResponse? MainImage,
    IReadOnlyList<ProductVariantResponse> Variants);

public sealed record ProductListItemResponse(
    Guid Id,
    string Name,
    decimal Price,
    bool IsActive,
    ProductMediaAssetResponse? MainImage,
    IReadOnlyList<ProductVariantResponse> Variants);
