using MediatR;

namespace CookieCommerce.CustomerDomain.Application.Customers.Commands;

public sealed record SetCustomerDefaultAddress(Guid CustomerId, Guid AddressId, DateTimeOffset ChangedAtUtc) : IRequest;
