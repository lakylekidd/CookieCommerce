using CookieCommerce.OrderDomain.Application.Orders.Commands;
using CookieCommerce.OrderDomain.Domain.Aggregates;
using CookieCommerce.OrderDomain.Domain.Exceptions;
using Marten;
using MediatR;

namespace CookieCommerce.OrderDomain.Application.Orders.Handlers;

/// <summary>
/// Handler for adding a product line to an order.
/// </summary>
public class AddOrderLineHandler(IDocumentSession session) : IRequestHandler<AddOrderLine>
{
    public async Task Handle(AddOrderLine request, CancellationToken cancellationToken)
    {
        var order = await session.Events.AggregateStreamAsync<OrderAggregate>(
            request.OrderId,
            token: cancellationToken)
            ?? throw new OrderNotFoundException(request.OrderId);

        order.AddOrderLine(request.ProductId, request.Quantity, request.UnitPrice);

        session.Events.Append(request.OrderId, order.DomainEvents);
        await session.SaveChangesAsync(cancellationToken);
    }
}
