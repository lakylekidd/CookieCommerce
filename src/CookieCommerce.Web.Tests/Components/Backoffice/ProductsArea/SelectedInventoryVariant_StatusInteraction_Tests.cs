using System.Reflection;
using CookieCommerce.Web.Components.Backoffice.ProductsArea;
using CookieCommerce.Web.Models.Products;
using CookieCommerce.Web.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Moq;

namespace CookieCommerce.Web.Tests.Components.Backoffice.ProductsArea;

public sealed class SelectedInventoryVariant_StatusInteraction_Tests
{
    [Fact]
    public async Task Variant_status_is_saved_before_the_display_changes()
    {
        var service = new Mock<IBackofficeService>(MockBehavior.Strict);
        var variant = new ProductVariantDto { Id = Guid.CreateVersion7(), Active = false };
        service.Setup(s => s.ChangeVariantStatusAsync(variant.Id, true)).Returns(Task.CompletedTask);
        var component = new SelectedInventoryVariant { Variant = variant };
        await ToggleAsync(component, service.Object, "HandleToggleStatusAsync", [new ChangeEventArgs { Value = true }]);
        variant.Active.Should().BeTrue();
        service.Verify(s => s.ChangeVariantStatusAsync(variant.Id, true), Times.Once);
    }
    [Fact]
    public async Task Product_status_is_saved_before_the_display_changes()
    {
        var service = new Mock<IBackofficeService>(MockBehavior.Strict);
        var product = new ProductDto { ProductId = Guid.CreateVersion7(), Active = false };
        service.Setup(s => s.ChangeProductStatusAsync(product.ProductId, true)).Returns(Task.CompletedTask);
        var component = new InventoryTableItem { Item = product };
        await ToggleAsync(component, service.Object, "HandleStatusToggle", []);
        product.Active.Should().BeTrue();
        service.Verify(s => s.ChangeProductStatusAsync(product.ProductId, true), Times.Once);
    }

    private static async Task ToggleAsync(object component, IBackofficeService service, string methodName, object[] args)
    {
        var type = component.GetType();
        type.GetProperty("BackofficeService", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)!.SetValue(component, service);
        await (Task)type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(component, args)!;
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
