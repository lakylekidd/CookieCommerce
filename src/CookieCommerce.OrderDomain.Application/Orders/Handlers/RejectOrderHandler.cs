using CookieCommerce.OrderDomain.Application.Orders.Commands;
using CookieCommerce.OrderDomain.Domain.Aggregates;
using CookieCommerce.OrderDomain.Domain.Exceptions;
using Marten;
using MediatR;

namespace CookieCommerce.OrderDomain.Application.Orders.Handlers;

/// <summary>
/// Handler for rejecting an order.
/// </summary>
public class RejectOrderHandler(IDocumentSession session) : IRequestHandler<RejectOrder>
{
    public async Task Handle(RejectOrder request, CancellationToken cancellationToken)
    {
        var order = await session.Events.AggregateStreamAsync<OrderAggregate>(
            request.OrderId,
            token: cancellationToken)
            ?? throw new OrderNotFoundException(request.OrderId);

        order.Reject(request.Reason);

        session.Events.Append(request.OrderId, order.DomainEvents);
        await session.SaveChangesAsync(cancellationToken);
    }
}
