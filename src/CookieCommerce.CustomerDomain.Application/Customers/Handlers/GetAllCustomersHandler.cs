using CookieCommerce.CustomerDomain.Application.Customers.Queries;
using CookieCommerce.CustomerDomain.Application.Customers.ReadModels;
using Marten;
using MediatR;

namespace CookieCommerce.CustomerDomain.Application.Customers.Handlers;

public sealed class GetAllCustomersHandler(IQuerySession session) : IRequestHandler<GetAllCustomers, IReadOnlyList<CustomerReadModel>>
{
    public async Task<IReadOnlyList<CustomerReadModel>> Handle(GetAllCustomers request, CancellationToken cancellationToken) =>
        await session.Query<CustomerReadModel>().OrderBy(c => c.LastName).ThenBy(c => c.FirstName).ToListAsync(cancellationToken);
}
