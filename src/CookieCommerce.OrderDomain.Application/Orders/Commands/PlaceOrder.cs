using MediatR;

namespace CookieCommerce.OrderDomain.Application.Orders.Commands;

/// <summary>
/// Command to place a new order for a customer.
/// </summary>
/// <param name="CustomerId">The ID of the customer placing the order</param>
public record PlaceOrder(Guid CustomerId) : IRequest<Guid>;
