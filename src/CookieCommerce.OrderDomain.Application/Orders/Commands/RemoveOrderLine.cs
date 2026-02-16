using MediatR;

namespace CookieCommerce.OrderDomain.Application.Orders.Commands;

/// <summary>
/// Command to remove a product line item from a pending order.
/// </summary>
/// <param name="OrderId">The ID of the order</param>
/// <param name="ProductId">The ID of the product to remove</param>
public record RemoveOrderLine(
    Guid OrderId,
    Guid ProductId) : IRequest;
