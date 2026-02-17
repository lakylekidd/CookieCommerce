using CookieCommerce.OrderDomain.Application.Orders.ReadModels;
using MediatR;

namespace CookieCommerce.OrderDomain.Application.Orders.Queries;

public record GetOrderById(Guid OrderId) : IRequest<OrderDetailsReadModel?>;
