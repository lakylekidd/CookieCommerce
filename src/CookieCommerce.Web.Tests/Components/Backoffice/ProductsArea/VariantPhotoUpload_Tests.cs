using CookieCommerce.Web.Models.Products;
using CookieCommerce.Web.Services;
using CookieCommerce.Web.Services.Products;
using FluentAssertions;

namespace CookieCommerce.Web.Tests.Components.Backoffice.ProductsArea;

public sealed class VariantPhotoUpload_Tests
{
    [Fact]
    public void SelectedInventoryVariant_RendersVariantPhotoUploadControls_GivenExpandedVariant()
    {
        var source = ProductsAreaSource.ReadComponent("SelectedInventoryVariant.razor");

        source.Should().Contain("Variant photos");
        source.Should().Contain("Accepted file types: JPEG, PNG, or WebP. Maximum size: 5 MB.");
        source.Should().Contain("InputFile");
        source.Should().Contain("UploadPhotoAsync");
        source.Should().Contain("BackofficeService.UploadVariantPhotoAsync");
        source.Should().Contain("BackofficeService.RemoveVariantPhotoAsync");
    }

    [Fact]
    public void SelectedInventoryVariant_PreservesState_GivenUploadValidationFailure()
    {
        var source = ProductsAreaSource.ReadComponent("SelectedInventoryVariant.razor");

        source.Should().Contain("photoValidationMessage");
        source.Should().Contain("The uploaded image exceeds the maximum size of 5 MB.");
        source.Should().Contain("The uploaded image content type is not supported.");
        source.Should().Contain("IsAcceptedImageType");
    }

    [Fact]
    public void SelectedInventoryVariantCss_ContainsResponsiveAccessibleMediaStyles_GivenVariantPhotoControls()
    {
        var source = ProductsAreaSource.ReadComponent("SelectedInventoryVariant.razor.css");

        source.Should().Contain(".variant-media-grid");
        source.Should().Contain("@media (max-width: 640px)");
        source.Should().Contain(":focus-visible");
    }

    [Fact]
    public async Task FakeBackofficeService_UploadVariantPhotoScopesPreview_GivenValidImage()
    {
        var service = new FakeBackofficeService();
        var productId = Guid.CreateVersion7();
        var variantId = Guid.CreateVersion7();
        var otherVariantId = Guid.CreateVersion7();
        var product = new ProductDto
        {
            ProductId = productId,
            ProductName = "Test product",
            Variants =
            [
                new ProductVariantDto { Id = variantId, Name = "Single", Price = 2.50m },
                new ProductVariantDto { Id = otherVariantId, Name = "Box", Price = 10.00m }
            ]
        };
        StaticInventoryList.InventoryItems.Add(product);

        var image = await service.UploadVariantPhotoAsync(
            productId,
            variantId,
            new ProductMediaUploadDto(new MemoryStream([1, 2, 3]), "photo.png", "image/png", 3, "Single photo"));

        product.Variants.Single(variant => variant.Id == variantId).Images.Should().ContainSingle(photo => photo.Id == image.Id);
        product.Variants.Single(variant => variant.Id == otherVariantId).Images.Should().BeEmpty();
    }

    [Fact]
    public async Task FakeBackofficeService_UploadVariantPhotoThrows_GivenUnsupportedFile()
    {
        var service = new FakeBackofficeService();

        Func<Task> act = () => service.UploadVariantPhotoAsync(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            new ProductMediaUploadDto(new MemoryStream([1]), "not-image.txt", "text/plain", 1));

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("The uploaded image content type is not supported.");
    }
}
