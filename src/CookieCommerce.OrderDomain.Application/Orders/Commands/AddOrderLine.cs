using MediatR;

namespace CookieCommerce.OrderDomain.Application.Orders.Commands;

/// <summary>
/// Command to add a product line item to a pending order.
/// </summary>
/// <param name="OrderId">The ID of the order</param>
/// <param name="ProductId">The ID of the product to add</param>
/// <param name="Quantity">The quantity of the product</param>
/// <param name="UnitPrice">The unit price of the product at the time of ordering</param>
public record AddOrderLine(
    Guid OrderId,
    Guid ProductId,
    int Quantity,
    decimal UnitPrice) : IRequest;
