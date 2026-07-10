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
    public void KeepsVariantStatusControlAccessible_GivenStatusBehaviorIsDeferred()
    {
        var source = ProductsAreaSource.ReadComponent("SelectedInventoryVariant.razor");

        source.Should().Contain("aria-label=\"Variant status, visible but not editable yet\"");
        source.Should().Contain("aria-disabled=\"true\"");
        source.Should().Contain("Status changes are not saved yet");
    }

    [Fact]
    public void KeepsProductStatusControlAccessible_GivenStatusBehaviorIsDeferred()
    {
        var source = ProductsAreaSource.ReadComponent("InventoryTableItem.razor");

        source.Should().Contain("aria-label=\"Product status, visible but not editable yet\"");
        source.Should().Contain("aria-disabled=\"true\"");
        source.Should().Contain("Status changes are not saved yet");
    }
}
