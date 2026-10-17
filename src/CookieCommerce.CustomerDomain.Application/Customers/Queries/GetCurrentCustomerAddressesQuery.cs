using CookieCommerce.CustomerDomain.Application.Customers.ReadModels;
using MediatR;

namespace CookieCommerce.CustomerDomain.Application.Customers.Queries;

public sealed record GetCurrentCustomerAddressesQuery(Guid CustomerId) : IRequest<IReadOnlyList<CustomerAddressReadModel>>;

public sealed record GetCurrentCustomerDefaultAddressQuery(Guid CustomerId) : IRequest<CustomerAddressReadModel?>;
