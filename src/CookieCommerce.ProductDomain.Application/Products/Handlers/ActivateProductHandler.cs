using CookieCommerce.ProductDomain.Application.Products.Commands;
using CookieCommerce.ProductDomain.Domain.Aggregates;
using CookieCommerce.ProductDomain.Domain.Exceptions;
using Marten;
using MediatR;

namespace CookieCommerce.ProductDomain.Application.Products.Handlers;

public sealed class ActivateProductHandler(IDocumentSession session) : IRequestHandler<ActivateProduct>
{
    public async Task Handle(ActivateProduct request, CancellationToken cancellationToken)
    {
        var product = await session.Events.AggregateStreamAsync<ProductAggregate>(request.ProductId, token: cancellationToken)
            ?? throw new ProductNotFoundException(request.ProductId);
        product.Activate();
        if (product.DomainEvents.Count == 0) return;
        session.Events.Append(request.ProductId, product.DomainEvents);
        await session.SaveChangesAsync(cancellationToken);
    }
}
