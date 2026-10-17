using CookieCommerce.ProductDomain.Application.Products.ReadModels;
using CookieCommerce.ProductDomain.Domain.DomainEvents;
using CookieCommerce.ProductDomain.Domain.ValueObjects;
using CookieCommerce.ProductDomain.Infrastructure.Projections;
using FluentAssertions;

namespace CookieCommerce.ProductDomain.Tests.ProjectionsTests;

public class ProductProjection_VariantPhotos_Tests
{
    [Fact]
    public void Apply_AddsVariantPhoto_GivenVariantPhotoAddedEvent()
    {
        var projection = new ProductDetailsProjection();
        var productId = Guid.CreateVersion7();
        var variantId = Guid.CreateVersion7();
        var readModel = CreateReadModel(productId, variantId);
        var photo = CreatePhoto(productId, variantId);

        projection.Apply(new VariantPhotoAdded { ProductId = productId, VariantId = variantId, MediaAsset = photo }, readModel);

        readModel.Variants.Single().Photos.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(new ProductMediaAssetReadModel
            {
                Id = photo.Id,
                ProductId = productId,
                VariantId = variantId,
                Role = ProductMediaRole.VariantPhoto.ToString(),
                Url = photo.Url,
                OriginalFileName = photo.OriginalFileName,
                ContentType = photo.ContentType,
                LengthBytes = photo.LengthBytes,
                Sha256 = photo.Sha256,
                AltText = photo.AltText,
                CreatedAt = photo.CreatedAt
            });
    }

    [Fact]
    public void Apply_RemovesVariantPhoto_GivenVariantPhotoRemovedEvent()
    {
        var projection = new ProductDetailsProjection();
        var productId = Guid.CreateVersion7();
        var variantId = Guid.CreateVersion7();
        var photo = CreatePhoto(productId, variantId);
        var readModel = CreateReadModel(productId, variantId);
        readModel.Variants.Single().Photos.Add(new ProductMediaAssetReadModel { Id = photo.Id });

        projection.Apply(new VariantPhotoRemoved { ProductId = productId, VariantId = variantId, MediaAssetId = photo.Id }, readModel);

        readModel.Variants.Single().Photos.Should().BeEmpty();
    }

    [Fact]
    public void Apply_DoesNotAddPhotoToDifferentVariant_GivenMismatchedVariantId()
    {
        var projection = new ProductDetailsProjection();
        var productId = Guid.CreateVersion7();
        var variantId = Guid.CreateVersion7();
        var readModel = CreateReadModel(productId, variantId);
        var photo = CreatePhoto(productId, Guid.CreateVersion7());

        projection.Apply(new VariantPhotoAdded { ProductId = productId, VariantId = photo.VariantId!.Value, MediaAsset = photo }, readModel);

        readModel.Variants.Single().Photos.Should().BeEmpty();
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
