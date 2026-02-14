using CookieCommerce.DomainBuildingBlocks;
using CookieCommerce.ProductDomain.Application.Products.Commands;
using CookieCommerce.ProductDomain.Application.Products.Integration;
using CookieCommerce.ProductDomain.Domain.Aggregates;
using CookieCommerce.ProductDomain.Domain.Exceptions;
using Marten;
using MediatR;

namespace CookieCommerce.ProductDomain.Application.Products.Handlers;

/// <summary>
/// Handler for deactivating a product.
/// </summary>
public class DeactivateProductHandler(IDocumentSession session, IIntegrationEventPublisher publisher) : IRequestHandler<DeactivateProduct>
{
    public async Task Handle(DeactivateProduct request, CancellationToken cancellationToken)
    {
        var product = await session.Events.AggregateStreamAsync<ProductAggregate>(
            request.ProductId, 
            token: cancellationToken) 
            ?? throw new ProductNotFoundException(request.ProductId);

        product.Deactivate();

        if (!product.DomainEvents.Any()) return;

        // Append domain events and save changes atomically
        session.Events.Append(request.ProductId, product.DomainEvents);
        
        // Publish integration event via outbox (stored in same transaction)
        await publisher.PublishAsync(
            new ProductDeactivatedIntegrationEvent(request.ProductId), 
            cancellationToken);
        
        await session.SaveChangesAsync(cancellationToken);
    }
}
