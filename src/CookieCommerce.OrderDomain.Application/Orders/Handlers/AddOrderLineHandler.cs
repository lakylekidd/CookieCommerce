using CookieCommerce.OrderDomain.Application.Orders.Commands;
using CookieCommerce.OrderDomain.Application.Products.ReadModels;
using CookieCommerce.OrderDomain.Domain.Aggregates;
using CookieCommerce.OrderDomain.Domain.Exceptions;
using CookieCommerce.OrderDomain.Domain.ValueObjects;
using Marten;
using MediatR;

namespace CookieCommerce.OrderDomain.Application.Orders.Handlers;

/// <summary>
/// Handler for adding a product line to an order.
/// Uses local product snapshot read model maintained via integration events for eventual consistency.
/// </summary>
public class AddOrderLineHandler(IDocumentSession session) : IRequestHandler<AddOrderLine>
{
    public async Task Handle(AddOrderLine request, CancellationToken cancellationToken)
    {
        // Query local product snapshot read model
        var productSnapshot = await session.LoadAsync<ProductSnapshotReadModel>(request.ProductId, cancellationToken)
            ?? throw new ProductNotFoundException(request.ProductId);

        // Validate product is active
        if (!productSnapshot.IsActive)
            throw new ProductNotFoundException(request.ProductId);

        var order = await session.Events.AggregateStreamAsync<OrderAggregate>(
            request.OrderId,
            token: cancellationToken)
            ?? throw new OrderNotFoundException(request.OrderId);

        // Create domain ProductSnapshot from read model
        var domainProductSnapshot = new ProductSnapshot(
            productSnapshot.Id,
            productSnapshot.Name,
            productSnapshot.Price);

        order.AddOrderLine(request.ProductId, domainProductSnapshot, request.Quantity, productSnapshot.Price);

        session.Events.Append(request.OrderId, order.DomainEvents);
        await session.SaveChangesAsync(cancellationToken);
    }
}
