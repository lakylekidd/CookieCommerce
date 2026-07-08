using CookieCommerce.CustomerDomain.Application.Customers.Commands;

namespace CookieCommerce.CustomerDomain.Api.Models.Customers;

public static class CustomerMapper
{
    public static CreateCustomer ToCommand(this CreateCustomerRequest request) =>
        new CreateCustomer(request.Email, request.FirstName, request.LastName, request.Gender, request.DateOfBirth);

    public static AddCustomerAddress ToCommand(this AddCustomerAddressRequest request, Guid customerId) =>
        new(
            customerId,
            Guid.CreateVersion7(),
            request.Icon,
            request.Label,
            request.FirstName,
            request.LastName,
            request.Street,
            request.HouseNumber,
            request.HouseNumberAddition,
            request.City,
            request.PostalCode,
            request.Province,
            request.Country,
            request.Phone,
            request.IsDefault,
            DateTimeOffset.UtcNow);
}
