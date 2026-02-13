using CookieCommerce.CatalogDomain.Application.Products.Commands;
using CookieCommerce.CatalogDomain.Domain.Aggregates;
using Marten;
using MediatR;

namespace CookieCommerce.CatalogDomain.Application.Products.Handlers;

/// <summary>
/// Handler for deactivating a product.
/// </summary>
public class DeactivateProductHandler(IDocumentSession session) : IRequestHandler<DeactivateProduct>
{
    public async Task Handle(DeactivateProduct request, CancellationToken cancellationToken)
    {
        var catalog = await session.Events.AggregateStreamAsync<CatalogAggregate>(
            request.ProductId, 
            token: cancellationToken);

        if (catalog == null)
            throw new InvalidOperationException($"Product with ID {request.ProductId} not found.");

        catalog.Deactivate();

        session.Events.Append(request.ProductId, catalog.DomainEvents);
        await session.SaveChangesAsync(cancellationToken);
    }
}
