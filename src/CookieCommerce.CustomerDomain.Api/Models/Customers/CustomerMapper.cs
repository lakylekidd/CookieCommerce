using CookieCommerce.CustomerDomain.Application.Customers.Commands;

namespace CookieCommerce.CustomerDomain.Api.Models.Customers;

public static class CustomerMapper
{
    public static CreateCustomer ToCommand(this CreateCustomerRequest request) =>
        new CreateCustomer(request.Email, request.FirstName, request.LastName, request.Gender, request.DateOfBirth);
}
