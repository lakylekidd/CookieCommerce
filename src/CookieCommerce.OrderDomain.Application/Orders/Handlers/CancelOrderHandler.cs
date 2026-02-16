using CookieCommerce.OrderDomain.Application.Orders.Commands;
using CookieCommerce.OrderDomain.Domain.Aggregates;
using CookieCommerce.OrderDomain.Domain.Exceptions;
using Marten;
using MediatR;

namespace CookieCommerce.OrderDomain.Application.Orders.Handlers;

/// <summary>
/// Handler for cancelling an order.
/// </summary>
public class CancelOrderHandler(IDocumentSession session) : IRequestHandler<CancelOrder>
{
    public async Task Handle(CancelOrder request, CancellationToken cancellationToken)
    {
        var order = await session.Events.AggregateStreamAsync<OrderAggregate>(
            request.OrderId,
            token: cancellationToken)
            ?? throw new OrderNotFoundException(request.OrderId);

        order.Cancel(request.Reason);

        session.Events.Append(request.OrderId, order.DomainEvents);
        await session.SaveChangesAsync(cancellationToken);
    }
}
