using CookieCommerce.CatalogDomain.Application.Products.Commands;
using CookieCommerce.CatalogDomain.Domain.Aggregates;
using Marten;
using MediatR;

namespace CookieCommerce.CatalogDomain.Application.Products.Handlers;

/// <summary>
/// Handler for creating a new product.
/// </summary>
public class CreateProductHandler(IDocumentSession session) : IRequestHandler<CreateProduct, Guid>
{
    public async Task<Guid> Handle(CreateProduct request, CancellationToken cancellationToken)
    {
        var productId = Guid.CreateVersion7();

        var catalog = new CatalogAggregate();
        catalog.Create(productId, request.Name, request.Description, request.Price);

        session.Events.StartStream<CatalogAggregate>(productId, catalog.DomainEvents);
        await session.SaveChangesAsync(cancellationToken);

        return productId;
    }
}
