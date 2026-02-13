using CookieCommerce.ProductDomain.Application.Products.Commands;
using CookieCommerce.ProductDomain.Domain.Aggregates;
using CookieCommerce.ProductDomain.Domain.Exceptions;
using Marten;
using MediatR;

namespace CookieCommerce.ProductDomain.Application.Products.Handlers;

/// <summary>
/// Handler for renaming an existing product.
/// </summary>
public class RenameProductHandler(IDocumentSession session) : IRequestHandler<RenameProduct>
{
    public async Task Handle(RenameProduct request, CancellationToken cancellationToken)
    {
        var product = await session.Events.AggregateStreamAsync<ProductAggregate>(
            request.ProductId, 
            token: cancellationToken)
            ?? throw new ProductNotFoundException(request.ProductId);

        product.Rename(request.NewName);

        session.Events.Append(request.ProductId, product.DomainEvents);
        await session.SaveChangesAsync(cancellationToken);
    }
}
