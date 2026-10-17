using System.Net;
using System.Net.Http.Json;
using CookieCommerce.ProductDomain.Api.Models.Products;
using FluentAssertions;

namespace CookieCommerce.ProductDomain.Tests.IntegrationTests;

public class ProductController_VariantPhotos_Tests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public ProductController_VariantPhotos_Tests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task UploadVariantPhoto_ReturnsCreatedAndScopesPhotoToVariant_GivenValidImage()
    {
        var client = _factory.CreateClient();
        var (productId, variantId) = await CreateProductWithVariantAsync(client);
        using var content = CreateMultipartImageContent("Variant photo");

        var response = await client.PostAsync($"/api/product/{productId}/variants/{variantId}/photos", content);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var media = await response.Content.ReadFromJsonAsync<ProductMediaAssetResponse>();
        media.Should().NotBeNull();
        media!.ProductId.Should().Be(productId);
        media.VariantId.Should().Be(variantId);
        media.Role.Should().Be("VariantPhoto");

        var product = await client.GetFromJsonAsync<ProductDetailsResponse>($"/api/product/{productId}");
        product!.Variants.Single(variant => variant.Id == variantId).Photos.Should().ContainSingle(photo => photo.Id == media.Id);
    }

    [Fact]
    public async Task UploadVariantPhoto_ReturnsBadRequest_GivenUnsupportedImage()
    {
        var client = _factory.CreateClient();
        var (productId, variantId) = await CreateProductWithVariantAsync(client);
        using var content = new MultipartFormDataContent();
        var file = new ByteArrayContent([1, 2, 3]);
        file.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/plain");
        content.Add(file, "file", "not-image.txt");

        var response = await client.PostAsync($"/api/product/{productId}/variants/{variantId}/photos", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RemoveVariantPhoto_ReturnsNoContentAndRemovesPhoto_GivenExistingPhoto()
    {
        var client = _factory.CreateClient();
        var (productId, variantId) = await CreateProductWithVariantAsync(client);
        using var content = CreateMultipartImageContent("Variant photo");
        var upload = await client.PostAsync($"/api/product/{productId}/variants/{variantId}/photos", content);
        upload.EnsureSuccessStatusCode();
        var media = await upload.Content.ReadFromJsonAsync<ProductMediaAssetResponse>();

        var response = await client.DeleteAsync($"/api/product/{productId}/variants/{variantId}/photos/{media!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var product = await client.GetFromJsonAsync<ProductDetailsResponse>($"/api/product/{productId}");
        product!.Variants.Single(variant => variant.Id == variantId).Photos.Should().BeEmpty();
    }

    private static MultipartFormDataContent CreateMultipartImageContent(string altText)
    {
        var content = new MultipartFormDataContent();
        var file = new ByteArrayContent([0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]);
        file.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
        content.Add(file, "file", "photo.png");
        content.Add(new StringContent(altText), "altText");
        return content;
    }

    private static async Task<(Guid ProductId, Guid VariantId)> CreateProductWithVariantAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/product", new CreateProductRequest
        {
            Name = $"Photo Product {Guid.CreateVersion7():N}",
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
