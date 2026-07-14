using CookieCommerce.Web.Models.Products;
using CookieCommerce.Web.Services;
using CookieCommerce.Web.Services.Products;
using CookieCommerce.Web.Tests.Helpers;
using FluentAssertions;

namespace CookieCommerce.Web.Tests.Components.Backoffice.ProductsArea;

public sealed class ProductMainImage_Tests
{
    [Fact]
    public void SelectedInventoryItem_RendersProductNameAndMainImageControls_GivenProductIsSelected()
    {
        var source = ProductsAreaSource.ReadComponent("SelectedInventoryItem.razor");

        source.Should().Contain("Selected Product");
        source.Should().Contain("Product name");
        source.Should().Contain("SaveProductNameAsync");
        source.Should().Contain("Main image");
        source.Should().Contain("Accepted file types: JPEG, PNG, or WebP. Maximum size: 5 MB.");
    }

    [Fact]
    public void SelectedInventoryItem_UsesTypedServiceForMainImageOperations_GivenMainImageControls()
    {
        var source = ProductsAreaSource.ReadComponent("SelectedInventoryItem.razor");

        source.Should().Contain("InputFile");
        source.Should().Contain("UploadMainImageAsync");
        source.Should().Contain("RemoveMainImageAsync");
        source.Should().Contain("BackofficeService.UploadProductMainImageAsync");
        source.Should().Contain("BackofficeService.RemoveProductMainImageAsync");
        source.Should().Contain("BackofficeService.UpdateProductNameAsync");
    }

    [Fact]
    public void SelectedInventoryItem_PreservesVariantState_GivenMainImageValidationFailure()
    {
        var source = ProductsAreaSource.ReadComponent("SelectedInventoryItem.razor");

        source.Should().Contain("_mainImageValidationMessage");
        source.Should().Contain("The uploaded image exceeds the maximum size of 5 MB.");
        source.Should().Contain("The uploaded image content type is not supported.");
        source.Should().Contain("ValidateMainImage");
        source.Should().Contain("_variantRows");
    }

    [Fact]
    public void SelectedInventoryItemCss_ContainsResponsiveAccessibleMainImageStyles_GivenMainImageControls()
    {
        var source = ProductsAreaSource.ReadComponent("SelectedInventoryItem.razor.css");

        source.Should().Contain(".main-image-card");
        source.Should().Contain("@media (max-width: 640px)");
        source.Should().Contain(":focus-visible");
    }

    [Fact]
    public async Task FakeBackofficeService_UploadProductMainImageReplacesExistingImage_GivenValidImage()
    {
        var service = new FakeBackofficeService(TestEnvironmentFactory.CreateTempWebHostEnvironment());
        var productId = Guid.CreateVersion7();
        var product = new ProductDto
        {
            ProductId = productId,
            ProductName = "Test product",
            ProductMainImage = new ProductImageDto { Id = Guid.CreateVersion7(), Role = "MainImage", Url = "/old.png" }
        };
        StaticInventoryList.InventoryItems.Add(product);

        var image = await service.UploadProductMainImageAsync(
            productId,
            new ProductMediaUploadDto(new MemoryStream([1, 2, 3]), "main.png", "image/png", 3, "Main image"));

        product.ProductMainImage.Should().Be(image);
        product.ProductMainImage!.OriginalFileName.Should().Be("main.png");
    }

    [Fact]
    public async Task FakeBackofficeService_RemoveProductMainImageRemovesOnlyMainImage_GivenVariantPhotosExist()
    {
        var service = new FakeBackofficeService(TestEnvironmentFactory.CreateTempWebHostEnvironment());
        var productId = Guid.CreateVersion7();
        var mainImageId = Guid.CreateVersion7();
        var variantPhoto = new ProductImageDto { Id = Guid.CreateVersion7(), Role = "VariantPhoto", Url = "/variant.png" };
        var product = new ProductDto
        {
            ProductId = productId,
            ProductName = "Test product",
            ProductMainImage = new ProductImageDto { Id = mainImageId, Role = "MainImage", Url = "/main.png" },
            Variants = [new ProductVariantDto { Id = Guid.CreateVersion7(), Name = "Single", Images = [variantPhoto] }]
        };
        StaticInventoryList.InventoryItems.Add(product);

        await service.RemoveProductMainImageAsync(productId, mainImageId);

        product.ProductMainImage.Should().BeNull();
        product.Variants.Single().Images.Should().ContainSingle(photo => photo.Id == variantPhoto.Id);
    }
}
