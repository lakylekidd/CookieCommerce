using CookieCommerce.OrderDomain.Application.Orders.Commands;
using CookieCommerce.OrderDomain.Application.Services;
using CookieCommerce.OrderDomain.Domain.Aggregates;
using CookieCommerce.OrderDomain.Domain.Exceptions;
using Marten;
using MediatR;

namespace CookieCommerce.OrderDomain.Application.Orders.Handlers;

/// <summary>
/// Handler for adding a product line to an order.
/// Fetches product details from the Catalog service and validates availability.
/// </summary>
public class AddOrderLineHandler(IDocumentSession session, IProductCatalogService productCatalogService) 
    : IRequestHandler<AddOrderLine>
{
    public async Task Handle(AddOrderLine request, CancellationToken cancellationToken)
    {
        // Fetch product details from catalog
        var productSnapshot = await productCatalogService.GetProductAsync(request.ProductId, cancellationToken)
            ?? throw new ProductNotFoundException(request.ProductId);

        var order = await session.Events.AggregateStreamAsync<OrderAggregate>(
            request.OrderId,
            token: cancellationToken)
            ?? throw new OrderNotFoundException(request.OrderId);

        order.AddOrderLine(request.ProductId, productSnapshot, request.Quantity, productSnapshot.Price);

        session.Events.Append(request.OrderId, order.DomainEvents);
        await session.SaveChangesAsync(cancellationToken);
    }
}
