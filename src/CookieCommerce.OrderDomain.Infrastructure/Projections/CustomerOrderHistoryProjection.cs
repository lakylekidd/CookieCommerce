using CookieCommerce.OrderDomain.Application.Orders.ReadModels;
using CookieCommerce.OrderDomain.Domain.DomainEvents;
using CookieCommerce.OrderDomain.Domain.ValueObjects;
using Marten.Events.Aggregation;

namespace CookieCommerce.OrderDomain.Infrastructure.Projections;

public class CustomerOrderHistoryProjection : SingleStreamProjection<CustomerOrderHistoryReadModel, Guid>
{
    public CustomerOrderHistoryProjection()
    {
        // Define inline projection methods
    }

    public CustomerOrderHistoryReadModel Create(OrderPlaced @event)
    {
        return new CustomerOrderHistoryReadModel
        {
            Id = @event.OrderId,
            CustomerId = @event.CustomerId,
            Status = OrderStatus.Pending,
            PlacedAtUtc = @event.PlacedAtUtc,
            Total = 0
        };
    }

    public CustomerOrderHistoryReadModel Apply(OrderLineAdded @event, CustomerOrderHistoryReadModel current)
    {
        // Recalculate total with tax
        current.Total += @event.OrderLine.LineTotal;
        current.Total = current.Total * 1.10m; // Add 10% tax
        return current;
    }

    public CustomerOrderHistoryReadModel Apply(OrderLineRemoved @event, CustomerOrderHistoryReadModel current)
    {
        // This is simplified - we don't have the line total here
        // In a real scenario, we might need to store order lines or recalculate
        // For now, we'll rely on the full OrderDetailsReadModel for accurate totals
        return current;
    }

    public CustomerOrderHistoryReadModel Apply(OrderConfirmed @event, CustomerOrderHistoryReadModel current)
    {
        current.Status = OrderStatus.Confirmed;
        current.ConfirmedAtUtc = @event.ConfirmedAtUtc;
        return current;
    }

    public CustomerOrderHistoryReadModel Apply(OrderPaid @event, CustomerOrderHistoryReadModel current)
    {
        current.Status = OrderStatus.Paid;
        current.PaidAtUtc = @event.PaidAtUtc;
        return current;
    }

    public CustomerOrderHistoryReadModel Apply(OrderCancelled @event, CustomerOrderHistoryReadModel current)
    {
        current.Status = OrderStatus.Cancelled;
        return current;
    }

    public CustomerOrderHistoryReadModel Apply(OrderRejected @event, CustomerOrderHistoryReadModel current)
    {
        current.Status = OrderStatus.Rejected;
        return current;
    }
}
