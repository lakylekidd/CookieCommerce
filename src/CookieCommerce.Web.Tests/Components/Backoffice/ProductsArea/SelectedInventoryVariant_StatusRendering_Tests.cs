using FluentAssertions;

namespace CookieCommerce.Web.Tests.Components.Backoffice.ProductsArea;

public sealed class SelectedInventoryVariant_StatusRendering_Tests
{
    [Fact]
    public void RendersStatusControl_GivenVariantRow()
    {
        var source = ProductsAreaSource.ReadComponent("SelectedInventoryVariant.razor");

        source.Should().Contain("type=\"checkbox\"");
        source.Should().Contain("checked=\"@Variant.Active\"");
        source.Should().Contain("@(Variant.Active ? \"Live\" : \"Draft\")");
    }

    [Fact]
    public void RendersVariantStatusControlAsInteractive_GivenFunctionalToggle()
    {
        var source = ProductsAreaSource.ReadComponent("SelectedInventoryVariant.razor");

        source.Should().Contain("@onchange=\"HandleToggleStatusAsync\"");
        source.Should().Contain("disabled=\"@isTogglingStatus\"");
        source.Should().NotContain("aria-disabled=\"true\"");
        source.Should().NotContain("Status changes are not saved yet");
    }

    [Fact]
    public void RendersProductStatusControlAsInteractive_GivenFunctionalToggle()
    {
        var source = ProductsAreaSource.ReadComponent("InventoryTableItem.razor");

        source.Should().Contain("@onchange=\"HandleStatusToggle\"");
        source.Should().Contain("disabled=\"@IsTogglingStatus\"");
        source.Should().NotContain("aria-disabled=\"true\"");
        source.Should().NotContain("Status changes are not saved yet");
    }
}
