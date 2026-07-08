namespace CookieCommerce.CustomerDomain.Api.Models.Customers;

public class AddCustomerAddressRequest
{
    public string? Icon { get; set; }
    public string Label { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string HouseNumber { get; set; } = string.Empty;
    public string? HouseNumberAddition { get; set; }
    public string City { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Province { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public bool IsDefault { get; set; } = false;

    public string State
    {
        get => Province;
        set => Province = value;
    }

    public bool IsPrimary
    {
        get => IsDefault;
        set => IsDefault = value;
    }
}
