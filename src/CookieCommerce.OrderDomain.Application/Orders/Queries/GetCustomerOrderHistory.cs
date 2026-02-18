using CookieCommerce.OrderDomain.Application.Orders.ReadModels;
using MediatR;

namespace CookieCommerce.OrderDomain.Application.Orders.Queries;

public record GetCustomerOrderHistory(Guid CustomerId) : IRequest<IReadOnlyList<CustomerOrderHistoryReadModel>>;
