using CookieCommerce.ProductDomain.Application.Products.Commands;
using CookieCommerce.ProductDomain.Application.Products.Handlers;
using CookieCommerce.ProductDomain.Domain.Exceptions;
using FluentAssertions;
using Marten;
using NSubstitute;

namespace CookieCommerce.ProductDomain.Tests.ApplicationTests;

public class UpsertProductVariantsHandler_Tests
{
    [Fact]
    public async Task Handle_ThrowsInvalidProductVariantException_GivenMissingVariantName()
    {
        var handler = new UpsertProductVariantsHandler(Substitute.For<IDocumentSession>());
        var command = new UpsertProductVariantsCommand(
            Guid.CreateVersion7(),
            [new UpsertProductVariant(null, string.Empty, 2.50m)]);

        Func<Task> act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidProductVariantException>()
            .WithMessage("Variant name is required.");
    }

    [Fact]
    public async Task Handle_ThrowsInvalidProductVariantException_GivenMissingVariantCurrencySymbol()
    {
        var handler = new UpsertProductVariantsHandler(Substitute.For<IDocumentSession>());
        var command = new UpsertProductVariantsCommand(
            Guid.CreateVersion7(),
            [new UpsertProductVariant(null, "Single", 2.50m, string.Empty)]);

        Func<Task> act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidProductVariantException>()
            .WithMessage("Variant currency symbol is required.");
    }

    [Fact]
    public async Task Handle_ThrowsInvalidProductVariantException_GivenNonPositiveVariantPrice()
    {
        var handler = new UpsertProductVariantsHandler(Substitute.For<IDocumentSession>());
        var command = new UpsertProductVariantsCommand(
            Guid.CreateVersion7(),
            [new UpsertProductVariant(null, "Single", 0m)]);

        Func<Task> act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidProductVariantException>()
            .WithMessage("Variant price must be greater than 0.");
    }

    [Fact]
    public async Task Handle_ThrowsDuplicateProductVariantException_GivenDuplicateVariantNames()
    {
        var productId = Guid.CreateVersion7();
        var handler = new UpsertProductVariantsHandler(Substitute.For<IDocumentSession>());
        var command = new UpsertProductVariantsCommand(
            productId,
            [
                new UpsertProductVariant(null, "Single", 2.50m),
                new UpsertProductVariant(null, " single ", 3.00m)
            ]);

        Func<Task> act = () => handler.Handle(command, CancellationToken.None);

        var exception = await act.Should().ThrowAsync<DuplicateProductVariantException>();
        exception.Which.ProductId.Should().Be(productId);
    }
}
