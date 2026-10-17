using System.Net;
using System.Net.Http.Json;
using CookieCommerce.ProductDomain.Api.Models.Products;
using FluentAssertions;

namespace CookieCommerce.ProductDomain.Tests.IntegrationTests;

public class ProductController_MainImage_Tests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public ProductController_MainImage_Tests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task UploadMainImage_ReturnsCreatedAndSetsSingleMainImage_GivenValidImage()
    {
        var client = _factory.CreateClient();
        var productId = await CreateProductAsync(client);
        using var content = CreateMultipartImageContent("Main cookie image", "main-a.png");

        var response = await client.PostAsync($"/api/product/{productId}/media/main-image", content);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var media = await response.Content.ReadFromJsonAsync<ProductMediaAssetResponse>();
        media.Should().NotBeNull();
        media!.ProductId.Should().Be(productId);
        media.VariantId.Should().BeNull();
        media.Role.Should().Be("MainImage");

        var product = await client.GetFromJsonAsync<ProductDetailsResponse>($"/api/product/{productId}");
        product!.MainImage!.Id.Should().Be(media.Id);
    }

    [Fact]
    public async Task UploadMainImage_ReplacesExistingMainImageWithoutChangingVariantPhotos_GivenSecondValidImage()
    {
        var client = _factory.CreateClient();
        var (productId, variantId) = await CreateProductWithVariantAsync(client);
        ProductMediaAssetResponse variantPhoto;
        using (var variantPhotoContent = CreateMultipartImageContent("Variant photo", "variant.png"))
        {
            var variantUpload = await client.PostAsync($"/api/product/{productId}/variants/{variantId}/photos", variantPhotoContent);
            variantUpload.EnsureSuccessStatusCode();
            variantPhoto = (await variantUpload.Content.ReadFromJsonAsync<ProductMediaAssetResponse>())!;
        }

        using (var firstMainImage = CreateMultipartImageContent("Main A", "main-a.png"))
        {
            var upload = await client.PostAsync($"/api/product/{productId}/media/main-image", firstMainImage);
            upload.EnsureSuccessStatusCode();
        }

        ProductMediaAssetResponse replacement;
        using (var secondMainImage = CreateMultipartImageContent("Main B", "main-b.png"))
        {
            var upload = await client.PostAsync($"/api/product/{productId}/media/main-image", secondMainImage);
            upload.EnsureSuccessStatusCode();
            replacement = (await upload.Content.ReadFromJsonAsync<ProductMediaAssetResponse>())!;
        }

        var product = await client.GetFromJsonAsync<ProductDetailsResponse>($"/api/product/{productId}");
        product!.MainImage!.Id.Should().Be(replacement.Id);
        product.Variants.Single(variant => variant.Id == variantId).Photos.Should().ContainSingle(photo => photo.Id == variantPhoto.Id);
    }

    [Fact]
    public async Task RemoveMainImage_ReturnsNoContentAndRemovesMainImage_GivenExistingMainImage()
    {
        var client = _factory.CreateClient();
        var productId = await CreateProductAsync(client);
        using var content = CreateMultipartImageContent("Main cookie image", "main-a.png");
        var upload = await client.PostAsync($"/api/product/{productId}/media/main-image", content);
        upload.EnsureSuccessStatusCode();
        var media = await upload.Content.ReadFromJsonAsync<ProductMediaAssetResponse>();

        var response = await client.DeleteAsync($"/api/product/{productId}/media/main-image/{media!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var product = await client.GetFromJsonAsync<ProductDetailsResponse>($"/api/product/{productId}");
        product!.MainImage.Should().BeNull();
    }

    [Fact]
    public async Task UploadMainImage_ReturnsBadRequest_GivenUnsupportedImage()
    {
        var client = _factory.CreateClient();
        var productId = await CreateProductAsync(client);
        using var content = new MultipartFormDataContent();
        var file = new ByteArrayContent([1, 2, 3]);
        file.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/plain");
        content.Add(file, "file", "not-image.txt");

        var response = await client.PostAsync($"/api/product/{productId}/media/main-image", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private static MultipartFormDataContent CreateMultipartImageContent(string altText, string fileName)
    {
        var content = new MultipartFormDataContent();
        var file = new ByteArrayContent([0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]);
        file.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
        content.Add(file, "file", fileName);
        content.Add(new StringContent(altText), "altText");
        return content;
    }

    private static async Task<Guid> CreateProductAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/product", new CreateProductRequest
        {
            Name = $"Main Image Product {Guid.CreateVersion7():N}",
            Description = "Description",
            Price = 2.50m
        });
        response.EnsureSuccessStatusCode();
        return Guid.Parse(response.Headers.Location!.ToString().Split('/').Last());
    }

    private static async Task<(Guid ProductId, Guid VariantId)> CreateProductWithVariantAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/product", new CreateProductRequest
        {
            Name = $"Main Image Product {Guid.CreateVersion7():N}",
            Description = "Description",
            Price = 2.50m,
            Variants = [new ProductVariantRequest { Name = "Single", Price = 2.50m }]
        });
        response.EnsureSuccessStatusCode();
        var productId = Guid.Parse(response.Headers.Location!.ToString().Split('/').Last());
        var product = await client.GetFromJsonAsync<ProductDetailsResponse>($"/api/product/{productId}");
        return (productId, product!.Variants.Single().Id);
    }
}
