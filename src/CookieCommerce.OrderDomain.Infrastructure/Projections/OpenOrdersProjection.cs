using CookieCommerce.OrderDomain.Application.Orders.ReadModels;
using CookieCommerce.OrderDomain.Domain.DomainEvents;
using CookieCommerce.OrderDomain.Domain.ValueObjects;
using Marten.Events.Aggregation;

namespace CookieCommerce.OrderDomain.Infrastructure.Projections;

public class OpenOrdersProjection : SingleStreamProjection<OpenOrdersReadModel, Guid>
{
    public OpenOrdersProjection()
    {
        // Define inline projection methods
        // Delete closed orders (paid, cancelled, rejected)
        DeleteEvent<OrderPaid>();
        DeleteEvent<OrderCancelled>();
        DeleteEvent<OrderRejected>();
    }

    public OpenOrdersReadModel Create(OrderPlaced @event)
    {
        return new OpenOrdersReadModel
        {
            Id = @event.OrderId,
            CustomerId = @event.CustomerId,
            Status = OrderStatus.Pending,
            PlacedAtUtc = @event.PlacedAtUtc,
            Total = 0
        };
    }

    public OpenOrdersReadModel Apply(OrderLineAdded @event, OpenOrdersReadModel current)
    {
        // Recalculate total with tax
        current.Total += @event.OrderLine.LineTotal;
        current.Total = current.Total * 1.10m; // Add 10% tax
        return current;
    }

    public OpenOrdersReadModel Apply(OrderLineRemoved @event, OpenOrdersReadModel current)
    {
        // Simplified - rely on OrderDetailsReadModel for accurate totals
        return current;
    }

    public OpenOrdersReadModel Apply(OrderConfirmed @event, OpenOrdersReadModel current)
    {
        current.Status = OrderStatus.Confirmed;
        current.ConfirmedAtUtc = @event.ConfirmedAtUtc;
        return current;
    }
}
