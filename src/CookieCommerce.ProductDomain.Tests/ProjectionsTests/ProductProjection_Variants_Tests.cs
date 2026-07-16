using CookieCommerce.ProductDomain.Application.Products.ReadModels;
using CookieCommerce.ProductDomain.Domain.DomainEvents;
using CookieCommerce.ProductDomain.Infrastructure.Projections;
using FluentAssertions;

namespace CookieCommerce.ProductDomain.Tests.ProjectionsTests;

public class ProductProjection_Variants_Tests
{
    [Fact]
    public void Apply_AddsVariant_GivenProductVariantAddedEvent()
    {
        var projection = new ProductDetailsProjection();
        var readModel = new ProductDetailsReadModel { Id = Guid.CreateVersion7(), Name = "Cookie", Description = "Desc", Price = 2.50m, IsActive = true };
        var variantId = Guid.CreateVersion7();

        projection.Apply(new ProductVariantAdded
        {
            ProductId = readModel.Id,
            VariantId = variantId,
            Name = "Single",
            Price = 2.50m,
            CurrencySymbol = "€",
            IsActive = true
        }, readModel);

        readModel.Variants.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(new ProductVariantReadModel
            {
                Id = variantId,
                Name = "Single",
                Price = 2.50m,
                CurrencySymbol = "€",
                IsActive = true
            });
    }

    [Fact]
    public void Apply_UpdatesVariant_GivenProductVariantUpdatedEvent()
    {
        var projection = new ProductDetailsProjection();
        var variantId = Guid.CreateVersion7();
        var readModel = new ProductDetailsReadModel
        {
            Id = Guid.CreateVersion7(),
            Name = "Cookie",
            Description = "Desc",
            Price = 2.50m,
            IsActive = true,
            Variants = [new ProductVariantReadModel { Id = variantId, Name = "Single", Price = 2.50m }]
        };

        projection.Apply(new ProductVariantUpdated
        {
            ProductId = readModel.Id,
            VariantId = variantId,
            Name = "Box",
            Price = 10.00m,
            CurrencySymbol = "€",
            IsActive = false,
            Sku = "BOX"
        }, readModel);

        readModel.Variants.Should().ContainSingle();
        readModel.Variants[0].Name.Should().Be("Box");
        readModel.Variants[0].Price.Should().Be(10.00m);
        readModel.Variants[0].IsActive.Should().BeFalse();
        readModel.Variants[0].Sku.Should().Be("BOX");
    }

    [Fact]
    public void Apply_RemovesVariant_GivenProductVariantRemovedEvent()
    {
        var projection = new ProductDetailsProjection();
        var variantId = Guid.CreateVersion7();
        var readModel = new ProductDetailsReadModel
        {
            Id = Guid.CreateVersion7(),
            Name = "Cookie",
            Description = "Desc",
            Price = 2.50m,
            IsActive = true,
            Variants = [new ProductVariantReadModel { Id = variantId, Name = "Single", Price = 2.50m }]
        };

        projection.Apply(new ProductVariantRemoved
        {
            ProductId = readModel.Id,
            VariantId = variantId
        }, readModel);

        readModel.Variants.Should().BeEmpty();
    }
}
