using CookieCommerce.ProductDomain.Domain.Aggregates;
using CookieCommerce.ProductDomain.Domain.Exceptions;
using CookieCommerce.ProductDomain.Domain.ValueObjects;
using FluentAssertions;

namespace CookieCommerce.ProductDomain.Tests.DomainTests;

public class ProductAggregate_VariantPhotos_Tests
{
    [Fact]
    public void AddVariantPhoto_AddsPhoto_GivenPhotoBelongsToVariant()
    {
        var productId = Guid.CreateVersion7();
        var variantId = Guid.CreateVersion7();
        var product = CreateProductWithVariant(productId, variantId);
        var photo = CreatePhoto(productId, variantId);

        product.AddVariantPhoto(variantId, photo);

        product.Variants.Single().Photos.Should().ContainSingle()
            .Which.Should().Be(photo);
    }

    [Fact]
    public void RemoveVariantPhoto_RemovesPhoto_GivenExistingPhoto()
    {
        var productId = Guid.CreateVersion7();
        var variantId = Guid.CreateVersion7();
        var product = CreateProductWithVariant(productId, variantId);
        var photo = CreatePhoto(productId, variantId);
        product.AddVariantPhoto(variantId, photo);

        product.RemoveVariantPhoto(variantId, photo.Id);

        product.Variants.Single().Photos.Should().BeEmpty();
    }

    [Fact]
    public void AddVariantPhoto_ThrowsInvalidProductMediaOwnershipException_GivenPhotoForDifferentVariant()
    {
        var productId = Guid.CreateVersion7();
        var variantId = Guid.CreateVersion7();
        var otherVariantId = Guid.CreateVersion7();
        var product = CreateProductWithVariant(productId, variantId);
        var photo = CreatePhoto(productId, otherVariantId);

        Action act = () => product.AddVariantPhoto(variantId, photo);

        act.Should().Throw<InvalidProductMediaOwnershipException>()
            .Which.Reason.Should().Be("asset variant scope does not match the requested target");
    }

    [Fact]
    public void AddVariantPhoto_ThrowsInvalidProductMediaOwnershipException_GivenUnknownVariant()
    {
        var productId = Guid.CreateVersion7();
        var variantId = Guid.CreateVersion7();
        var unknownVariantId = Guid.CreateVersion7();
        var product = CreateProductWithVariant(productId, variantId);
        var photo = CreatePhoto(productId, unknownVariantId);

        Action act = () => product.AddVariantPhoto(unknownVariantId, photo);

        act.Should().Throw<InvalidProductMediaOwnershipException>();
    }

    private static ProductAggregate CreateProductWithVariant(Guid productId, Guid variantId)
    {
        var product = new ProductAggregate();
        product.Create(productId, "Cookie", "Cookie description", 2.50m);
        product.AddVariant(variantId, "Single", 2.50m);
        return product;
    }

    private static ProductMediaAsset CreatePhoto(Guid productId, Guid variantId) => new(
        Guid.CreateVersion7(),
        productId,
        variantId,
        ProductMediaRole.VariantPhoto,
        "storage-key",
        "/media/products/photo.png",
        "photo.png",
        "image/png",
        8,
        "hash",
        "Photo",
        DateTimeOffset.UtcNow);
}
