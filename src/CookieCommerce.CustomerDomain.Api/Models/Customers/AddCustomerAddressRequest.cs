using System.ComponentModel.DataAnnotations;

namespace CookieCommerce.CustomerDomain.Api.Models.Customers;

public class AddCustomerAddressRequest
{
    public string? Icon { get; set; }
    [Required] public string Label { get; set; } = string.Empty;
    [Required] public string FirstName { get; set; } = string.Empty;
    [Required] public string LastName { get; set; } = string.Empty;
    [Required] public string Street { get; set; } = string.Empty;
    [Required] public string HouseNumber { get; set; } = string.Empty;
    public string? HouseNumberAddition { get; set; }
    [Required] public string City { get; set; } = string.Empty;
    [Required] public string PostalCode { get; set; } = string.Empty;
    [Required] public string Province { get; set; } = string.Empty;
    [Required] public string Country { get; set; } = string.Empty;
    [Required] public string Phone { get; set; } = string.Empty;
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
