using CookieCommerce.ProductDomain.Domain.Aggregates;
using CookieCommerce.ProductDomain.Domain.DomainEvents;
using FluentAssertions;

namespace CookieCommerce.ProductDomain.Tests;

public sealed class ProductActivationTests
{
    [Fact]
    public void Activation_restores_a_deactivated_product_and_is_idempotent()
    {
        var product = new ProductAggregate();
        product.Create(Guid.CreateVersion7(), "Cookie", "Fresh cookie", 3);
        product.Deactivate();
        product.ClearUncommitedEvents();
        product.Activate();
        product.IsActive.Should().BeTrue();
        product.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<ProductActivated>();
        product.ClearUncommitedEvents();
        product.Activate();
        product.DomainEvents.Should().BeEmpty();
        product.Rename("Chocolate cookie");
        product.Name.Should().Be("Chocolate cookie");
    }
}
