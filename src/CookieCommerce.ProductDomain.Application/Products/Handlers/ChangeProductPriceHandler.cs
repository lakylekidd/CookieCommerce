using CookieCommerce.ProductDomain.Application.Products.Commands;
using CookieCommerce.ProductDomain.Domain.Aggregates;
using CookieCommerce.ProductDomain.Domain.Exceptions;
using Marten;
using MediatR;

namespace CookieCommerce.ProductDomain.Application.Products.Handlers;

/// <summary>
/// Handler for changing the price of an existing product.
/// </summary>
public class ChangeProductPriceHandler(IDocumentSession session) : IRequestHandler<ChangeProductPrice>
{
    public async Task Handle(ChangeProductPrice request, CancellationToken cancellationToken)
    {
        var product = await session.Events.AggregateStreamAsync<ProductAggregate>(
            request.ProductId, 
            token: cancellationToken) 
            ?? throw new ProductNotFoundException(request.ProductId);

        product.ChangePrice(request.NewPrice);

        session.Events.Append(request.ProductId, product.DomainEvents);
        await session.SaveChangesAsync(cancellationToken);
    }
}
