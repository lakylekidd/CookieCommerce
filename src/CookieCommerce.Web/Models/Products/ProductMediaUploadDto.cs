namespace CookieCommerce.Web.Models.Products;

public sealed record ProductMediaUploadDto(
    Stream Content,
    string FileName,
    string ContentType,
    long LengthBytes,
    string? AltText = null,
    int? SortOrder = null);

public sealed record UpsertProductVariantDto(
    Guid? Id,
    string Name,
    decimal Price,
    string CurrencySymbol = "€",
    bool IsActive = true,
    string? Sku = null);
