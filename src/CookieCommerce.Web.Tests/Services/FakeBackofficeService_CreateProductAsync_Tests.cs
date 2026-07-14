using CookieCommerce.Web.Models.Products;
using CookieCommerce.Web.Services;
using CookieCommerce.Web.Services.Products;
using CookieCommerce.Web.Tests.Helpers;
using FluentAssertions;

namespace CookieCommerce.Web.Tests.Services;

public sealed class FakeBackofficeService_CreateProductAsync_Tests
{
    [Fact]
    public async Task CreatesDraftVariants_GivenProductCreatedWithoutFunctionalStatusInput()
    {
        var service = new FakeBackofficeService(TestEnvironmentFactory.CreateTempWebHostEnvironment());
        var productReference = $"TEST-{Guid.NewGuid():N}";
        var productName = "Status Deferred Test Cookie";
        var product = new CreateProductDto
        {
            ProductReference = productReference,
            ProductName = productName,
            Variants =
            [
                new CreateProductVariantDto
                {
                    Name = "Standard",
                    Price = 2.50m
                }
            ]
        };

        await service.CreateProductAsync(product);

        var createdProduct = StaticInventoryList.InventoryItems.Single(item => item.ProductReference == productReference);
        createdProduct.ProductName.Should().Be(productName);
        createdProduct.Variants.Should().ContainSingle();
        createdProduct.Variants.Single().Active.Should().BeFalse();
    }
}
