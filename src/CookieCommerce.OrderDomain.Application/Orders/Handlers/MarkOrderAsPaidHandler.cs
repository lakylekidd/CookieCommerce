using CookieCommerce.OrderDomain.Application.Orders.Commands;
using CookieCommerce.OrderDomain.Domain.Aggregates;
using CookieCommerce.OrderDomain.Domain.Exceptions;
using Marten;
using MediatR;

namespace CookieCommerce.OrderDomain.Application.Orders.Handlers;

/// <summary>
/// Handler for marking a confirmed order as paid.
/// </summary>
public class MarkOrderAsPaidHandler(IDocumentSession session) : IRequestHandler<MarkOrderAsPaid>
{
    public async Task Handle(MarkOrderAsPaid request, CancellationToken cancellationToken)
    {
        var order = await session.Events.AggregateStreamAsync<OrderAggregate>(
            request.OrderId,
            token: cancellationToken)
            ?? throw new OrderNotFoundException(request.OrderId);

        order.MarkAsPaid();

        session.Events.Append(request.OrderId, order.DomainEvents);
        await session.SaveChangesAsync(cancellationToken);
    }
}
