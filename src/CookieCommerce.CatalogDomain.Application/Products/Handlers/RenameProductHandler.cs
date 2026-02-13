using CookieCommerce.CatalogDomain.Application.Products.Commands;
using CookieCommerce.CatalogDomain.Domain.Aggregates;
using Marten;
using MediatR;

namespace CookieCommerce.CatalogDomain.Application.Products.Handlers;

/// <summary>
/// Handler for renaming an existing product.
/// </summary>
public class RenameProductHandler(IDocumentSession session) : IRequestHandler<RenameProduct>
{
    public async Task Handle(RenameProduct request, CancellationToken cancellationToken)
    {
        var catalog = await session.Events.AggregateStreamAsync<CatalogAggregate>(
            request.ProductId, 
            token: cancellationToken);

        if (catalog == null)
            throw new InvalidOperationException($"Product with ID {request.ProductId} not found.");

        catalog.Rename(request.NewName);

        session.Events.Append(request.ProductId, catalog.DomainEvents);
        await session.SaveChangesAsync(cancellationToken);
    }
}
