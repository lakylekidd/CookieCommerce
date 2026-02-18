using MediatR;

namespace CookieCommerce.OrderDomain.Application.Orders.Commands;

/// <summary>
/// Command to add a product line item to a pending order.
/// Product details (name and price) are fetched from the Catalog service at the time of adding.
/// </summary>
/// <param name="OrderId">The ID of the order</param>
/// <param name="ProductId">The ID of the product to add</param>
/// <param name="Quantity">The quantity of the product</param>
public record AddOrderLine(
    Guid OrderId,
    Guid ProductId,
    int Quantity) : IRequest;
