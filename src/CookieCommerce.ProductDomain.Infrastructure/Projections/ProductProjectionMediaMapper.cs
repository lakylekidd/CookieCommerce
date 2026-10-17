using CookieCommerce.ProductDomain.Application.Products.ReadModels;
using CookieCommerce.ProductDomain.Domain.ValueObjects;

namespace CookieCommerce.ProductDomain.Infrastructure.Projections;

internal static class ProductProjectionMediaMapper
{
    public static ProductMediaAssetReadModel ToReadModel(ProductMediaAsset asset) => new()
    {
        Id = asset.Id,
        ProductId = asset.ProductId,
        VariantId = asset.VariantId,
        Role = asset.Role.ToString(),
        Url = asset.Url,
        OriginalFileName = asset.OriginalFileName,
        ContentType = asset.ContentType,
        LengthBytes = asset.LengthBytes,
        Sha256 = asset.Sha256,
        AltText = asset.AltText,
        CreatedAt = asset.CreatedAt
    };
}
