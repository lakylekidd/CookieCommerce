using CookieCommerce.DomainBuildingBlocks;
using CookieCommerce.ProductDomain.Application.Products.Commands;
using CookieCommerce.ProductDomain.Application.Products.Integration;
using CookieCommerce.ProductDomain.Domain.Aggregates;
using Marten;
using MediatR;

namespace CookieCommerce.ProductDomain.Application.Products.Handlers;

/// <summary>
/// Handler for creating a new product.
/// </summary>
public class CreateProductHandler(IDocumentSession session, IIntegrationEventPublisher publisher) : IRequestHandler<CreateProduct, Guid>
{
    public async Task<Guid> Handle(CreateProduct request, CancellationToken cancellationToken)
    {
        var productId = Guid.CreateVersion7();

        var product = new ProductAggregate();
        product.Create(productId, request.Name, request.Description, request.Price);

        session.Events.StartStream<ProductAggregate>(productId, product.DomainEvents);
        
        // Publish integration event via outbox (stored in same transaction)
        await publisher.PublishAsync(
            new ProductSnapshotPublished(
                productId,
                request.Name,
                request.Description,
                request.Price,
                IsActive: true),
            cancellationToken);
        
        await session.SaveChangesAsync(cancellationToken);

        return productId;
    }
}
