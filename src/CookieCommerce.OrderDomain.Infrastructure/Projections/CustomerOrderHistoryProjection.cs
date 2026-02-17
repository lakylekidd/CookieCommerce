using CookieCommerce.OrderDomain.Application.Orders.ReadModels;
using CookieCommerce.OrderDomain.Domain.DomainEvents;
using CookieCommerce.OrderDomain.Domain.ValueObjects;
using Marten.Events.Aggregation;

namespace CookieCommerce.OrderDomain.Infrastructure.Projections;

public class CustomerOrderHistoryProjection : SingleStreamProjection<CustomerOrderHistoryReadModel, Guid>
{
    private const decimal TaxRate = 0.10m;

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
        // Add the line total to the running total
        var subTotal = current.Total / (1 + TaxRate); // Remove tax to get subtotal
        subTotal += @event.OrderLine.LineTotal;
        current.Total = subTotal + (subTotal * TaxRate);
        return current;
    }

    public CustomerOrderHistoryReadModel Apply(OrderLineRemoved @event, CustomerOrderHistoryReadModel current)
    {
        // Note: We cannot accurately recalculate without knowing the line total
        // This is a limitation of this simplified read model
        // For accurate totals, query OrderDetailsReadModel instead
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
