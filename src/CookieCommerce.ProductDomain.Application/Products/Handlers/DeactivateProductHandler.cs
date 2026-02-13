using CookieCommerce.ProductDomain.Application.Products.Commands;
using CookieCommerce.ProductDomain.Domain.Aggregates;
using CookieCommerce.ProductDomain.Domain.Exceptions;
using Marten;
using MediatR;

namespace CookieCommerce.ProductDomain.Application.Products.Handlers;

/// <summary>
/// Handler for deactivating a product.
/// </summary>
public class DeactivateProductHandler(IDocumentSession session) : IRequestHandler<DeactivateProduct>
{
    public async Task Handle(DeactivateProduct request, CancellationToken cancellationToken)
    {
        var product = await session.Events.AggregateStreamAsync<ProductAggregate>(
            request.ProductId, 
            token: cancellationToken);

        if (product == null)
            throw new ProductNotFoundException(request.ProductId);

        product.Deactivate();

        // Only append and save if domain events were produced
        if (product.DomainEvents.Any())
        {
            session.Events.Append(request.ProductId, product.DomainEvents);
            await session.SaveChangesAsync(cancellationToken);
        }
    }
}
