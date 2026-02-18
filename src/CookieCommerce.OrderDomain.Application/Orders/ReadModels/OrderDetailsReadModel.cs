using CookieCommerce.OrderDomain.Domain.ValueObjects;

namespace CookieCommerce.OrderDomain.Application.Orders.ReadModels;

public class OrderDetailsReadModel
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public OrderStatus Status { get; set; }
    public List<OrderLineReadModel> OrderLines { get; set; } = new();
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal Total { get; set; }
    public DateTime PlacedAtUtc { get; set; }
    public DateTime? StatusDate { get; set; }
    public Dictionary<string, DateTime> StatusHistory { get; set; } = new();
}
