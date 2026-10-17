using System.Net;
using System.Net.Http.Json;
using CookieCommerce.ProductDomain.Api.Models.Products;
using CookieCommerce.ProductDomain.Application.Products.ReadModels;
using FluentAssertions;

namespace CookieCommerce.ProductDomain.Tests.IntegrationTests;

public class ProductControllerIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public ProductControllerIntegrationTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateProduct_ValidRequest_ReturnsCreatedWithId()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new CreateProductRequest
        {
            Name = "Test Product",
            Description = "A test product description",
            Price = 99.99m
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/product", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        var location = response.Headers.Location!.ToString().ToLowerInvariant();
        location.Should().Contain("/api/product/");
    }

    [Fact]
    public async Task CreateProduct_MissingRequiredFields_ReturnsBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new CreateProductRequest
        {
            Name = "", // Required field missing
            Description = "Description",
            Price = 99.99m
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/product", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateProduct_InvalidPrice_ReturnsBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new CreateProductRequest
        {
            Name = "Test Product",
            Description = "Description",
            Price = 0 // Invalid price
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/product", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetProductById_ExistingProduct_ReturnsProduct()
    {
        // Arrange - create a product first
        var client = _factory.CreateClient();
        var createRequest = new CreateProductRequest
        {
            Name = "Get Test Product",
            Description = "Product for get test",
            Price = 49.99m
        };

        var createResponse = await client.PostAsJsonAsync("/api/product", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var location = createResponse.Headers.Location!.ToString();
        var productId = location.Split('/').Last();

        // Give projection time to process
        await Task.Delay(500);

        // Act
        var getResponse = await client.GetAsync($"/api/product/{productId}");

        // Assert
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var product = await getResponse.Content.ReadFromJsonAsync<ProductDetailsReadModel>();
        product.Should().NotBeNull();
        product!.Name.Should().Be("Get Test Product");
        product.Description.Should().Be("Product for get test");
        product.Price.Should().Be(49.99m);
        product.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetProductById_NonExistentProduct_ReturnsNotFound()
    {
        // Arrange
        var client = _factory.CreateClient();
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await client.GetAsync($"/api/product/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAllProducts_ReturnsProductList()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Create a couple of products
        var product1 = new CreateProductRequest
        {
            Name = "Product 1",
            Description = "First product",
            Price = 10.00m
        };
        var product2 = new CreateProductRequest
        {
            Name = "Product 2",
            Description = "Second product",
            Price = 20.00m
        };

        await client.PostAsJsonAsync("/api/product", product1);
        await client.PostAsJsonAsync("/api/product", product2);

        // Give projections time to process
        await Task.Delay(500);

        // Act
        var response = await client.GetAsync("/api/product");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var products = await response.Content.ReadFromJsonAsync<List<ProductListItemReadModel>>();
        products.Should().NotBeNull();
        products!.Count.Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task UpdateProduct_ExistingProduct_ReturnsNoContent()
    {
        // Arrange - create a product first
        var client = _factory.CreateClient();
        var createRequest = new CreateProductRequest
        {
            Name = "Original Name",
            Description = "Original description",
            Price = 29.99m
        };

        var createResponse = await client.PostAsJsonAsync("/api/product", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var location = createResponse.Headers.Location!.ToString();
        var productId = location.Split('/').Last();

        await Task.Delay(500);

        var updateRequest = new UpdateProductRequest
        {
            Name = "Updated Name",
            Price = 39.99m
        };

        // Act
        var updateResponse = await client.PutAsJsonAsync($"/api/product/{productId}", updateRequest);

        // Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify the update
        await Task.Delay(500);
        var getResponse = await client.GetAsync($"/api/product/{productId}");
        var product = await getResponse.Content.ReadFromJsonAsync<ProductDetailsReadModel>();
        product.Should().NotBeNull();
        product!.Name.Should().Be("Updated Name");
        product.Price.Should().Be(39.99m);
    }

    [Fact]
    public async Task UpdateProduct_NonExistentProduct_ReturnsNotFound()
    {
        // Arrange
        var client = _factory.CreateClient();
        var nonExistentId = Guid.NewGuid();
        var updateRequest = new UpdateProductRequest
        {
            Name = "Updated Name",
            Price = 39.99m
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/product/{nonExistentId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteProduct_ExistingProduct_ReturnsNoContent()
    {
        // Arrange - create a product first
        var client = _factory.CreateClient();
        var createRequest = new CreateProductRequest
        {
            Name = "Product to Delete",
            Description = "Will be deactivated",
            Price = 19.99m
        };

        var createResponse = await client.PostAsJsonAsync("/api/product", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var location = createResponse.Headers.Location!.ToString();
        var productId = location.Split('/').Last();

        await Task.Delay(500);

        // Act
        var deleteResponse = await client.DeleteAsync($"/api/product/{productId}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify the product is deactivated
        await Task.Delay(500);
        var getResponse = await client.GetAsync($"/api/product/{productId}");
        var product = await getResponse.Content.ReadFromJsonAsync<ProductDetailsReadModel>();
        product.Should().NotBeNull();
        product!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteProduct_NonExistentProduct_ReturnsNotFound()
    {
        // Arrange
        var client = _factory.CreateClient();
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await client.DeleteAsync($"/api/product/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateProduct_DeactivatedProduct_ReturnsBadRequest()
    {
        // Arrange - create and deactivate a product
        var client = _factory.CreateClient();
        var createRequest = new CreateProductRequest
        {
            Name = "Product to Deactivate",
            Description = "Will be deactivated then updated",
            Price = 29.99m
        };

        var createResponse = await client.PostAsJsonAsync("/api/product", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var location = createResponse.Headers.Location!.ToString();
        var productId = location.Split('/').Last();

        await Task.Delay(500);

        // Deactivate the product
        await client.DeleteAsync($"/api/product/{productId}");
        await Task.Delay(500);

        var updateRequest = new UpdateProductRequest
        {
            Name = "Updated Name",
            Price = 39.99m
        };

        // Act - try to update deactivated product
        var updateResponse = await client.PutAsJsonAsync($"/api/product/{productId}", updateRequest);

        // Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
