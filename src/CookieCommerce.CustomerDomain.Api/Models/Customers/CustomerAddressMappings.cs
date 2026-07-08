using CookieCommerce.CustomerDomain.Application.Customers.ReadModels;

namespace CookieCommerce.CustomerDomain.Api.Models.Customers;

public static class CustomerAddressMappings
{
    public static CustomerAddressResponse ToResponse(this CustomerAddressReadModel address) =>
        new(
            address.AddressId,
            address.Index,
            address.Icon,
            address.Label,
            address.FirstName,
            address.LastName,
            address.Street,
            address.HouseNumber,
            address.HouseNumberAddition,
            address.City,
            address.PostalCode,
            address.Province,
            address.Country,
            address.Phone,
            address.IsDefault);

    public static IReadOnlyList<CustomerAddressResponse> ToResponses(this IEnumerable<CustomerAddressReadModel> addresses) =>
        addresses.Select(address => address.ToResponse()).ToList();
}
