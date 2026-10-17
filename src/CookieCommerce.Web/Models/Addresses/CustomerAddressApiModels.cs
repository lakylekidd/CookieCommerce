namespace CookieCommerce.Web.Models.Addresses;

public sealed record CustomerAddressApiResponse(
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

public sealed record AddCustomerAddressApiRequest(
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
