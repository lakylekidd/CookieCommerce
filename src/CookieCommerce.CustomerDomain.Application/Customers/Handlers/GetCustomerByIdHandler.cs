using CookieCommerce.CustomerDomain.Application.Customers.Queries;
using CookieCommerce.CustomerDomain.Application.Customers.ReadModels;
using Marten;
using MediatR;

namespace CookieCommerce.CustomerDomain.Application.Customers.Handlers;

internal class GetCustomerByIdHandler(IQuerySession query) : IRequestHandler<GetCustomerById, CustomerReadModel?>
{
    private readonly IQuerySession _query = query;

    public async Task<CustomerReadModel?> Handle(
        GetCustomerById request,
        CancellationToken cancellationToken) => await _query.LoadAsync<CustomerReadModel>(request.CustomerId, cancellationToken);
}
