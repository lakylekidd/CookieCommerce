using CookieCommerce.ProductDomain.Application.Products.ReadModels;
using CookieCommerce.ProductDomain.Domain.DomainEvents;
using CookieCommerce.ProductDomain.Infrastructure.Projections;
using FluentAssertions;

namespace CookieCommerce.ProductDomain.Tests.ProjectionsTests;

public class ProductDetailsProjectionTests
{
    [Fact]
    public void Create_ProductCreated_ReturnsReadModelWithDescription()
    {
        // Arrange
        var projection = new ProductDetailsProjection();
        var productId = Guid.NewGuid();

        var created = new ProductCreated
        {
            ProductId = productId,
            Name = "Chocolate Chip Cookies",
            Description = "Delicious homemade cookies with premium chocolate chips",
            Price = 12.99m
        };

        // Act
        var result = projection.Create(created);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(productId);
        result.Name.Should().Be("Chocolate Chip Cookies");
        result.Description.Should().Be("Delicious homemade cookies with premium chocolate chips");
        result.Price.Should().Be(12.99m);
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Apply_ProductRenamed_UpdatesName()
    {
        // Arrange
        var projection = new ProductDetailsProjection();
        var productId = Guid.NewGuid();

        var current = new ProductDetailsReadModel
        {
            Id = productId,
            Name = "Old Name",
            Description = "Description",
            Price = 10.00m,
            IsActive = true
        };

        var renamed = new ProductRenamed(productId, "New Name");

        // Act
        var result = projection.Apply(renamed, current);

        // Assert
        result.Name.Should().Be("New Name");
        result.Id.Should().Be(productId);
        result.Description.Should().Be("Description");
        result.Price.Should().Be(10.00m);
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Apply_ProductPriceChanged_UpdatesPrice()
    {
        // Arrange
        var projection = new ProductDetailsProjection();
        var productId = Guid.NewGuid();

        var current = new ProductDetailsReadModel
        {
            Id = productId,
            Name = "Chocolate Chip Cookies",
            Description = "Delicious cookies",
            Price = 10.00m,
            IsActive = true
        };

        var priceChanged = new ProductPriceChanged(productId, 10.00m, 15.99m);

        // Act
        var result = projection.Apply(priceChanged, current);

        // Assert
        result.Price.Should().Be(15.99m);
        result.Id.Should().Be(productId);
        result.Name.Should().Be("Chocolate Chip Cookies");
        result.Description.Should().Be("Delicious cookies");
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Apply_ProductDeactivated_SetsIsActiveFalse()
    {
        // Arrange
        var projection = new ProductDetailsProjection();
        var productId = Guid.NewGuid();

        var current = new ProductDetailsReadModel
        {
            Id = productId,
            Name = "Chocolate Chip Cookies",
            Description = "Delicious cookies",
            Price = 10.00m,
            IsActive = true
        };

        var deactivated = new ProductDeactivated(productId);

        // Act
        var result = projection.Apply(deactivated, current);

        // Assert
        result.IsActive.Should().BeFalse();
        result.Id.Should().Be(productId);
        result.Name.Should().Be("Chocolate Chip Cookies");
        result.Description.Should().Be("Delicious cookies");
        result.Price.Should().Be(10.00m);
    }

    [Fact]
    public void Apply_MultipleEvents_AppliesInSequence()
    {
        // Arrange
        var projection = new ProductDetailsProjection();
        var productId = Guid.NewGuid();

        var created = new ProductCreated
        {
            ProductId = productId,
            Name = "Original Name",
            Description = "Original Description",
            Price = 10.00m
        };

        // Act
        var result = projection.Create(created);
        result = projection.Apply(new ProductRenamed(productId, "Updated Name"), result);
        result = projection.Apply(new ProductPriceChanged(productId, 10.00m, 20.00m), result);
        result = projection.Apply(new ProductDeactivated(productId), result);

        // Assert
        result.Id.Should().Be(productId);
        result.Name.Should().Be("Updated Name");
        result.Description.Should().Be("Original Description");
        result.Price.Should().Be(20.00m);
        result.IsActive.Should().BeFalse();
    }
}
