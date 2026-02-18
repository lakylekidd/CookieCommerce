namespace CookieCommerce.OrderDomain.Domain.ValueObjects;

/// <summary>
/// Represents a shipping or billing address for an order.
/// This is a snapshot of address information at the time the order was placed.
/// </summary>
public class OrderAddress
{
    public string Street { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string State { get; init; } = string.Empty;
    public string PostalCode { get; init; } = string.Empty;
    public string Country { get; init; } = string.Empty;

    public OrderAddress() { }

    public OrderAddress(string street, string city, string state, string postalCode, string country)
    {
        Street = street;
        City = city;
        State = state;
        PostalCode = postalCode;
        Country = country;
    }
}
