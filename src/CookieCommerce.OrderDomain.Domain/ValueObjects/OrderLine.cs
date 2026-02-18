namespace CookieCommerce.OrderDomain.Domain.ValueObjects;

public class OrderLine
{
    public Guid ProductId { get; }
    public ProductSnapshot ProductSnapshot { get; }
    public int Quantity { get; }
    public decimal UnitPrice { get; }
    public decimal LineTotal => Quantity * UnitPrice;

    public OrderLine(Guid productId, ProductSnapshot productSnapshot, int quantity, decimal unitPrice)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than 0.", nameof(quantity));

        if (unitPrice <= 0)
            throw new ArgumentException("Unit price must be greater than 0.", nameof(unitPrice));

        ProductId = productId;
        ProductSnapshot = productSnapshot ?? throw new ArgumentNullException(nameof(productSnapshot));
        Quantity = quantity;
        UnitPrice = unitPrice;
    }
}
