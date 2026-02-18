using CookieCommerce.OrderDomain.Application.Orders.ReadModels;
using CookieCommerce.OrderDomain.Application.Orders.ValueObjects;
using CookieCommerce.OrderDomain.Domain.DomainEvents;
using CookieCommerce.OrderDomain.Domain.ValueObjects;
using Marten.Events.Aggregation;

namespace CookieCommerce.OrderDomain.Infrastructure.Projections;

public class OrderDetailsProjection : SingleStreamProjection<OrderDetailsReadModel, Guid>
{
    private const decimal TaxRate = 0.10m;

    public OrderDetailsProjection()
    {
        // Define inline projection methods - no delete events for orders
    }

    public OrderDetailsReadModel Create(OrderPlaced @event)
    {
        var readModel = new OrderDetailsReadModel
        {
            Id = @event.OrderId,
            CustomerId = @event.CustomerId,
            CustomerName = @event.CustomerSnapshot?.CustomerName,
            CustomerMaskedEmail = @event.CustomerSnapshot?.MaskedEmail,
            Status = OrderStatus.Pending,
            PlacedAtUtc = @event.PlacedAtUtc,
            OrderLines = new List<OrderLineReadModel>(),
            SubTotal = 0,
            TaxAmount = 0,
            Total = 0,
            StatusDate = @event.PlacedAtUtc
        };

        readModel.StatusHistory[OrderStatus.Pending.ToString()] = @event.PlacedAtUtc;

        return readModel;
    }

    public OrderDetailsReadModel Apply(OrderLineAdded @event, OrderDetailsReadModel current)
    {
        var orderLine = new OrderLineReadModel
        {
            ProductId = @event.OrderLine.ProductId,
            Quantity = @event.OrderLine.Quantity,
            PriceInfo = new PriceInfo
            {
                BasePrice = @event.OrderLine.UnitPrice,
                TaxType = TaxType.Percentage,
                TaxValue = TaxRate,
                IsTaxIncluded = false
            },
            LineTotal = @event.OrderLine.LineTotal
        };

        current.OrderLines.Add(orderLine);
        current.SubTotal = current.OrderLines.Sum(ol => ol.LineTotal);
        current.TaxAmount = current.SubTotal * TaxRate;
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
            current.TaxAmount = current.SubTotal * TaxRate;
            current.Total = current.SubTotal + current.TaxAmount;
        }

        return current;
    }

    public OrderDetailsReadModel Apply(OrderConfirmed @event, OrderDetailsReadModel current)
    {
        current.Status = OrderStatus.Confirmed;
        current.StatusDate = @event.ConfirmedAtUtc;
        current.StatusHistory[OrderStatus.Confirmed.ToString()] = @event.ConfirmedAtUtc;
        return current;
    }

    public OrderDetailsReadModel Apply(OrderPaid @event, OrderDetailsReadModel current)
    {
        current.Status = OrderStatus.Paid;
        current.StatusDate = @event.PaidAtUtc;
        current.StatusHistory[OrderStatus.Paid.ToString()] = @event.PaidAtUtc;
        return current;
    }

    public OrderDetailsReadModel Apply(OrderCancelled @event, OrderDetailsReadModel current)
    {
        current.Status = OrderStatus.Cancelled;
        current.StatusDate = @event.CancelledAtUtc;
        current.StatusHistory[OrderStatus.Cancelled.ToString()] = @event.CancelledAtUtc;
        return current;
    }

    public OrderDetailsReadModel Apply(OrderRejected @event, OrderDetailsReadModel current)
    {
        current.Status = OrderStatus.Rejected;
        current.StatusDate = @event.RejectedAtUtc;
        current.StatusHistory[OrderStatus.Rejected.ToString()] = @event.RejectedAtUtc;
        return current;
    }
}
