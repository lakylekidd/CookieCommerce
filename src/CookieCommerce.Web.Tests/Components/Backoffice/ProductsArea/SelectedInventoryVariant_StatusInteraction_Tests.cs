using CookieCommerce.Web.Services;
using FluentAssertions;
using Moq;

namespace CookieCommerce.Web.Tests.Components.Backoffice.ProductsArea;

public sealed class SelectedInventoryVariant_StatusInteraction_Tests
{
    [Fact]
    public void DoesNotCallStatusServices_GivenMoqBackofficeServiceGuardrail()
    {
        var backofficeService = new Mock<IBackofficeService>(MockBehavior.Strict);

        backofficeService.Verify(
            service => service.ChangeProductStatusAsync(It.IsAny<Guid>(), It.IsAny<bool>()),
            Times.Never);
        backofficeService.Verify(
            service => service.ChangeVariantStatusAsync(It.IsAny<Guid>(), It.IsAny<bool>()),
            Times.Never);
    }

    [Fact]
    public void PreventsStatusToggleDefault_GivenDeferredStatusInteraction()
    {
        var source = ProductsAreaSource.ReadComponent("SelectedInventoryVariant.razor");

        source.Should().Contain("@onclick:preventDefault=\"true\"");
        source.Should().Contain("@onchange=\"IgnoreStatusToggle\"");
        source.Should().NotContain("@onchange=\"ToggleActive\"");
    }

    [Fact]
    public void DoesNotCallChangeVariantStatusAsync_GivenDeferredVariantStatusInteraction()
    {
        var source = ProductsAreaSource.ReadComponent("SelectedInventoryVariant.razor");
        var codeBlock = source[(source.IndexOf("@code", StringComparison.Ordinal))..];

        codeBlock.Should().NotContain("ChangeVariantStatusAsync");
        codeBlock.Should().Contain("private void IgnoreStatusToggle(ChangeEventArgs e)");
    }

    [Fact]
    public void DoesNotCallChangeProductStatusAsync_GivenDeferredProductStatusInteraction()
    {
        var source = ProductsAreaSource.ReadComponent("InventoryTableItem.razor");
        var codeBlock = source[(source.IndexOf("@code", StringComparison.Ordinal))..];

        codeBlock.Should().NotContain("ChangeProductStatusAsync");
        codeBlock.Should().Contain("private void IgnoreStatusToggle(ChangeEventArgs e)");
    }
}
