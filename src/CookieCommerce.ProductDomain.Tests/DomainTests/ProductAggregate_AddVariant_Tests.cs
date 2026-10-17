using CookieCommerce.ProductDomain.Domain.Aggregates;
using CookieCommerce.ProductDomain.Domain.Exceptions;
using FluentAssertions;

namespace CookieCommerce.ProductDomain.Tests.DomainTests;

public class ProductAggregate_AddVariant_Tests
{
    [Fact]
    public void AddVariant_AddsMultipleVariants_GivenDistinctNames()
    {
        var product = new ProductAggregate();
        product.Create(Guid.CreateVersion7(), "Cookie", "Cookie description", 2.50m);

        product.AddVariant(Guid.CreateVersion7(), "Single", 2.50m);
        product.AddVariant(Guid.CreateVersion7(), "Box", 10.00m);

        product.Variants.Should().HaveCount(2);
        product.Variants.Select(variant => variant.Name).Should().Contain(["Single", "Box"]);
    }

    [Fact]
    public void AddVariant_ThrowsDuplicateProductVariantException_GivenDuplicateNameWithDifferentCasing()
    {
        var product = new ProductAggregate();
        product.Create(Guid.CreateVersion7(), "Cookie", "Cookie description", 2.50m);
        product.AddVariant(Guid.CreateVersion7(), "Single", 2.50m);

        Action act = () => product.AddVariant(Guid.CreateVersion7(), " single ", 3.00m);

        act.Should().Throw<DuplicateProductVariantException>()
            .Which.VariantName.Should().Be(" single ");
    }

    [Fact]
    public void UpdateVariant_ThrowsDuplicateProductVariantException_GivenExistingVariantName()
    {
        var product = new ProductAggregate();
        product.Create(Guid.CreateVersion7(), "Cookie", "Cookie description", 2.50m);
        var singleId = Guid.CreateVersion7();
        var boxId = Guid.CreateVersion7();
        product.AddVariant(singleId, "Single", 2.50m);
        product.AddVariant(boxId, "Box", 10.00m);

        Action act = () => product.UpdateVariant(boxId, "Single", 12.00m);

        act.Should().Throw<DuplicateProductVariantException>();
    }

    [Fact]
    public void RemoveVariant_RemovesVariant_GivenExistingVariantId()
    {
        var product = new ProductAggregate();
        product.Create(Guid.CreateVersion7(), "Cookie", "Cookie description", 2.50m);
        var variantId = Guid.CreateVersion7();
        product.AddVariant(variantId, "Single", 2.50m);

        product.RemoveVariant(variantId);

        product.Variants.Should().BeEmpty();
    }
}
