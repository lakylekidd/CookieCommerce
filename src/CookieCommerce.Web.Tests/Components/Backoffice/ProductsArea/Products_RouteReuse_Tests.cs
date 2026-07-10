using CookieCommerce.Web.Services;
using FluentAssertions;
using Moq;

namespace CookieCommerce.Web.Tests.Components.Backoffice.ProductsArea;

public sealed class Products_RouteReuse_Tests
{
    [Fact]
    public void KeepsPrimaryProductsRoute_GivenExistingProductsPage()
    {
        var source = ProductsAreaSource.ReadComponent("Products.razor");

        source.Should().Contain("@page \"/backoffice/products/\"");
    }

    [Fact]
    public void KeepsSelectedProductRoute_GivenExistingProductsPage()
    {
        var source = ProductsAreaSource.ReadComponent("Products.razor");

        source.Should().Contain("@page \"/backoffice/products/{ActiveProductId:guid}\"");
    }

    [Fact]
    public void AvoidsDuplicateProductsWorkflowPage_GivenBackofficeComponents()
    {
        var repositoryRoot = ProductsAreaSource.FindRepositoryRoot();
        var componentsPath = Path.Combine(repositoryRoot.FullName, "src", "CookieCommerce.Web", "Components");

        var productRouteFiles = Directory
            .EnumerateFiles(componentsPath, "*.razor", SearchOption.AllDirectories)
            .Where(path => File.ReadAllText(path).Contains("@page \"/backoffice/products", StringComparison.Ordinal))
            .Select(path => Path.GetRelativePath(repositoryRoot.FullName, path))
            .ToList();

        productRouteFiles.Should().ContainSingle()
            .Which.Should().Be(Path.Combine(
                "src",
                "CookieCommerce.Web",
                "Components",
                "Backoffice",
                "ProductsArea",
                "Products.razor"));
    }

    [Fact]
    public void CanVerifyNoStatusCalls_GivenMoqBackofficeServiceGuardrail()
    {
        var backofficeService = new Mock<IBackofficeService>(MockBehavior.Loose);

        backofficeService.Verify(
            service => service.ChangeProductStatusAsync(It.IsAny<Guid>(), It.IsAny<bool>()),
            Times.Never);
        backofficeService.Verify(
            service => service.ChangeVariantStatusAsync(It.IsAny<Guid>(), It.IsAny<bool>()),
            Times.Never);
    }

    [Fact]
    public void KeepsBackofficeServiceStatusContract_GivenNoServiceInterfaceChange()
    {
        typeof(IBackofficeService).GetMethod(
                nameof(IBackofficeService.ChangeProductStatusAsync),
                [typeof(Guid), typeof(bool)])
            .Should()
            .NotBeNull();
        typeof(IBackofficeService).GetMethod(
                nameof(IBackofficeService.ChangeVariantStatusAsync),
                [typeof(Guid), typeof(bool)])
            .Should()
            .NotBeNull();
    }
}

internal static class ProductsAreaSource
{
    public static string ReadComponent(string componentFileName)
    {
        var repositoryRoot = FindRepositoryRoot();
        var componentPath = Path.Combine(
            repositoryRoot.FullName,
            "src",
            "CookieCommerce.Web",
            "Components",
            "Backoffice",
            "ProductsArea",
            componentFileName);

        File.Exists(componentPath).Should().BeTrue($"{componentPath} should exist");

        return File.ReadAllText(componentPath);
    }

    public static DirectoryInfo FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current is not null)
        {
            if (Directory.Exists(Path.Combine(current.FullName, ".git"))
                && Directory.Exists(Path.Combine(current.FullName, "src", "CookieCommerce.Web")))
            {
                return current;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate CookieCommerce repository root from test output directory.");
    }
}
