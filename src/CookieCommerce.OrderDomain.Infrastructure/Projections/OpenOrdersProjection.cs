using CookieCommerce.OrderDomain.Application.Orders.ReadModels;
using CookieCommerce.OrderDomain.Domain.DomainEvents;
using CookieCommerce.OrderDomain.Domain.ValueObjects;
using Marten.Events.Aggregation;

namespace CookieCommerce.OrderDomain.Infrastructure.Projections;

public class OpenOrdersProjection : SingleStreamProjection<OpenOrdersReadModel, Guid>
{
    private const decimal TaxRate = 0.10m;

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
        // Add the line total with tax directly
        current.Total += @event.OrderLine.LineTotal * (1 + TaxRate);
        return current;
    }

    public OpenOrdersReadModel Apply(OrderLineRemoved @event, OpenOrdersReadModel current)
    {
        // Note: We cannot accurately recalculate without knowing the line total
        // This is a limitation of this simplified read model
        // For accurate totals, query OrderDetailsReadModel instead
        return current;
    }

    public OpenOrdersReadModel Apply(OrderConfirmed @event, OpenOrdersReadModel current)
    {
        current.Status = OrderStatus.Confirmed;
        current.ConfirmedAtUtc = @event.ConfirmedAtUtc;
        return current;
    }
}
