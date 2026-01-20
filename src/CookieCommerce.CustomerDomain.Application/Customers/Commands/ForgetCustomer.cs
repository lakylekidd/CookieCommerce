using MediatR;

namespace CookieCommerce.CustomerDomain.Application.Customers.Commands;

public record ForgetCustomer(Guid CustomerId) : IRequest;