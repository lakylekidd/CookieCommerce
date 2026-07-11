using CookieCommerce.Web.Models.Products;
using CookieCommerce.Web.Services;
using FluentAssertions;

namespace CookieCommerce.Web.Tests.Components.Backoffice.ProductsArea;

public sealed class ProductVariants_Tests
{
    [Fact]
    public void SelectedInventoryItem_RendersVariantEditor_GivenProductIsSelected()
    {
        var source = ProductsAreaSource.ReadComponent("SelectedInventoryItem.razor");

        source.Should().Contain("Product Variants");
        source.Should().Contain("AddVariantRow");
        source.Should().Contain("SaveVariantRows");
        source.Should().Contain("BackofficeService.UpsertProductVariantsAsync");
    }

    [Fact]
    public void SelectedInventoryItem_ValidatesRows_GivenMissingOrDuplicateVariants()
    {
        var source = ProductsAreaSource.ReadComponent("SelectedInventoryItem.razor");

        source.Should().Contain("Variant name is required.");
        source.Should().Contain("Variant price must be greater than 0.");
        source.Should().Contain("Variant name must be unique.");
        source.Should().Contain("StringComparison.OrdinalIgnoreCase");
    }

    [Fact]
    public void CreateNewProductModal_PreservesProductFields_GivenVariantValidationFailure()
    {
        var source = ProductsAreaSource.ReadComponent(Path.Combine("CreateProductModalArea", "CreateNewProductModal.razor"));

        source.Should().Contain("if (!ValidateVariants())");
        source.Should().Contain("return;");
        source.Should().Contain("Every variant needs a name.");
        source.Should().Contain("Variant names must be unique.");
    }

    [Fact]
    public async Task FakeBackofficeService_UpsertProductVariantsPersistsDistinctRows_GivenValidVariants()
    {
        var service = new FakeBackofficeService();
        var productId = Guid.CreateVersion7();
        var product = new ProductDto
        {
            ProductId = productId,
            ProductName = "Test product",
            Variants = [new ProductVariantDto { Id = Guid.CreateVersion7(), Name = "Old", Price = 1m }]
        };
        CookieCommerce.Web.Services.Products.StaticInventoryList.InventoryItems.Add(product);

        await service.UpsertProductVariantsAsync(productId,
        [
            new UpsertProductVariantDto(null, "Single", 2.50m),
            new UpsertProductVariantDto(null, "Box", 10.00m)
        ]);

        product.Variants.Select(variant => variant.Name).Should().Contain(["Single", "Box"]);
        product.Variants.Should().HaveCount(2);
    }
}
