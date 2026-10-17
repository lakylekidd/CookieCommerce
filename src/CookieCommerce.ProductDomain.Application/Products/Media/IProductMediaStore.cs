using CookieCommerce.ProductDomain.Domain.ValueObjects;

namespace CookieCommerce.ProductDomain.Application.Products.Media;

public sealed class ProductMediaOptions
{
    public const string SectionName = "ProductMedia";

    public string StorageRootPath { get; set; } = Path.Combine(AppContext.BaseDirectory, "product-media");
    public string RequestPath { get; set; } = "/media/products";
    public long MaxFileSizeBytes { get; set; } = 5 * 1024 * 1024;
    public string[] AllowedContentTypes { get; set; } = ["image/jpeg", "image/png", "image/webp"];
}

public sealed record ProductMediaStoreWriteRequest(
    Guid ProductId,
    Guid? VariantId,
    ProductMediaRole Role,
    string OriginalFileName,
    string ContentType,
    Stream Content);

public sealed record ProductMediaStoreWriteResult(
    string StorageKey,
    string Url,
    long LengthBytes,
    string Sha256);

public interface IProductMediaStore
{
    Task<ProductMediaStoreWriteResult> SaveAsync(ProductMediaStoreWriteRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(string storageKey, CancellationToken cancellationToken);
}
