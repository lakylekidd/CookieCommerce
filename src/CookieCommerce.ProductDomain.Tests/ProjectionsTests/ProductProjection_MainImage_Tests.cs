using CookieCommerce.ProductDomain.Application.Products.ReadModels;
using CookieCommerce.ProductDomain.Domain.DomainEvents;
using CookieCommerce.ProductDomain.Domain.ValueObjects;
using CookieCommerce.ProductDomain.Infrastructure.Projections;
using FluentAssertions;

namespace CookieCommerce.ProductDomain.Tests.ProjectionsTests;

public class ProductProjection_MainImage_Tests
{
    [Fact]
    public void Apply_SetsMainImage_GivenProductMainImageSetEvent()
    {
        var projection = new ProductDetailsProjection();
        var productId = Guid.CreateVersion7();
        var readModel = CreateReadModel(productId, Guid.CreateVersion7());
        var mainImage = CreateMainImage(productId, "main-a.png");

        projection.Apply(new ProductMainImageSet { ProductId = productId, MediaAsset = mainImage }, readModel);

        readModel.MainImage.Should().BeEquivalentTo(new ProductMediaAssetReadModel
        {
            Id = mainImage.Id,
            ProductId = productId,
            VariantId = null,
            Role = ProductMediaRole.MainImage.ToString(),
            Url = mainImage.Url,
            OriginalFileName = mainImage.OriginalFileName,
            ContentType = mainImage.ContentType,
            LengthBytes = mainImage.LengthBytes,
            Sha256 = mainImage.Sha256,
            AltText = mainImage.AltText,
            CreatedAt = mainImage.CreatedAt
        });
    }

    [Fact]
    public void Apply_ReplacesMainImageWithoutChangingVariantPhotos_GivenProductMainImageSetEvent()
    {
        var projection = new ProductDetailsProjection();
        var productId = Guid.CreateVersion7();
        var variantId = Guid.CreateVersion7();
        var variantPhotoId = Guid.CreateVersion7();
        var readModel = CreateReadModel(productId, variantId);
        readModel.MainImage = ToReadModel(CreateMainImage(productId, "main-a.png"));
        readModel.Variants.Single().Photos.Add(new ProductMediaAssetReadModel
        {
            Id = variantPhotoId,
            ProductId = productId,
            VariantId = variantId,
            Role = ProductMediaRole.VariantPhoto.ToString(),
            Url = "/media/products/variant.png"
        });
        var replacement = CreateMainImage(productId, "main-b.png");

        projection.Apply(new ProductMainImageSet { ProductId = productId, MediaAsset = replacement }, readModel);

        readModel.MainImage!.Id.Should().Be(replacement.Id);
        readModel.Variants.Single().Photos.Should().ContainSingle(photo => photo.Id == variantPhotoId);
    }

    [Fact]
    public void Apply_RemovesMainImageWithoutChangingVariantPhotos_GivenProductMainImageRemovedEvent()
    {
        var projection = new ProductDetailsProjection();
        var productId = Guid.CreateVersion7();
        var variantId = Guid.CreateVersion7();
        var mainImage = CreateMainImage(productId, "main-a.png");
        var variantPhotoId = Guid.CreateVersion7();
        var readModel = CreateReadModel(productId, variantId);
        readModel.MainImage = ToReadModel(mainImage);
        readModel.Variants.Single().Photos.Add(new ProductMediaAssetReadModel { Id = variantPhotoId });

        projection.Apply(new ProductMainImageRemoved { ProductId = productId, MediaAssetId = mainImage.Id }, readModel);

        readModel.MainImage.Should().BeNull();
        readModel.Variants.Single().Photos.Should().ContainSingle(photo => photo.Id == variantPhotoId);
    }

    private static ProductDetailsReadModel CreateReadModel(Guid productId, Guid variantId) => new()
    {
        Id = productId,
        Name = "Cookie",
        Description = "Desc",
        Price = 2.50m,
        IsActive = true,
        Variants = [new ProductVariantReadModel { Id = variantId, Name = "Single", Price = 2.50m }]
    };

    private static ProductMediaAssetReadModel ToReadModel(ProductMediaAsset asset) => new()
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

    private static ProductMediaAsset CreateMainImage(Guid productId, string fileName) => new(
        Guid.CreateVersion7(),
        productId,
        null,
        ProductMediaRole.MainImage,
        $"storage/{fileName}",
        $"/media/products/{fileName}",
        fileName,
        "image/png",
        8,
        "hash",
        "Main image",
        DateTimeOffset.UtcNow);
}
