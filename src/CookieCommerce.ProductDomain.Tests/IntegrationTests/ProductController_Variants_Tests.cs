using System.Net;
using System.Net.Http.Json;
using CookieCommerce.ProductDomain.Api.Models.Products;
using FluentAssertions;

namespace CookieCommerce.ProductDomain.Tests.IntegrationTests;

public class ProductController_Variants_Tests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public ProductController_Variants_Tests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateProduct_ReturnsCreatedAndPersistsVariants_GivenInlineVariantRequests()
    {
        var client = _factory.CreateClient();
        var request = new CreateProductRequest
        {
            Name = "Variant Product",
            Description = "Product with variants",
            Price = 2.50m,
            Variants =
            [
                new ProductVariantRequest { Name = "Single", Price = 2.50m },
                new ProductVariantRequest { Name = "Box", Price = 10.00m }
            ]
        };

        var response = await client.PostAsJsonAsync("/api/product", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var productId = response.Headers.Location!.ToString().Split('/').Last();
        var product = await client.GetFromJsonAsync<ProductDetailsResponse>($"/api/product/{productId}");
        product.Should().NotBeNull();
        product!.Variants.Select(variant => variant.Name).Should().Contain(["Single", "Box"]);
    }

    [Fact]
    public async Task UpsertVariants_ReturnsNoContentAndPersistsVariants_GivenValidRows()
    {
        var client = _factory.CreateClient();
        var productId = await CreateProductAsync(client);
        var request = new List<ProductVariantRequest>
        {
            new() { Name = "Single", Price = 2.50m },
            new() { Name = "Box", Price = 10.00m }
        };

        var response = await client.PutAsJsonAsync($"/api/product/{productId}/variants", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var product = await client.GetFromJsonAsync<ProductDetailsResponse>($"/api/product/{productId}");
        product!.Variants.Should().HaveCount(2);
    }

    [Fact]
    public async Task UpsertVariants_ReturnsBadRequest_GivenDuplicateVariantNames()
    {
        var client = _factory.CreateClient();
        var productId = await CreateProductAsync(client);
        var request = new List<ProductVariantRequest>
        {
            new() { Name = "Single", Price = 2.50m },
            new() { Name = " single ", Price = 3.00m }
        };

        var response = await client.PutAsJsonAsync($"/api/product/{productId}/variants", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private static async Task<Guid> CreateProductAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/product", new CreateProductRequest
        {
            Name = "Product",
            Description = "Description",
            Price = 2.50m
        });
        response.EnsureSuccessStatusCode();
        return Guid.Parse(response.Headers.Location!.ToString().Split('/').Last());
    }
}
