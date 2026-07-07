using CookieCommerce.CustomerDomain.Application.Customers.ReadModels;

namespace CookieCommerce.CustomerDomain.Api.Models.Customers;

public static class CustomerAddressMappings
{
    public static CustomerAddressResponse ToResponse(this CustomerAddressReadModel address, int index) =>
        new(
            address.AddressId,
            index,
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
}
