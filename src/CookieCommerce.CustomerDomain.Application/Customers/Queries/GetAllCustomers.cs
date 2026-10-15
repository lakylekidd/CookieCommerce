using CookieCommerce.CustomerDomain.Application.Customers.ReadModels;
using MediatR;

namespace CookieCommerce.CustomerDomain.Application.Customers.Queries;

public sealed record GetAllCustomers : IRequest<IReadOnlyList<CustomerReadModel>>;
