using MediatR;

namespace CookieCommerce.OrderDomain.Application.Orders.Commands;

/// <summary>
/// Command to mark a confirmed order as paid.
/// </summary>
/// <param name="OrderId">The ID of the order to mark as paid</param>
public record MarkOrderAsPaid(Guid OrderId) : IRequest;
