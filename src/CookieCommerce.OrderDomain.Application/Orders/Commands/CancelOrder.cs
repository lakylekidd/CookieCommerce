using MediatR;

namespace CookieCommerce.OrderDomain.Application.Orders.Commands;

/// <summary>
/// Command to cancel an order.
/// </summary>
/// <param name="OrderId">The ID of the order to cancel</param>
/// <param name="Reason">The reason for cancellation</param>
public record CancelOrder(
    Guid OrderId,
    string Reason) : IRequest;
