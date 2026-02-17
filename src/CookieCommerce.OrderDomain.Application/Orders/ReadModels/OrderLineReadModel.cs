namespace CookieCommerce.OrderDomain.Application.Orders.ReadModels;

public class OrderLineReadModel
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}
