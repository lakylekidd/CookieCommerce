using FluentAssertions;

namespace CookieCommerce.Web.Tests.Components.Backoffice.ProductsArea;

public sealed class Products_ComponentStructure_Tests
{
    [Fact]
    public void KeepsProductsTable_GivenExistingProductsPage()
    {
        var source = ProductsAreaSource.ReadComponent("Products.razor");

        source.Should().Contain("<InventoryTable");
        source.Should().Contain("InventoryItems=\"@InventoryItems\"");
        source.Should().Contain("OnProductSelected=\"UpdateActiveId\"");
    }

    [Fact]
    public void KeepsSelectedProductArea_GivenExistingProductsPage()
    {
        var source = ProductsAreaSource.ReadComponent("Products.razor");

        source.Should().Contain("<SelectedInventoryItem");
        source.Should().Contain("InventoryItem=\"@SelectedInventoryItem\"");
        source.Should().Contain("IsLoading=\"@IsLoadingSelectedInventoryItem\"");
    }

    [Fact]
    public void KeepsCreateProductModal_GivenExistingProductsPage()
    {
        var source = ProductsAreaSource.ReadComponent("Products.razor");

        source.Should().Contain("<CreateNewProductModal");
        source.Should().Contain("OnProductCreated=\"StateHasChanged\"");
        source.Should().Contain("CreateModal?.Open()");
    }
}
