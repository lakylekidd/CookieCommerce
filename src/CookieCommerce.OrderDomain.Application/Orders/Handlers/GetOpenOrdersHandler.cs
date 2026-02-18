using CookieCommerce.OrderDomain.Application.Orders.Queries;
using CookieCommerce.OrderDomain.Application.Orders.ReadModels;
using Marten;
using MediatR;

namespace CookieCommerce.OrderDomain.Application.Orders.Handlers;

public class GetOpenOrdersHandler(IQuerySession query) : IRequestHandler<GetOpenOrders, IReadOnlyList<OpenOrderReadModel>>
{
    public async Task<IReadOnlyList<OpenOrderReadModel>> Handle(
        GetOpenOrders request,
        CancellationToken cancellationToken)
    {
        var orders = await query
            .Query<OpenOrderReadModel>()
            .OrderByDescending(o => o.PlacedAtUtc)
            .ToListAsync(cancellationToken);

        return orders;
    }
}
