using CookieCommerce.OrderDomain.Application.Orders.Queries;
using CookieCommerce.OrderDomain.Application.Orders.ReadModels;
using Marten;
using MediatR;

namespace CookieCommerce.OrderDomain.Application.Orders.Handlers;

public class GetOpenOrdersHandler(IQuerySession query) : IRequestHandler<GetOpenOrders, IReadOnlyList<OpenOrdersReadModel>>
{
    private readonly IQuerySession _query = query;

    public async Task<IReadOnlyList<OpenOrdersReadModel>> Handle(
        GetOpenOrders request,
        CancellationToken cancellationToken)
    {
        var orders = await _query
            .Query<OpenOrdersReadModel>()
            .OrderByDescending(o => o.PlacedAtUtc)
            .ToListAsync(cancellationToken);

        return orders;
    }
}
