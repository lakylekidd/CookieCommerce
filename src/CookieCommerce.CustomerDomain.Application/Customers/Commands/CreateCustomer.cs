using CookieCommerce.CustomerDomain.Domain.ValueObjects;
using MediatR;

namespace CookieCommerce.CustomerDomain.Application.Customers.Commands;

public record CreateCustomer(
    string Email,
    string FirstName,
    string LastName,
    Gender Gender,
    DateTime? DateOfBirth) : IRequest<Guid>;
