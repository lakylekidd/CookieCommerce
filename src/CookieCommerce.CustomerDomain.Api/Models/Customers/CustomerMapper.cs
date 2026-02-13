using CookieCommerce.CustomerDomain.Application.Customers.Commands;

namespace CookieCommerce.CustomerDomain.Api.Models.Customers;

public static class CustomerMapper
{
    public static CreateCustomer ToCommand(this CreateCustomerRequest request) =>
        new CreateCustomer(request.Email, request.FirstName, request.LastName, request.Gender, request.DateOfBirth);

    public static AddCustomerAddress ToCommand(this AddCustomerAddressRequest request, Guid customerId) =>
        new AddCustomerAddress(customerId, request.Label, request.Street, request.City, request.State, request.PostalCode, request.Country, request.IsPrimary);
}
