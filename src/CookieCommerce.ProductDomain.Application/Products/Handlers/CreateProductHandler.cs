using CookieCommerce.ProductDomain.Application.Products.Commands;
using CookieCommerce.ProductDomain.Domain.Aggregates;
using Marten;
using MediatR;

namespace CookieCommerce.ProductDomain.Application.Products.Handlers;

/// <summary>
/// Handler for creating a new product.
/// </summary>
public class CreateProductHandler(IDocumentSession session) : IRequestHandler<CreateProduct, Guid>
{
    public async Task<Guid> Handle(CreateProduct request, CancellationToken cancellationToken)
    {
        var productId = Guid.CreateVersion7();

        var product = new ProductAggregate();
        product.Create(productId, request.Name, request.Description, request.Price);

        session.Events.StartStream<ProductAggregate>(productId, product.DomainEvents);
        await session.SaveChangesAsync(cancellationToken);

        return productId;
    }
}
