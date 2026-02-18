using CookieCommerce.OrderDomain.Application.Orders.ReadModels;
using CookieCommerce.OrderDomain.Domain.DomainEvents;
using CookieCommerce.OrderDomain.Domain.ValueObjects;
using Marten.Events.Aggregation;

namespace CookieCommerce.OrderDomain.Infrastructure.Projections;

public class OpenOrdersProjection : SingleStreamProjection<OpenOrderReadModel, Guid>
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

    public OpenOrderReadModel Create(OrderPlaced @event)
    {
        return new OpenOrderReadModel
        {
            Id = @event.OrderId,
            CustomerId = @event.CustomerId,
            Status = OrderStatus.Pending,
            PlacedAtUtc = @event.PlacedAtUtc,
            Total = 0
        };
    }

    public OpenOrderReadModel Apply(OrderLineAdded @event, OpenOrderReadModel current)
    {
        // Add the line total with tax directly
        current.Total += @event.OrderLine.LineTotal * (1 + TaxRate);
        return current;
    }

    public OpenOrderReadModel Apply(OrderLineRemoved @event, OpenOrderReadModel current)
    {
        // Note: We cannot accurately recalculate without knowing the line total
        // This is a limitation of this simplified read model
        // For accurate totals, query OrderDetailsReadModel instead
        return current;
    }

    public OpenOrderReadModel Apply(OrderConfirmed @event, OpenOrderReadModel current)
    {
        current.Status = OrderStatus.Confirmed;
        current.ConfirmedAtUtc = @event.ConfirmedAtUtc;
        return current;
    }
}
