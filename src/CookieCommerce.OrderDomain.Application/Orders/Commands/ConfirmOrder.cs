using MediatR;

namespace CookieCommerce.OrderDomain.Application.Orders.Commands;

/// <summary>
/// Command to confirm a pending order, locking it for fulfillment.
/// </summary>
/// <param name="OrderId">The ID of the order to confirm</param>
public record ConfirmOrder(Guid OrderId) : IRequest;
