using MediatR;

namespace CookieCommerce.CustomerDomain.Application.Customers.Commands;

public class AddCustomerAddress(Guid customerId,
    string label,
    string street,
    string city,
    string state,
    string postalCode,
    string country,
    bool isPrimary) : IRequest
{
    public Guid CustomerId { get; } = customerId;
    public string Label { get; } = label;
    public string Street { get; } = street;
    public string City { get; } = city;
    public string State { get; } = state;
    public string PostalCode { get; } = postalCode;
    public string Country { get; } = country;
    public bool IsPrimary { get; } = isPrimary;
}
