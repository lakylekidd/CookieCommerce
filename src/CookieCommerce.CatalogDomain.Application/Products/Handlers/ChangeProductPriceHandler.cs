using CookieCommerce.CatalogDomain.Application.Products.Commands;
using CookieCommerce.CatalogDomain.Domain.Aggregates;
using Marten;
using MediatR;

namespace CookieCommerce.CatalogDomain.Application.Products.Handlers;

/// <summary>
/// Handler for changing the price of an existing product.
/// </summary>
public class ChangeProductPriceHandler(IDocumentSession session) : IRequestHandler<ChangeProductPrice>
{
    public async Task Handle(ChangeProductPrice request, CancellationToken cancellationToken)
    {
        var catalog = await session.Events.AggregateStreamAsync<CatalogAggregate>(
            request.ProductId, 
            token: cancellationToken);

        if (catalog == null)
            throw new InvalidOperationException($"Product with ID {request.ProductId} not found.");

        catalog.ChangePrice(request.NewPrice);

        session.Events.Append(request.ProductId, catalog.DomainEvents);
        await session.SaveChangesAsync(cancellationToken);
    }
}
