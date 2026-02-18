using CookieCommerce.OrderDomain.Domain.ValueObjects;
using MediatR;

namespace CookieCommerce.OrderDomain.Application.Orders.Commands;

/// <summary>
/// Command to place a new order for a customer.
/// </summary>
/// <param name="CustomerId">The ID of the customer placing the order</param>
/// <param name="ShippingAddress">The shipping address for the order</param>
/// <param name="BillingAddress">The billing address for the order</param>
public record PlaceOrder(
    Guid CustomerId,
    OrderAddress ShippingAddress,
    OrderAddress BillingAddress) : IRequest<Guid>;
