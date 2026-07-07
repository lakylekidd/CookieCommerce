using MediatR;

namespace CookieCommerce.CustomerDomain.Application.Customers.Commands;

public class AddCustomerAddress(
    Guid customerId,
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
    bool isDefault) : IRequest
{
    public Guid CustomerId { get; } = customerId;
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
}
