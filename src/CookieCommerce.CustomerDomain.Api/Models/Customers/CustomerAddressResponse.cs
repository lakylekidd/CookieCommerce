namespace CookieCommerce.CustomerDomain.Api.Models.Customers;

public sealed record CustomerAddressResponse(
    Guid Id,
    int Index,
    string? Icon,
    string Label,
    string FirstName,
    string LastName,
    string Street,
    string HouseNumber,
    string? HouseNumberAddition,
    string City,
    string PostalCode,
    string Province,
    string Country,
    string Phone,
    bool IsDefault);
