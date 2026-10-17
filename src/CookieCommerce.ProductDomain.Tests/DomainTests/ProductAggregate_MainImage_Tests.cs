using CookieCommerce.ProductDomain.Domain.Aggregates;
using CookieCommerce.ProductDomain.Domain.Exceptions;
using CookieCommerce.ProductDomain.Domain.ValueObjects;
using FluentAssertions;

namespace CookieCommerce.ProductDomain.Tests.DomainTests;

public class ProductAggregate_MainImage_Tests
{
    [Fact]
    public void SetMainImage_SetsMainImage_GivenAssetBelongsToProduct()
    {
        var productId = Guid.CreateVersion7();
        var product = CreateProduct(productId);
        var mainImage = CreateMainImage(productId, "main-a.png");

        product.SetMainImage(mainImage);

        product.MainImage.Should().Be(mainImage);
    }

    [Fact]
    public void SetMainImage_ReplacesMainImage_GivenExistingMainImage()
    {
        var productId = Guid.CreateVersion7();
        var product = CreateProduct(productId);
        var original = CreateMainImage(productId, "main-a.png");
        var replacement = CreateMainImage(productId, "main-b.png");
        product.SetMainImage(original);

        product.SetMainImage(replacement);

        product.MainImage.Should().Be(replacement);
    }

    [Fact]
    public void RemoveMainImage_RemovesMainImage_GivenMatchingAssetId()
    {
        var productId = Guid.CreateVersion7();
        var product = CreateProduct(productId);
        var mainImage = CreateMainImage(productId, "main-a.png");
        product.SetMainImage(mainImage);

        product.RemoveMainImage(mainImage.Id);

        product.MainImage.Should().BeNull();
    }

    [Fact]
    public void SetMainImage_DoesNotAlterVariantPhotos_GivenVariantPhotosExist()
    {
        var productId = Guid.CreateVersion7();
        var variantId = Guid.CreateVersion7();
        var product = CreateProduct(productId);
        product.AddVariant(variantId, "Single", 2.50m);
        var photo = CreateVariantPhoto(productId, variantId);
        product.AddVariantPhoto(variantId, photo);

        product.SetMainImage(CreateMainImage(productId, "main-a.png"));

        product.Variants.Single().Photos.Should().ContainSingle()
            .Which.Should().Be(photo);
    }

    [Fact]
    public void SetMainImage_ThrowsInvalidProductMediaOwnershipException_GivenVariantPhotoAsset()
    {
        var productId = Guid.CreateVersion7();
        var variantId = Guid.CreateVersion7();
        var product = CreateProduct(productId);
        product.AddVariant(variantId, "Single", 2.50m);
        var variantPhoto = CreateVariantPhoto(productId, variantId);

        Action act = () => product.SetMainImage(variantPhoto);

        act.Should().Throw<InvalidProductMediaOwnershipException>()
            .Which.Reason.Should().Be("asset role must be MainImage");
    }

    private static ProductAggregate CreateProduct(Guid productId)
    {
        var product = new ProductAggregate();
        product.Create(productId, "Cookie", "Cookie description", 2.50m);
        return product;
    }

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

    private static ProductMediaAsset CreateVariantPhoto(Guid productId, Guid variantId) => new(
        Guid.CreateVersion7(),
        productId,
        variantId,
        ProductMediaRole.VariantPhoto,
        "storage/photo.png",
        "/media/products/photo.png",
        "photo.png",
        "image/png",
        8,
        "hash",
        "Variant photo",
        DateTimeOffset.UtcNow);
}
