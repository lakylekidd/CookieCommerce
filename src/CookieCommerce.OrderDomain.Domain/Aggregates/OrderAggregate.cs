using CookieCommerce.OrderDomain.Domain.DomainEvents;
using CookieCommerce.OrderDomain.Domain.Exceptions;
using CookieCommerce.OrderDomain.Domain.ValueObjects;
using CookieCommerce.DomainBuildingBlocks;

namespace CookieCommerce.OrderDomain.Domain.Aggregates;

public class OrderAggregate : AggregateBase
{
    private const decimal TaxRate = 0.10m;

    private readonly List<OrderLine> _orderLines = [];
    public IReadOnlyCollection<OrderLine> OrderLines => _orderLines.AsReadOnly();

    public Guid CustomerId { get; private set; }
    public CustomerSnapshot? CustomerSnapshot { get; private set; }
    public OrderAddress? ShippingAddress { get; private set; }
    public OrderAddress? BillingAddress { get; private set; }
    public OrderStatus Status { get; private set; }
    public decimal SubTotal { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal Total { get; private set; }
    public DateTime PlacedAtUtc { get; private set; }
    public DateTime? ConfirmedAtUtc { get; private set; }
    public DateTime? PaidAtUtc { get; private set; }
    public DateTime? CancelledAtUtc { get; private set; }
    public DateTime? RejectedAtUtc { get; private set; }

    public OrderAggregate() { }

    public void PlaceOrder(Guid orderId, Guid customerId, CustomerSnapshot? customerSnapshot = null, OrderAddress? shippingAddress = null, OrderAddress? billingAddress = null)
    {
        if (customerId == Guid.Empty)
            throw new ArgumentException("Customer ID is required.", nameof(customerId));

        Apply(new OrderPlaced
        {
            OrderId = orderId,
            CustomerId = customerId,
            CustomerSnapshot = customerSnapshot,
            ShippingAddress = shippingAddress,
            BillingAddress = billingAddress,
            PlacedAtUtc = DateTime.UtcNow
        });
    }

    public void AddOrderLine(Guid productId, int quantity, decimal unitPrice)
    {
        if (Status != OrderStatus.Pending)
            throw new CannotModifyNonPendingOrderException(Id, Status.ToString());

        if (_orderLines.Any(ol => ol.ProductId == productId))
            throw new DuplicateOrderLineException(Id, productId);

        var orderLine = new OrderLine(productId, quantity, unitPrice);

        Apply(new OrderLineAdded
        {
            OrderId = Id,
            OrderLine = orderLine
        });

        RecalculateTotals();
    }

    public void RemoveOrderLine(Guid productId)
    {
        if (Status != OrderStatus.Pending)
            throw new CannotModifyNonPendingOrderException(Id, Status.ToString());

        if (!_orderLines.Any(ol => ol.ProductId == productId))
            throw new OrderLineNotFoundException(Id, productId);

        Apply(new OrderLineRemoved
        {
            OrderId = Id,
            ProductId = productId
        });

        RecalculateTotals();
    }

    public void Confirm()
    {
        if (Status == OrderStatus.Cancelled)
            throw new CannotConfirmCancelledOrderException(Id);

        if (Status == OrderStatus.Rejected)
            throw new CannotConfirmRejectedOrderException(Id);

        if (!_orderLines.Any())
            throw new EmptyOrderException(Id);

        if (Status == OrderStatus.Confirmed)
            return; // Already confirmed

        Apply(new OrderConfirmed
        {
            OrderId = Id,
            ConfirmedAtUtc = DateTime.UtcNow
        });
    }

    public void Cancel(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Cancellation reason is required.", nameof(reason));

        if (Status == OrderStatus.Cancelled)
            return; // Already cancelled

        Apply(new OrderCancelled
        {
            OrderId = Id,
            Reason = reason,
            CancelledAtUtc = DateTime.UtcNow
        });
    }

    public void MarkAsPaid()
    {
        if (Status != OrderStatus.Confirmed)
            throw new InvalidOperationException($"Cannot mark order {Id} as paid because it is not confirmed.");

        if (Status == OrderStatus.Paid)
            return; // Already paid

        Apply(new OrderPaid
        {
            OrderId = Id,
            PaidAtUtc = DateTime.UtcNow
        });
    }

    public void Reject(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Rejection reason is required.", nameof(reason));

        if (Status == OrderStatus.Rejected)
            return; // Already rejected

        Apply(new OrderRejected
        {
            OrderId = Id,
            Reason = reason,
            RejectedAtUtc = DateTime.UtcNow
        });
    }

    private void RecalculateTotals()
    {
        SubTotal = _orderLines.Sum(ol => ol.LineTotal);
        TaxAmount = SubTotal * TaxRate;
        Total = SubTotal + TaxAmount;
    }

    protected override void When(object @event)
    {
        switch (@event)
        {
            case OrderPlaced e:
                Handle(e);
                break;
            case OrderLineAdded e:
                Handle(e);
                break;
            case OrderLineRemoved e:
                Handle(e);
                break;
            case OrderConfirmed e:
                Handle(e);
                break;
            case OrderCancelled e:
                Handle(e);
                break;
            case OrderPaid e:
                Handle(e);
                break;
            case OrderRejected e:
                Handle(e);
                break;
        }
    }

    private void Handle(OrderPlaced e)
    {
        Id = e.OrderId;
        CustomerId = e.CustomerId;
        CustomerSnapshot = e.CustomerSnapshot;
        ShippingAddress = e.ShippingAddress;
        BillingAddress = e.BillingAddress;
        Status = OrderStatus.Pending;
        PlacedAtUtc = e.PlacedAtUtc;
    }

    private void Handle(OrderLineAdded e)
    {
        _orderLines.Add(e.OrderLine);
        SubTotal = _orderLines.Sum(ol => ol.LineTotal);
        TaxAmount = SubTotal * TaxRate;
        Total = SubTotal + TaxAmount;
    }

    private void Handle(OrderLineRemoved e)
    {
        var orderLine = _orderLines.FirstOrDefault(ol => ol.ProductId == e.ProductId);
        if (orderLine != null)
            _orderLines.Remove(orderLine);

        SubTotal = _orderLines.Sum(ol => ol.LineTotal);
        TaxAmount = SubTotal * TaxRate;
        Total = SubTotal + TaxAmount;
    }

    private void Handle(OrderConfirmed e)
    {
        Status = OrderStatus.Confirmed;
        ConfirmedAtUtc = e.ConfirmedAtUtc;
    }

    private void Handle(OrderCancelled e)
    {
        Status = OrderStatus.Cancelled;
        CancelledAtUtc = e.CancelledAtUtc;
    }

    private void Handle(OrderPaid e)
    {
        Status = OrderStatus.Paid;
        PaidAtUtc = e.PaidAtUtc;
    }

    private void Handle(OrderRejected e)
    {
        Status = OrderStatus.Rejected;
        RejectedAtUtc = e.RejectedAtUtc;
    }
}
