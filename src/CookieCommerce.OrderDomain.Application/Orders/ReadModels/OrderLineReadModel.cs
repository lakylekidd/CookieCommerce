using CookieCommerce.OrderDomain.Application.Orders.ValueObjects;

namespace CookieCommerce.OrderDomain.Application.Orders.ReadModels;

public class OrderLineReadModel
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public PriceInfo PriceInfo { get; set; } = null!;
    public decimal LineTotal { get; set; }
}
