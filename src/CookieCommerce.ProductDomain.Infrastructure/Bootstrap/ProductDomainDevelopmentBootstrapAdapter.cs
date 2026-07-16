using CookieCommerce.DevBootstrap;
using CookieCommerce.ProductDomain.Application.Products.Integration;
using CookieCommerce.ProductDomain.Application.Products.Media;
using CookieCommerce.ProductDomain.Domain.Aggregates;
using CookieCommerce.ProductDomain.Domain.ValueObjects;
using CookieCommerce.DomainBuildingBlocks;
using Marten;
using Microsoft.Extensions.Logging;

namespace CookieCommerce.ProductDomain.Infrastructure.Bootstrap;

public sealed class ProductDomainDevelopmentBootstrapAdapter(
    IDocumentSession session,
    IProductMediaStore mediaStore,
    IIntegrationEventPublisher publisher,
    ILogger<ProductDomainDevelopmentBootstrapAdapter> logger) : IDevelopmentProductBootstrapAdapter
{
    public async Task EnsureProductAsync(
        DevelopmentBootstrapProduct productDefault,
        string staticWebRootPath,
        CancellationToken cancellationToken)
    {
        var existing = await session.Events.AggregateStreamAsync<ProductAggregate>(
            productDefault.ProductId,
            token: cancellationToken);

        if (existing is not null)
        {
            logger.LogInformation(
                "Development bootstrap skipped existing product {ProductReference} ({ProductId})",
                productDefault.ProductReference,
                productDefault.ProductId);
            return;
        }

        var product = new ProductAggregate();
        product.Create(
            productDefault.ProductId,
            productDefault.Name,
            productDefault.Description,
            productDefault.BasePrice);

        foreach (var variant in productDefault.Variants)
        {
            product.AddVariant(
                variant.VariantId,
                variant.Name,
                variant.Price,
                variant.CurrencySymbol,
                variant.IsActive,
                variant.Sku);
        }

        if (productDefault.MainImage is not null)
        {
            var mainImage = await SaveMediaAssetAsync(
                productDefault.ProductId,
                variantId: null,
                ProductMediaRole.MainImage,
                productDefault.MainImage,
                productDefault.Name,
                staticWebRootPath,
                cancellationToken);

            product.SetMainImage(mainImage);
        }

        foreach (var variant in productDefault.Variants)
        {
            foreach (var photo in variant.Photos)
            {
                var variantPhoto = await SaveMediaAssetAsync(
                    productDefault.ProductId,
                    variant.VariantId,
                    ProductMediaRole.VariantPhoto,
                    photo,
                    $"{productDefault.Name} {variant.Name}",
                    staticWebRootPath,
                    cancellationToken);

                product.AddVariantPhoto(variant.VariantId, variantPhoto);
            }
        }

        session.Events.StartStream<ProductAggregate>(productDefault.ProductId, product.DomainEvents);

        await publisher.PublishAsync(
            new ProductSnapshotPublished(
                productDefault.ProductId,
                productDefault.Name,
                productDefault.Description,
                productDefault.BasePrice,
                productDefault.IsActive),
            cancellationToken);

        await session.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Development bootstrap seeded product {ProductReference} ({ProductId}) with {VariantCount} variants",
            productDefault.ProductReference,
            productDefault.ProductId,
            productDefault.Variants.Count);
    }

    private async Task<ProductMediaAsset> SaveMediaAssetAsync(
        Guid productId,
        Guid? variantId,
        ProductMediaRole role,
        DevelopmentBootstrapMediaAsset mediaDefault,
        string fallbackAltText,
        string staticWebRootPath,
        CancellationToken cancellationToken)
    {
        var filePath = DevelopmentBootstrapService.ResolveStaticWebRootMediaPath(staticWebRootPath, mediaDefault.SourcePath);
        await using var stream = File.OpenRead(filePath);

        var writeResult = await mediaStore.SaveAsync(
            new ProductMediaStoreWriteRequest(
                productId,
                variantId,
                role,
                Path.GetFileName(filePath),
                GetContentType(filePath),
                stream),
            cancellationToken);

        return new ProductMediaAsset(
            mediaDefault.MediaAssetId,
            productId,
            variantId,
            role,
            writeResult.StorageKey,
            writeResult.Url,
            Path.GetFileName(filePath),
            GetContentType(filePath),
            writeResult.LengthBytes,
            writeResult.Sha256,
            mediaDefault.AltText ?? fallbackAltText,
            DateTimeOffset.UtcNow,
            true);
    }

    private static string GetContentType(string path) => Path.GetExtension(path).ToLowerInvariant() switch
    {
        ".jpg" or ".jpeg" => "image/jpeg",
        ".png" => "image/png",
        ".webp" => "image/webp",
        _ => "application/octet-stream"
    };
}
