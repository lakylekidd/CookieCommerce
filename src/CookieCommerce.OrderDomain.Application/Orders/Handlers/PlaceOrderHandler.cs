using CookieCommerce.OrderDomain.Application.Orders.Commands;
using CookieCommerce.OrderDomain.Domain.Aggregates;
using Marten;
using MediatR;

namespace CookieCommerce.OrderDomain.Application.Orders.Handlers;

/// <summary>
/// Handler for placing a new order.
/// </summary>
public class PlaceOrderHandler(IDocumentSession session) : IRequestHandler<PlaceOrder, Guid>
{
    public async Task<Guid> Handle(PlaceOrder request, CancellationToken cancellationToken)
    {
        var orderId = Guid.CreateVersion7();

        var order = new OrderAggregate();
        order.PlaceOrder(orderId, request.CustomerId);

        session.Events.StartStream<OrderAggregate>(orderId, order.DomainEvents);
        await session.SaveChangesAsync(cancellationToken);

        return orderId;
    }
}
