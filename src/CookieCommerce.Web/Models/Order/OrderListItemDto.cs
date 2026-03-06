using System.Numerics;

namespace CookieCommerce.Web.Models.Order;

/// <summary>
/// This class represents a data transfer object (DTO) for an order list item, 
/// which includes details about the order. It is used to display an order
/// in a list format, typically in an order history or order summary page.
/// </summary>
public class OrderListItemDto
{
    public Guid Id { get; set; }
    public string OrderReference { get; set; } = string.Empty;
    public DateTime PlacedOn { get; set; }
    public ICollection<OrderListItemProductDto> Products { get; set; } = [];
    public OrderStatusDto Status { get; set; } = new();
    public string CurrencySymbol { get; set; } = "$";
    public decimal Total { get; set; }
    public int RewardPointsEarned => (int)(Total * 10);
}

public class OrderListItemProductDto
{
    public string ProductImageUrl { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
}