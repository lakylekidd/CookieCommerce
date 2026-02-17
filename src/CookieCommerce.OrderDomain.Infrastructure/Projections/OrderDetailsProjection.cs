using CookieCommerce.OrderDomain.Application.Orders.ReadModels;
using CookieCommerce.OrderDomain.Domain.DomainEvents;
using CookieCommerce.OrderDomain.Domain.ValueObjects;
using Marten.Events.Aggregation;

namespace CookieCommerce.OrderDomain.Infrastructure.Projections;

public class OrderDetailsProjection : SingleStreamProjection<OrderDetailsReadModel, Guid>
{
    public OrderDetailsProjection()
    {
        // Define inline projection methods - no delete events for orders
    }

    public OrderDetailsReadModel Create(OrderPlaced @event)
    {
        return new OrderDetailsReadModel
        {
            Id = @event.OrderId,
            CustomerId = @event.CustomerId,
            Status = OrderStatus.Pending,
            PlacedAtUtc = @event.PlacedAtUtc,
            OrderLines = new List<OrderLineReadModel>(),
            SubTotal = 0,
            TaxAmount = 0,
            Total = 0
        };
    }

    public OrderDetailsReadModel Apply(OrderLineAdded @event, OrderDetailsReadModel current)
    {
        var orderLine = new OrderLineReadModel
        {
            ProductId = @event.OrderLine.ProductId,
            Quantity = @event.OrderLine.Quantity,
            UnitPrice = @event.OrderLine.UnitPrice,
            LineTotal = @event.OrderLine.LineTotal
        };

        current.OrderLines.Add(orderLine);
        current.SubTotal = current.OrderLines.Sum(ol => ol.LineTotal);
        current.TaxAmount = current.SubTotal * 0.10m;
        current.Total = current.SubTotal + current.TaxAmount;

        return current;
    }

    public OrderDetailsReadModel Apply(OrderLineRemoved @event, OrderDetailsReadModel current)
    {
        var orderLine = current.OrderLines.FirstOrDefault(ol => ol.ProductId == @event.ProductId);
        if (orderLine != null)
        {
            current.OrderLines.Remove(orderLine);
            current.SubTotal = current.OrderLines.Sum(ol => ol.LineTotal);
            current.TaxAmount = current.SubTotal * 0.10m;
            current.Total = current.SubTotal + current.TaxAmount;
        }

        return current;
    }

    public OrderDetailsReadModel Apply(OrderConfirmed @event, OrderDetailsReadModel current)
    {
        current.Status = OrderStatus.Confirmed;
        current.ConfirmedAtUtc = @event.ConfirmedAtUtc;
        return current;
    }

    public OrderDetailsReadModel Apply(OrderPaid @event, OrderDetailsReadModel current)
    {
        current.Status = OrderStatus.Paid;
        current.PaidAtUtc = @event.PaidAtUtc;
        return current;
    }

    public OrderDetailsReadModel Apply(OrderCancelled @event, OrderDetailsReadModel current)
    {
        current.Status = OrderStatus.Cancelled;
        current.CancelledAtUtc = @event.CancelledAtUtc;
        return current;
    }

    public OrderDetailsReadModel Apply(OrderRejected @event, OrderDetailsReadModel current)
    {
        current.Status = OrderStatus.Rejected;
        current.RejectedAtUtc = @event.RejectedAtUtc;
        return current;
    }
}
