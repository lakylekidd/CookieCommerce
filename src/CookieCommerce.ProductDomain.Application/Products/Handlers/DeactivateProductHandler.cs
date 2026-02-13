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
            token: cancellationToken) 
            ?? throw new ProductNotFoundException(request.ProductId);

        product.Deactivate();

        if (!product.DomainEvents.Any()) return;

        // Only append and save if domain events were produced
        session.Events.Append(request.ProductId, product.DomainEvents);
        await session.SaveChangesAsync(cancellationToken);
    }
}
