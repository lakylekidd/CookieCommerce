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
    public void CallsChangeVariantStatusAsync_GivenFunctionalVariantStatusToggle()
    {
        var source = ProductsAreaSource.ReadComponent("SelectedInventoryVariant.razor");
        var codeBlock = source[(source.IndexOf("@code", StringComparison.Ordinal))..];

        source.Should().Contain("@onchange=\"HandleToggleStatusAsync\"");
        source.Should().NotContain("@onclick:preventDefault=\"true\"");
        codeBlock.Should().Contain("ChangeVariantStatusAsync");
        codeBlock.Should().Contain("private async Task HandleToggleStatusAsync(ChangeEventArgs e)");
    }

    [Fact]
    public void CallsChangeProductStatusAsync_GivenFunctionalProductStatusToggle()
    {
        var source = ProductsAreaSource.ReadComponent("InventoryTableItem.razor");
        var codeBlock = source[(source.IndexOf("@code", StringComparison.Ordinal))..];

        codeBlock.Should().Contain("ChangeProductStatusAsync");
    }
}
