using CookieCommerce.CustomerDomain.Application.Customers.ReadModels;
using MediatR;

namespace CookieCommerce.CustomerDomain.Application.Customers.Queries;

public record GetCustomerById(Guid CustomerId) : IRequest<CustomerReadModel>;
