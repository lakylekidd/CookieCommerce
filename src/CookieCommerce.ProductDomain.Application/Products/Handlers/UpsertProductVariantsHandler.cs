using CookieCommerce.ProductDomain.Application.Products.Commands;
using CookieCommerce.ProductDomain.Domain.Aggregates;
using CookieCommerce.ProductDomain.Domain.Exceptions;
using Marten;
using MediatR;

namespace CookieCommerce.ProductDomain.Application.Products.Handlers;

public sealed class UpsertProductVariantsHandler(IDocumentSession session) : IRequestHandler<UpsertProductVariantsCommand>
{
    public async Task Handle(UpsertProductVariantsCommand request, CancellationToken cancellationToken)
    {
        ValidateRequest(request);

        var product = await session.Events.AggregateStreamAsync<ProductAggregate>(
            request.ProductId,
            token: cancellationToken)
            ?? throw new ProductNotFoundException(request.ProductId);

        var requestedVariantIds = request.Variants
            .Where(variant => variant.Id.HasValue)
            .Select(variant => variant.Id!.Value)
            .ToHashSet();

        foreach (var existingVariant in product.Variants.ToList())
        {
            if (!requestedVariantIds.Contains(existingVariant.Id))
                product.RemoveVariant(existingVariant.Id);
        }

        foreach (var variant in request.Variants)
        {
            if (variant.Id.HasValue && product.Variants.Any(existing => existing.Id == variant.Id.Value))
            {
                product.UpdateVariant(
                    variant.Id.Value,
                    variant.Name,
                    variant.Price,
                    variant.CurrencySymbol,
                    variant.IsActive,
                    variant.Sku);
            }
            else
            {
                product.AddVariant(
                    variant.Id ?? Guid.CreateVersion7(),
                    variant.Name,
                    variant.Price,
                    variant.CurrencySymbol,
                    variant.IsActive,
                    variant.Sku);
            }
        }

        if (product.DomainEvents.Count > 0)
            session.Events.Append(request.ProductId, product.DomainEvents);

        await session.SaveChangesAsync(cancellationToken);
    }

    private static void ValidateRequest(UpsertProductVariantsCommand request)
    {
        var duplicateNames = request.Variants
            .Where(variant => !string.IsNullOrWhiteSpace(variant.Name))
            .GroupBy(variant => variant.Name.Trim(), StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();

        if (duplicateNames.Count > 0)
            throw new DuplicateProductVariantException(request.ProductId, duplicateNames[0]);

        foreach (var variant in request.Variants)
        {
            if (string.IsNullOrWhiteSpace(variant.Name))
                throw new InvalidProductVariantException("Variant name is required.");

            if (variant.Price <= 0)
                throw new InvalidProductVariantException("Variant price must be greater than 0.");

            if (string.IsNullOrWhiteSpace(variant.CurrencySymbol))
                throw new InvalidProductVariantException("Variant currency symbol is required.");
        }
    }
}
