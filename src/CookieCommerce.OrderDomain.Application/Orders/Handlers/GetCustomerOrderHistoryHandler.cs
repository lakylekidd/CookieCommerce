using CookieCommerce.OrderDomain.Application.Orders.Queries;
using CookieCommerce.OrderDomain.Application.Orders.ReadModels;
using Marten;
using MediatR;

namespace CookieCommerce.OrderDomain.Application.Orders.Handlers;

public class GetCustomerOrderHistoryHandler(IQuerySession query) : IRequestHandler<GetCustomerOrderHistory, IReadOnlyList<CustomerOrderHistoryReadModel>>
{
    public async Task<IReadOnlyList<CustomerOrderHistoryReadModel>> Handle(
        GetCustomerOrderHistory request,
        CancellationToken cancellationToken)
    {
        var orders = await query
            .Query<CustomerOrderHistoryReadModel>()
            .Where(o => o.CustomerId == request.CustomerId)
            .OrderByDescending(o => o.PlacedAtUtc)
            .ToListAsync(cancellationToken);

        return orders;
    }
}
