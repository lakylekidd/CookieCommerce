using MediatR;

namespace CookieCommerce.OrderDomain.Application.Orders.Commands;

/// <summary>
/// Command to reject an order (admin action).
/// </summary>
/// <param name="OrderId">The ID of the order to reject</param>
/// <param name="Reason">The reason for rejection</param>
public record RejectOrder(
    Guid OrderId,
    string Reason) : IRequest;
