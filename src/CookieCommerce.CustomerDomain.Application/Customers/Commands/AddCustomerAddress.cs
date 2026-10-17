using CookieCommerce.CustomerDomain.Application.Customers.ReadModels;
using MediatR;

namespace CookieCommerce.CustomerDomain.Application.Customers.Commands;

public class AddCustomerAddress(
    Guid customerId,
    Guid addressId,
    string? icon,
    string label,
    string firstName,
    string lastName,
    string street,
    string houseNumber,
    string? houseNumberAddition,
    string city,
    string postalCode,
    string province,
    string country,
    string phone,
    bool isDefault,
    DateTimeOffset createdAtUtc) : IRequest<CustomerAddressReadModel>
{
    public Guid CustomerId { get; } = customerId;
    public Guid AddressId { get; } = addressId;
    public string? Icon { get; } = icon;
    public string Label { get; } = label;
    public string FirstName { get; } = firstName;
    public string LastName { get; } = lastName;
    public string Street { get; } = street;
    public string HouseNumber { get; } = houseNumber;
    public string? HouseNumberAddition { get; } = houseNumberAddition;
    public string City { get; } = city;
    public string PostalCode { get; } = postalCode;
    public string Province { get; } = province;
    public string Country { get; } = country;
    public string Phone { get; } = phone;
    public bool IsDefault { get; } = isDefault;
    public DateTimeOffset CreatedAtUtc { get; } = createdAtUtc;
}
