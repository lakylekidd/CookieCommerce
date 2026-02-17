using CookieCommerce.OrderDomain.Domain.ValueObjects;

namespace CookieCommerce.OrderDomain.Application.Orders.ReadModels;

public class OpenOrdersReadModel
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public OrderStatus Status { get; set; }
    public decimal Total { get; set; }
    public DateTime PlacedAtUtc { get; set; }
    public DateTime? ConfirmedAtUtc { get; set; }
}
