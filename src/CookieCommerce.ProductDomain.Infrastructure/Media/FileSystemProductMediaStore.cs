using System.Security.Cryptography;
using CookieCommerce.ProductDomain.Application.Products.Media;
using CookieCommerce.ProductDomain.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CookieCommerce.ProductDomain.Infrastructure.Media;

public sealed class FileSystemProductMediaStore(
    IOptions<ProductMediaOptions> options,
    ILogger<FileSystemProductMediaStore> logger) : IProductMediaStore
{
    private readonly ProductMediaOptions _options = options.Value;

    public async Task<ProductMediaStoreWriteResult> SaveAsync(ProductMediaStoreWriteRequest request, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(_options.StorageRootPath);

        var safeFileName = BuildSafeFileName(request.OriginalFileName);
        var relativeDirectory = BuildRelativeDirectory(request.ProductId, request.VariantId, request.Role);
        var targetDirectory = Path.Combine(_options.StorageRootPath, relativeDirectory);
        Directory.CreateDirectory(targetDirectory);

        var storageFileName = $"{Guid.CreateVersion7():N}-{safeFileName}";
        var storageKey = Path.Combine(relativeDirectory, storageFileName).Replace(Path.DirectorySeparatorChar, '/');
        var physicalPath = Path.Combine(targetDirectory, storageFileName);

        await using var output = File.Create(physicalPath);
        using var sha256 = SHA256.Create();
        await using var hashingStream = new CryptoStream(output, sha256, CryptoStreamMode.Write);
        await request.Content.CopyToAsync(hashingStream, cancellationToken);
        await hashingStream.FlushAsync(cancellationToken);
        hashingStream.FlushFinalBlock();

        var lengthBytes = new FileInfo(physicalPath).Length;
        var hash = Convert.ToHexString(sha256.Hash ?? []).ToLowerInvariant();
        var url = $"{_options.RequestPath.TrimEnd('/')}/{storageKey}";

        logger.LogInformation(
            "Stored product media asset for product {ProductId} variant {VariantId} role {Role} with {LengthBytes} bytes and storage key hash {StorageKeyHash}.",
            request.ProductId,
            request.VariantId,
            request.Role,
            lengthBytes,
            HashStorageKeyForLogging(storageKey));

        return new ProductMediaStoreWriteResult(storageKey, url, lengthBytes, hash);
    }

    public Task DeleteAsync(string storageKey, CancellationToken cancellationToken)
    {
        var normalizedKey = storageKey.Replace('/', Path.DirectorySeparatorChar);
        var physicalPath = Path.GetFullPath(Path.Combine(_options.StorageRootPath, normalizedKey));
        var rootPath = Path.GetFullPath(_options.StorageRootPath);

        if (!physicalPath.StartsWith(rootPath, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Media storage key resolves outside the configured storage root.");

        if (File.Exists(physicalPath))
        {
            File.Delete(physicalPath);
            logger.LogInformation("Deleted product media asset with storage key hash {StorageKeyHash}.", HashStorageKeyForLogging(storageKey));
        }

        return Task.CompletedTask;
    }

    private static string BuildRelativeDirectory(Guid productId, Guid? variantId, ProductMediaRole role)
    {
        return role switch
        {
            ProductMediaRole.MainImage => Path.Combine(productId.ToString("N"), "main"),
            ProductMediaRole.VariantPhoto when variantId is not null => Path.Combine(productId.ToString("N"), "variants", variantId.Value.ToString("N")),
            _ => throw new InvalidOperationException("Variant photo media requires a variant id.")
        };
    }

    private static string BuildSafeFileName(string originalFileName)
    {
        var fileName = Path.GetFileName(originalFileName);
        var invalidCharacters = Path.GetInvalidFileNameChars();
        var sanitized = new string(fileName.Select(character => invalidCharacters.Contains(character) ? '-' : character).ToArray());
        return string.IsNullOrWhiteSpace(sanitized) ? "upload" : sanitized;
    }

    private static string HashStorageKeyForLogging(string storageKey)
    {
        var hash = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(storageKey));
        return Convert.ToHexString(hash[..8]).ToLowerInvariant();
    }
}
