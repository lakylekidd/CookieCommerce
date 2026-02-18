using CookieCommerce.OrderDomain.Application.Orders.Queries;
using CookieCommerce.OrderDomain.Application.Orders.ReadModels;
using Marten;
using MediatR;

namespace CookieCommerce.OrderDomain.Application.Orders.Handlers;

public class GetOrderByIdHandler(IQuerySession query) : IRequestHandler<GetOrderById, OrderDetailsReadModel?>
{
    public async Task<OrderDetailsReadModel?> Handle(
        GetOrderById request,
        CancellationToken cancellationToken) => await query.LoadAsync<OrderDetailsReadModel>(request.OrderId, cancellationToken);
}
