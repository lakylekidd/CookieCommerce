using CookieCommerce.OrderDomain.Application.Orders.ReadModels;
using MediatR;

namespace CookieCommerce.OrderDomain.Application.Orders.Queries;

public record GetOpenOrders() : IRequest<IReadOnlyList<OpenOrderReadModel>>;
